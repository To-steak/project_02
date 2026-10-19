using Project02.Core;
using Project02.Enemy;
using Project02.Player;
using Project02.Server;
using Project02.Service;
using Unity.Netcode;
using UnityEngine;

namespace Project02.Client
{
    // World 진입점: ClientWorld를 소유하고, 시스템을 정해진 순서로 돌린다
    // 호스트: 같은 프레임에 ClientLoop(입력 → 예측 → 입력 전송)가 ServerLoop(소비 → 시뮬 → 송신)보다 먼저
    [DefaultExecutionOrder(-200)]
    public class ClientLoop : MonoBehaviour
    {
        public static ClientLoop Instance { get; private set; }

        private readonly ClientWorld _world = new();

        private void Awake() => Instance = this;

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
            _world.Input = default;
            _world.Camera = default;

            InputService.Instance.Actions.Enable();
        }

        public void Unregister(PlayerOwnerAuthoring owner)
        {
            if (_world.Local != owner) return;

            _world.Local = null;

            InputService input = InputService.Instance;
            if (input != null)
            {
                input.Actions.Disable();
            }
        }

        public void Register(PlayerRemoteAuthoring player) => _world.Players[player.NetworkObjectId] = player;
        public void Unregister(PlayerRemoteAuthoring player) => _world.Players.Remove(player.NetworkObjectId);
        public void Register(EnemyRemoteAuthoring enemy) => _world.Enemies[enemy.NetworkObjectId] = enemy;
        public void Unregister(EnemyRemoteAuthoring enemy) => _world.Enemies.Remove(enemy.NetworkObjectId);

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

            // 3. 리컨사일 (뒤처졌으면 시계 재동기화)
            NetworkManager network = NetworkManager.Singleton;
            float rttMs = network.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.ServerClientId);
            ReconcileSystem.Process(world, rttMs / 1000.0f / SimulationClock.DeltaTime);

            // 4~5. 예측 시계 → 예측
            int ticks = ClockSystem.Advance(ref world.Clock, frameTime);
            for (int i = 0; i < ticks; i++)
            {
                PredictSystem.Tick(world, SimulationClock.DeltaTime);
                world.Clock.Tick++;
            }

            // 6. 루트 transform 출력 (리컨사일·예측 루프 밖에서 1회)
            PlayerOwnerAuthoring owner = world.Local;
            PlayerStepUtil.WriteTransform(owner.transform, owner.Simulation);
        }

        private void LateUpdate()
        {
            ClientWorld world = _world;
            float frameTime = Time.deltaTime;

            // 7. 보간 시계
            InterpolationSystem.Advance(ref world.Interp, frameTime);

            // 8. 원격 엔티티 표시 (로컬 플레이어 유무와 무관)
            EnemyViewSystem.Present(world);
            RemotePlayerViewSystem.Present(world, frameTime);

            // 9. 로컬 플레이어 표시
            if (world.Local == null) return;
            ViewSystem.Present(world, frameTime);
        }

        private void OnClientStarted()
        {
            _world.Interp = default;
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(SnapshotSystem.MESSAGE, OnSnapshot);
        }

        private void OnClientStopped(bool wasHost)
        {
            NetworkManager.Singleton.CustomMessagingManager?.UnregisterNamedMessageHandler(SnapshotSystem.MESSAGE);
            _world.Enemies.Clear();
            _world.Players.Clear();
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
        }
#endif
    }
}