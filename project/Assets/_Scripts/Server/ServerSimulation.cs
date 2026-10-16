using System.Collections.Generic;
using Project02.Player;
using Project02.Enemy;
using Unity.Netcode;
using UnityEngine;
using Project02.Service;
using Project02.Core;

namespace Project02.Server
{
    public class ServerSimulation : MonoBehaviour
    {
        public static ServerSimulation Instance { get; private set; }

        private ServerClock _clock;
        private readonly List<PlayerServer> _players = new();
        private readonly List<EnemyController> _enemies = new();
        private readonly List<HitInfo> _hits = new();
        private readonly List<EnemyController> _deaths = new();

        private const int MAX_TICKS_PER_FRAME = 5;

        private void Awake() => Instance = this;
        private void OnDestroy() { if (Instance == this) Instance = null; }

        public void Register(PlayerServer player) => _players.Add(player);
        public void Register(EnemyController enemy) => _enemies.Add(enemy);

        public void Unregister(PlayerServer player) => _players.Remove(player);
        public void Unregister(EnemyController enemy) => _enemies.Remove(enemy);

        public void EnqueueDeath(EnemyController enemy) => _deaths.Add(enemy);

        private void Update()
        {
            NetworkManager networkManager = NetworkManager.Singleton;
            if (networkManager == null || !networkManager.IsServer) return;

            _clock.Accumulator += Time.deltaTime;

            int ticks = 0;
            while (_clock.Accumulator >= SimulationClock.DeltaTime && ticks < MAX_TICKS_PER_FRAME)
            {
                _clock.Accumulator -= SimulationClock.DeltaTime;
                Step(SimulationClock.DeltaTime);
                _clock.Tick++;
                ticks++;
            }

            if (ticks == MAX_TICKS_PER_FRAME)
            {
                _clock.Accumulator = 0.0f;
            }
        }

        private void Step(float deltaTime)
        {
            // 1~3. 플레이어
            foreach (PlayerServer player in _players) player.Tick(deltaTime);

            // 4~5. 투사체
            Physics.SyncTransforms();
            ProjectileService.Instance.Tick(_hits, deltaTime);

            // 6. 피해
            DamageSystem.Process(_hits);

            // 7~8. 적
            for (int i = _enemies.Count - 1; i >= 0; i--) _enemies[i].Tick(deltaTime);

            // 9. 사망
            DeathSystem.Process(_deaths);

            // 10. 스냅샷 (이번 틱의 결과) [int Tick][ushort Count][EnemySnapshot × Count]
            SnapshotSystem.Send(_clock.Tick, _enemies);
        }
    }
}