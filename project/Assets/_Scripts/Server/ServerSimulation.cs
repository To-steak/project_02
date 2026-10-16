using Project02.Core;
using Project02.Enemy;
using Project02.Player;
using Project02.Service;
using Unity.Netcode;
using UnityEngine;

namespace Project02.Server
{
    public class ServerSimulation : MonoBehaviour
    {
        public static ServerSimulation Instance { get; private set; }

        private readonly ServerWorld _world = new();

        private const int MAX_TICKS_PER_FRAME = 5;

        private void Awake() => Instance = this;
        private void OnDestroy() { if (Instance == this) Instance = null; }

        public void Register(PlayerServer player)
        {
            _world.Players.Add(player);
            SessionSystem.Join(player, SessionService.Instance);
        }
        public void Register(EnemyController enemy) => _world.Enemies.Add(enemy);
        public void Unregister(PlayerServer player) => _world.Players.Remove(player);
        public void Unregister(EnemyController enemy) => _world.Enemies.Remove(enemy);

        private void Update()
        {
            NetworkManager network = NetworkManager.Singleton;
            if (network == null || !network.IsServer) return;

            ref ServerClock clock = ref _world.Clock;
            clock.Accumulator += Time.deltaTime;

            int ticks = 0;
            while (clock.Accumulator >= SimulationClock.DeltaTime && ticks < MAX_TICKS_PER_FRAME)
            {
                clock.Accumulator -= SimulationClock.DeltaTime;
                Step(_world, network, SimulationClock.DeltaTime);
                clock.Tick++;
                ticks++;
            }

            if (ticks == MAX_TICKS_PER_FRAME)
            {
                clock.Accumulator = 0.0f;
            }
        }

        private static void Step(ServerWorld world, NetworkManager network, float deltaTime)
        {
            ProjectileService projectiles = ProjectileService.Instance;

            // 1~3. 플레이어: 수신 → 소비 → Step → 발사 → Outbox
            ServerPlayerSystem.Tick(world, projectiles, deltaTime);

            // 4~5. 투사체 → world.Hits
            Physics.SyncTransforms();
            projectiles.Tick(world.Hits, deltaTime);

            // 6. 피해: Hits → Health
            DamageSystem.Process(world.Hits);

            // 7. 적
            EnemySystem.Tick(world, deltaTime);

            // 8. 사망: IsDead인 적 → world.Rewards + Despawn
            DeathSystem.Process(world);

            // 9. 세션: Rewards → SessionService → 레벨
            SessionSystem.Process(world, SessionService.Instance);

            // 10. 스냅샷 (적)
            SnapshotSystem.Send(world, network);

            // 11. 송신: Outbox → RPC (플레이어 상태, 발사 연출)
            NetSendSystem.Flush(world);
        }
    }
}