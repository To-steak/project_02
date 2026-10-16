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
    public class ClientSimulation : MonoBehaviour
    {
        public static ClientSimulation Instance { get; private set; }

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

        public void Register(PlayerOwner owner)
        {
            _world.Local = owner;
            _world.Clock = ClockSystem.Create();
            _world.Input = default;
            _world.Camera = default;

            InputService.Instance.Actions.Enable();
        }

        public void Unregister(PlayerOwner owner)
        {
            if (_world.Local != owner) return;

            _world.Local = null;

            InputService input = InputService.Instance;
            if (input != null)
            {
                input.Actions.Disable();
            }
        }

        public void Register(EnemyRemote enemy) => _world.Enemies[enemy.NetworkObjectId] = enemy;
        public void Unregister(EnemyRemote enemy) => _world.Enemies.Remove(enemy.NetworkObjectId);

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

            // 3. 리컨사일
            ReconcileSystem.Process(world);

            // 4~5. 예측 시계 → 예측
            int ticks = ClockSystem.Advance(ref world.Clock, frameTime);
            for (int i = 0; i < ticks; i++)
            {
                PredictSystem.Tick(world, SimulationClock.DeltaTime);
                world.Clock.Tick++;
            }
        }

        private void LateUpdate()
        {
            ClientWorld world = _world;
            float frameTime = Time.deltaTime;

            // 5. 원격 엔티티 보간 (로컬 플레이어 유무와 무관)
            InterpolationSystem.Advance(ref world.Interp, frameTime);
            foreach (EnemyRemote enemy in world.Enemies.Values)
            {
                InterpolationSystem.Present(enemy, world.Interp.RenderTick);
            }

            // 6. 로컬 플레이어 연출
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
        }

        // NGO가 아무 때나 부른다 → 기록만 (deferment)
        private void OnSnapshot(ulong senderId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out int tick);
            reader.ReadValueSafe(out ushort count);

            InterpolationSystem.Receive(ref _world.Interp, tick);

            for (int i = 0; i < count; i++)
            {
                reader.ReadNetworkSerializable(out EnemySnapshot snapshot);
                if (_world.Enemies.TryGetValue(snapshot.ObjectId, out EnemyRemote enemy))
                {
                    enemy.Record(tick, snapshot);
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