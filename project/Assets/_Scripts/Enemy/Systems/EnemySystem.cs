using Project02.Enemy;
using UnityEngine;

namespace Project02.Server
{
    // 서버의 적 틱: 상태 머신 → 이동 → transform 출력
    public static class EnemySystem
    {
        public static void Tick(ServerWorld world, float deltaTime)
        {
            foreach (EnemyServerAuthoring enemy in world.Enemies)
            {
                if (enemy.Health.IsDead) continue;   // DeathSystem이 같은 틱에 치운다

                EnemySimulation sim = enemy.Simulation;

                // 1. 상태 머신이 이번 틱 이동 입력을 정한다
                sim.MoveInput = Vector2.zero;
                sim.MoveSpeed = 0.0f;
                EnemyFsmUtil.Tick(world, sim, deltaTime);

                // 2. 이동
                EnemyMoveUtil.Step(sim.Settings, ref sim.Movement, ref sim.Rotation, sim.MoveInput, sim.MoveSpeed, deltaTime);

                // 3. 출력 (규칙 7): 클라가 없는 프로세스에서만. 클라가 있으면 EnemyViewSystem이 쓴다
                if (world.WriteTransforms)
                {
                    enemy.transform.SetPositionAndRotation(sim.Movement.Position, sim.Rotation);
                }
            }
        }
    }
}