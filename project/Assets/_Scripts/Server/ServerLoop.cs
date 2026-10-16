using Project02.Core;
using Project02.Enemy;
using Project02.Player;
using Project02.Service;
using Unity.Netcode;
using UnityEngine;

namespace Project02.Server
{
    [DefaultExecutionOrder(-100)]
    public class ServerLoop : MonoBehaviour
    {
        public static ServerLoop Instance { get; private set; }

        [Tooltip("투사체 레이캐스트 대상: 지형·장애물만. Enemy(이력 판정)와 Player 레이어는 빼야 함")]
        [SerializeField] private LayerMask _projectileHitLayer;
        private readonly ServerWorld _world = new();

        private const int MAX_TICKS_PER_FRAME = 5;

        private void Awake() => Instance = this;
        private void OnDestroy() { if (Instance == this) Instance = null; }

        public void Register(PlayerServer player)
        {
            _world.Players.Add(player);
            SessionSystem.Join(player, SessionService.Instance);
        }
        public void Register(EnemyController enemy)
        {
            _world.Enemies.Add(enemy);
            EnemyFsm.Reset(enemy.Simulation, _world.Clock.Tick);
        }
        public void Unregister(PlayerServer player) => _world.Players.Remove(player);
        public void Unregister(EnemyController enemy) => _world.Enemies.Remove(enemy);

        private void Update()
        {
            NetworkManager network = NetworkManager.Singleton;
            if (network == null || !network.IsServer) return;

            // 클라 World가 같은 프로세스에 있으면 transform은 클라 표시가 쓴다 (호스트)
            _world.WriteTransforms = !network.IsClient;

            ref ServerClock clock = ref _world.Clock;
            clock.Accumulator += Time.deltaTime;

            int ticks = 0;
            while (clock.Accumulator >= SimulationClock.DeltaTime && ticks < MAX_TICKS_PER_FRAME)
            {
                clock.Accumulator -= SimulationClock.DeltaTime;
                Tick(_world, network, _projectileHitLayer, SimulationClock.DeltaTime);
                clock.Tick++;
                ticks++;
            }

            if (ticks == MAX_TICKS_PER_FRAME)
            {
                clock.Accumulator = 0.0f;
            }
        }

        private static void Tick(ServerWorld world, NetworkManager network, LayerMask projectileHitLayer, float deltaTime)
        {
            // 0. 장착: EquippedWeapon 변경 → WeaponState (이번 틱 발사에 반영)
            EquipSystem.Process(world);

            // 1. 플레이어: 수신 → 소비 → Step → 발사(world.Projectiles) → Outbox
            ServerPlayerSystem.Tick(world, deltaTime);

            // 2. 투사체: 지형 레이캐스트 + 되감은 적 → world.Hits
            Physics.SyncTransforms();
            ProjectileSystem.Tick(world, projectileHitLayer, deltaTime);

            // 3. 피해: Hits → Health
            DamageSystem.Process(world.Hits);

            // 4. 적: 상태 머신 → 이동
            EnemySystem.Tick(world, deltaTime);

            // 5. 사망: IsDead인 적 → world.Rewards, world.Despawns
            DeathSystem.Process(world);

            // 6. 세션: Rewards → SessionService → Level
            SessionSystem.Process(world, SessionService.Instance);

            // 7. 스탯: Level 변경 → Health, Stamina
            StatSystem.Process(world);

            // 8. 되감기 이력: 이번 틱 결과 (스냅샷과 같은 시점)
            RewindSystem.Record(world);

            // 9. 스냅샷 (분할 전송)
            SnapshotSystem.Send(world, network);

            // 10. 송신: Outbox → RPC
            NetSendSystem.Flush(world);

            // 11. Despawn: 모든 순회가 끝난 뒤
            DespawnSystem.Process(world);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            foreach (Weapon.Projectile projectile in _world.Projectiles)
            {
                Gizmos.DrawRay(projectile.Position, projectile.Velocity * SimulationClock.DeltaTime);
            }
        }
#endif
    }
}