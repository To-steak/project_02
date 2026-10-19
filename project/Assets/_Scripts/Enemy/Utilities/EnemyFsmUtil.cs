using Project02.World;

namespace Project02.Enemy
{
    // 유틸: 상태 전환과 분기. 상태별 동작은 EnemyFsmUtil.<상태>.cs (partial, private)
    // 상태를 추가하면: 파일 EnemyFsmUtil.<상태>.cs + 아래 switch에 case
    public static partial class EnemyFsmUtil
    {
        public static void Reset(EnemySimulation sim, int tick)
        {
            sim.StateId = EnemyStateId.General;
            sim.StateEnterTick = tick;
            Enter(sim, EnemyStateId.General);
        }

        public static void Change(EnemySimulation sim, EnemyStateId next, int tick)
        {
            Exit(sim, sim.StateId);
            sim.StateId = next;
            sim.StateEnterTick = tick;
            Enter(sim, next);
        }

        // 읽기: 현재 상태에 머문 틱 수 (쿨다운·지속 시간 판정용)
        public static int Elapsed(EnemySimulation sim, int tick) => tick - sim.StateEnterTick;

        public static void Tick(ServerWorld world, EnemySimulation sim, float deltaTime)
        {
            switch (sim.StateId)
            {

                case EnemyStateId.General: TickGeneral(world, sim); break;
                case EnemyStateId.Battle: TickBattle(world, sim); break;
            }
        }

        private static void Enter(EnemySimulation sim, EnemyStateId id)
        {
            switch (id)
            {
                case EnemyStateId.General: EnterGeneral(sim); break;
            }
        }

        private static void Exit(EnemySimulation sim, EnemyStateId id)
        {
            // 아직 Exit이 필요한 상태 없음
        }
    }
}