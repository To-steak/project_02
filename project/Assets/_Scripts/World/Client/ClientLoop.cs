using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Project02.Combat;
using Project02.Core;
using Project02.Enemy;
using Project02.Player;
using Project02.Service;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.InputSystem;
#endif

namespace Project02.World
{
    // World 진입점: ClientWorld를 소유하고, 시스템을 정해진 순서로 돌린다
    // 호스트: 같은 프레임에 ClientLoop(입력 → 예측 → 입력 전송)가 ServerLoop(소비 → 시뮬 → 송신)보다 먼저
    [DefaultExecutionOrder(-200)]
    public class ClientLoop : MonoBehaviour
    {
        public static ClientLoop Instance { get; private set; }

        private readonly ClientWorld _world = new();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private readonly System.Text.StringBuilder _clockLog = new();
        private bool _clockLogging;
#endif

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            NetworkManager.Singleton.OnClientStarted += OnClientStarted;
            NetworkManager.Singleton.OnClientStopped += OnClientStopped;
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientStarted -= OnClientStarted;
                NetworkManager.Singleton.OnClientStopped -= OnClientStopped;
            }

            if (Instance == this) Instance = null;
        }

        public void Register(PlayerOwnerAuthoring owner)
        {
            _world.Local = owner;
            _world.Clock = ClockSystem.Create();
            _world.AckTick = -1;
            _world.Input = default;
            _world.Camera = default;
            _world.PredictedShots.Clear();
            _world.PredictedImpacts.Clear();

            InputService.Instance.Actions.Enable();
        }

        public void Unregister(PlayerOwnerAuthoring owner)
        {
            if (_world.Local != owner) return;

            _world.Local = null;
            _world.Clock = ClockSystem.Create();   // RttTicks = -1: 로컬 플레이어가 없으면 예상 lag를 재지 않는다 (외삽 끔)
            _world.PredictedShots.Clear();
            _world.PredictedImpacts.Clear();

            InputService input = InputService.Instance;
            if (input != null)
            {
                input.Actions.Disable();
            }
        }

        public void Register(PlayerRemoteAuthoring player) => _world.Players[player.NetworkObjectId] = player;
        public void Unregister(PlayerRemoteAuthoring player) => _world.Players.Remove(player.NetworkObjectId);

        // 종류 목록(Enemies)과 피격 묶음(HitTargets)은 여기서만 함께 바꾼다 (규칙 11)
        public void Register(EnemyRemoteAuthoring enemy, float radius, float height)
        {
            _world.Enemies[enemy.NetworkObjectId] = enemy;
            _world.HitTargets.Add(new ClientHitTarget(enemy.Snapshots, radius, height));
        }
        public void Unregister(EnemyRemoteAuthoring enemy)
        {
            _world.Enemies.Remove(enemy.NetworkObjectId);

            // 같은 Snapshots 참조로 찾는다
            List<ClientHitTarget> list = _world.HitTargets;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(list[i].Snapshots, enemy.Snapshots)) list.RemoveAt(i);
            }
        }

        private void Update()
        {
            ClientWorld world = _world;
            if (world.Local == null) return;

            float frameTime = Time.deltaTime;
            InputService input = InputService.Instance;

            // 1. 입력 샘플
            InputSampleSystem.Sample(ref world.Input, input.Actions);

            // 2. 카메라
            CameraSystem.Update(world, input.MouseSensitivity, frameTime);

            // 3. 리컨사일 (뒤처졌으면 시계 재동기화). RTT는 직접 잰 값, 표본 전에는 Transport 값
            NetworkManager network = NetworkManager.Singleton;
            float now = Time.realtimeSinceStartup;
            float transportRttMs = network.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.ServerClientId);
            ReconcileSystem.Process(world, transportRttMs / 1000.0f / SimulationClock.DeltaTime, now);

            // 4~5. 예측 시계 → 예측
            int ticks = ClockSystem.Advance(ref world.Clock, frameTime);
            for (int i = 0; i < ticks; i++)
            {
                PredictSystem.Tick(world, SimulationClock.DeltaTime, now);
                world.Clock.Tick++;
            }

            // 6. 피격 예측: 예측 탄을 이 프레임의 예측 틱 수만큼 진행 → world.PredictedImpacts (연출은 LateUpdate)
            if (ticks > 0) ShotPredictSystem.Tick(world, ticks, SimulationClock.DeltaTime);

            // 6-1. 입력 전송 (예측 루프 뒤 프레임당 1회. 호스트에서는 이 프레임의 ServerLoop가 바로 받는다)
            if (ticks > 0) InputSendSystem.Flush(world);

            // 7. 루트 transform 출력 (리컨사일·예측 루프 밖에서 1회)
            PlayerOwnerAuthoring owner = world.Local;
            PlayerStepUtil.WriteTransform(owner.transform, owner.Simulation);

            // 8. (디버그) 지연 측정. F9: 예측 켜기/끄기
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Keyboard.current != null && Keyboard.current.f9Key.wasPressedThisFrame)
            {
                LatencySystem.Toggle(world);
            }
            LatencySystem.Measure(world, owner.Simulation.State.Movement.Position, Time.realtimeSinceStartup);

            if (Keyboard.current != null && Keyboard.current.f10Key.wasPressedThisFrame)
            {
                _clockLogging = !_clockLogging;
                if (_clockLogging)
                {
                    _clockLog.Clear().Append("time,buffer,interval_ms,rtt_ms\n");
                }
                else
                {
                    string path = System.IO.Path.Combine(Application.persistentDataPath, "clock_log.csv");
                    System.IO.File.WriteAllText(path, _clockLog.ToString());
                    Debug.Log($"clock log → {path}");
                }
            }

            if (_clockLogging)
            {
                ClientClock clock = world.Clock;
                _clockLog.Append($"{now:F3},{clock.SmoothedBuffer:F2},{clock.Interval * 1000.0f:F2},{clock.RttTicks * SimulationClock.DeltaTime * 1000.0f:F0}\n");
            }
