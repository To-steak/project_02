using System.Collections.Generic;
using Project02.Enemy;
using Project02.Player;
using Project02.Server;
using Unity.Netcode;
using UnityEngine;

namespace Project02.Client
{
    public class ClientSimulation : MonoBehaviour
    {
        public static ClientSimulation Instance { get; private set; }

        private ClientClock _clock;
        private PlayerOwner _local;
        private InterpolationClock _interp;
        private readonly Dictionary<ulong, EnemyRemote> _enemies = new();

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

            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Register(PlayerOwner owner)
        {
            _local = owner;
            _clock = ClockSystem.Create();
        }

        public void Register(EnemyRemote enemy)
        {
            _enemies[enemy.NetworkObjectId] = enemy;
        }

        public void Unregister(PlayerOwner owner)
        {
            if (_local == owner) _local = null;
        }

        public void Unregister(EnemyRemote enemy)
        {
            _enemies.Remove(enemy.NetworkObjectId);
        }

        private void Update()
        {
            if (_local == null) return;
            float frameTime = Time.deltaTime;

            CameraSystem.Update(_local, frameTime);

            ReconcileSystem.Process(_local, ref _clock);

            int ticks = ClockSystem.Advance(ref _clock, frameTime);
            for (int i = 0; i < ticks; i++)
            {
                PredictSystem.Tick(_local, _clock.Tick, SimulationClock.DeltaTime);
                _clock.Tick++;
            }
        }

        private void LateUpdate()
        {
            float frameTime = Time.deltaTime;

            InterpolationSystem.Advance(ref _interp, frameTime);
            foreach (EnemyRemote enemy in _enemies.Values)
            {
                InterpolationSystem.Present(enemy, _interp.RenderTick);
            }

            if (_local == null) return;
            float alpha = Mathf.Clamp01(_clock.Accumulator / _clock.Interval);
            ViewSystem.Present(_local, alpha, frameTime);
        }

        private void OnClientStarted()
        {
            _interp = default;
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(SnapshotSystem.MESSAGE, OnSnapshot);
        }

        private void OnClientStopped(bool value)
        {
            NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(SnapshotSystem.MESSAGE);
            _enemies.Clear();
        }

        private void OnSnapshot(ulong senderId, FastBufferReader reader)
        {
            reader.ReadValueSafe(out int tick);
            reader.ReadValueSafe(out ushort count);

            if (_interp.HasTick && tick <= _interp.LatestTick - EnemyRemote.BUFFER_SIZE) return;
            InterpolationSystem.Receive(ref _interp, tick);

            for (int i = 0; i < count; i++)
            {
                reader.ReadNetworkSerializable(out EnemySnapshot snapshot);
                if (_enemies.TryGetValue(snapshot.ObjectId, out EnemyRemote enemy))
                {
                    enemy.Record(tick, snapshot);
                }
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (_local == null) return;
            GUIStyle style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperRight };
            style.normal.textColor = Color.black;
            float x = Screen.width - 310f;

            GUI.Label(new Rect(x, 150, 300, 20), $"tick: {_clock.Tick}", style);
            GUI.Label(new Rect(x, 170, 300, 20), $"buffer: {_clock.SmoothedBuffer:F2}", style);
            GUI.Label(new Rect(x, 190, 300, 20), $"interval: {_clock.Interval * 1000.0f:F2}ms", style);
        }
#endif
    }
}