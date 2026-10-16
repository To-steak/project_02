using Project02.Core;
using Project02.Enemy;
using Project02.Player;
using Project02.Service;
using Unity.Netcode;
using UnityEngine;

namespace Project02.Server
{
    // World 진입점: ServerWorld를 소유하고, 시스템을 정해진 순서로 돌린다
    public class ServerSimulation : MonoBehaviour
    {
        public static ServerSimulation Instance { get; private set; }

        private readonly ServerWorld _world = new();

        private const int MAX_TICKS_PER_FRAME = 5;

        private void Awake() => Instance = this;
        private void OnDestroy() { if (Instance == this) Instance = null; }

        // 등록 (Authoring MonoBehaviour가 NGO 생명주기에서 부른다)
        public void Register(PlayerServer player) => _world.Players.Add(player);
        public void Register(EnemyController enemy) => _world.Enemies.Add(enemy);
        public void Unregister(PlayerServer player) => _world.Players.Remove(player);
        public void Unregister(EnemyController enemy) => _world.Enemies.Remove(enemy);

        // 2단계(데미지)에서 사라질 예정
        public void EnqueueDeath(EnemyController enemy) => _world.Deaths.Add(enemy);

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
            // 1~3. 플레이어
            foreach (PlayerServer player in world.Players) player.Tick(deltaTime);

            // 4~5. 투사체 (Iceberg 진입은 World 진입점에서만)
            Physics.SyncTransforms();   // 이번 틱에 움직인 콜라이더를 물리에 반영
            ProjectileService.Instance.Tick(world.Hits, deltaTime);

            // 6. 피해
            DamageSystem.Process(world.Hits);

            // 7~8. 적
            for (int i = world.Enemies.Count - 1; i >= 0; i--) world.Enemies[i].Tick(deltaTime);

            // 9. 사망
            DeathSystem.Process(world.Deaths);

            // 10. 스냅샷 (이번 틱의 결과)
            SnapshotSystem.Send(world, network);
        }
    }
}