#endif
        }

        private void LateUpdate()
        {
            ClientWorld world = _world;
            float frameTime = Time.deltaTime;

            // 9. 보간 시계 → 그린 시점 (ViewTick의 정본. 다음 프레임 Update의 입력·피격 예측도 이 값을 쓴다)
            InterpolationSystem.Advance(ref world.Interp, frameTime);
            ViewTickSystem.Update(world);

            // 10. 원격 엔티티 표시 (로컬 플레이어 유무와 무관)
            EnemyViewSystem.Present(world);
            RemotePlayerViewSystem.Present(world, frameTime);

            // 11. 로컬 플레이어 표시
            if (world.Local == null) return;
            ViewSystem.Present(world, frameTime);

            // 12. 예측 피격 연출: PredictedImpacts → WeaponView → ProjectileVisualService
            ImpactViewSystem.Present(world);
        }

        private void OnClientStarted()
        {
            _world.Interp = default;
            _world.Clock = ClockSystem.Create();   // 로컬 플레이어 등록 전에도 RttTicks = -1 (외삽 끔)
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(SnapshotSystem.MESSAGE, OnSnapshot);
        }

        private void OnClientStopped(bool wasHost)
        {
            NetworkManager.Singleton.CustomMessagingManager?.UnregisterNamedMessageHandler(SnapshotSystem.MESSAGE);
            _world.Enemies.Clear();
            _world.HitTargets.Clear();
            _world.Players.Clear();
            _world.PredictedShots.Clear();
            _world.PredictedImpacts.Clear();
        }

        private void OnSnapshot(ulong senderId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out int tick);
            InterpolationSystem.Receive(ref _world.Interp, tick);

            reader.ReadValueSafe(out ushort enemyCount);
            for (int i = 0; i < enemyCount; i++)
            {
                reader.ReadNetworkSerializable(out EnemySnapshot snapshot);
                if (_world.Enemies.TryGetValue(snapshot.ObjectId, out EnemyRemoteAuthoring enemy))
                {
                    enemy.Snapshots.SetValue(tick, snapshot);
                }
            }

            reader.ReadValueSafe(out ushort playerCount);
            for (int i = 0; i < playerCount; i++)
            {
                reader.ReadNetworkSerializable(out PlayerSnapshot snapshot);
                if (_world.Players.TryGetValue(snapshot.ObjectId, out PlayerRemoteAuthoring player))
                {
                    player.Snapshots.SetValue(tick, snapshot);
                }
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (_world.Local == null) return;
            GUIStyle style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperRight };
            style.normal.textColor = Color.black;
            float x = Screen.width - 310f;

            GUI.Label(new Rect(x, 150, 300, 20), $"tick: {_world.Clock.Tick}", style);
            GUI.Label(new Rect(x, 170, 300, 20), $"buffer: {_world.Clock.SmoothedBuffer:F2}", style);
            GUI.Label(new Rect(x, 190, 300, 20), $"interval: {_world.Clock.Interval * 1000.0f:F2}ms", style);
            GUI.Label(new Rect(x, 210, 300, 20), $"latest: {_world.Interp.LatestTick} render: {_world.Interp.RenderTick:F2}", style);
            GUI.Label(new Rect(x, 230, 300, 20), $"delay: {_world.Interp.LatestTick - _world.Interp.RenderTick:F2} ticks", style);

            LatencyProbe probe = _world.Probe;
            LatencySystem.Summarize(probe.Samples, out float median, out float p95, out float max);
            GUI.Label(new Rect(x, 250, 300, 20), $"predict: {(probe.PredictionEnabled ? "ON" : "OFF")} (F9)  n={probe.Samples.Count}", style);
            GUI.Label(new Rect(x, 270, 300, 20), $"delay med {median:F1} / p95 {p95:F1} / max {max:F1} ms", style);
            GUI.Label(new Rect(x, 290, 300, 20), $"rtt (measured): {_world.Clock.RttTicks * SimulationClock.DeltaTime * 1000.0f:F0}ms", style);
            GUI.Label(new Rect(x, 310, 300, 20), $"still {Time.realtimeSinceStartup - probe.StillSince:F2}s  rejected {probe.Rejected}", style);
            GUI.Label(new Rect(x, 330, 300, 20), $"lag (expected): {_world.ExpectedLag:F2} / max {RewindTarget.MAX_LAG_TICKS} ticks", style);
            GUI.Label(new Rect(x, 350, 300, 20), $"extrapolation: {_world.ViewExtrapolation:F2} ticks  view: {_world.ViewTick:F2}", style);
        }
#endif
    }
}