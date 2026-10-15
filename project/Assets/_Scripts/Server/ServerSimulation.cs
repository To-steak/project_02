using System.Collections.Generic;
using Project02.Player;
using Project02.Enemy;
using Unity.Netcode;
using UnityEngine;
using Project02.Service;

namespace Project02.Server
{
    public class ServerSimulation : MonoBehaviour
    {
        public static ServerSimulation Instance { get; private set; }

        private readonly List<PlayerServer> _players = new();
        private readonly List<EnemyController> _enemies = new();
        private readonly List<HitInfo> _hits = new();
        private readonly List<EnemyController> _deaths = new();

        private void Awake() => Instance = this;
        private void OnDestroy() { if (Instance == this) Instance = null; }

        public void Register(PlayerServer player) => _players.Add(player);
        public void Register(EnemyController enemy) => _enemies.Add(enemy);

        public void Unregister(PlayerServer player) => _players.Remove(player);
        public void Unregister(EnemyController enemy) => _enemies.Remove(enemy);

        public void EnqueueDeath(EnemyController enemy) => _deaths.Add(enemy);

        private void FixedUpdate()
        {
            NetworkManager networkManager = NetworkManager.Singleton;
            if (networkManager == null || !networkManager.IsServer) return;

            // 1~3. 플레이어
            foreach (PlayerServer player in _players)
            {
                player.Tick(SimulationClock.DeltaTime);
            }

            // 4~5. 투사체
            ProjectileService.Instance.Tick(_hits, SimulationClock.DeltaTime);

            // 6. 피해 (6-3)
            DamageSystem.Process(_hits);

            // 7~8. 적
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                _enemies[i].Tick(SimulationClock.DeltaTime);
            }

            // 9. 사망 (6-4)
            DeathSystem.Process(_deaths);
        }
    }
}