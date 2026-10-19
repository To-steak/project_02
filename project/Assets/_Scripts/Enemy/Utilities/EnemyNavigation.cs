using Project02.Navigation;
using UnityEngine;

namespace Project02.Enemy
{
    public static class EnemyNavigation
    {
        public const int WANDER_INTERVAL_TICKS = 50;   // 1초 (50Hz). 도착 후 다음 배회까지 대기
        public const float ARRIVE_THRESHOLD = 0.2f;

        public static Vector2 WanderInput(EnemySimulation sim, int tick)
        {
            if (sim.Waypoint >= sim.Path.Count)
            {
                if (sim.NextWanderTick < 0)
                {
                    sim.NextWanderTick = tick + WANDER_INTERVAL_TICKS;   // 대기 시작
                }
                else if (tick >= sim.NextWanderTick)
                {
                    sim.NextWanderTick = -1;
                    Wander(sim);   // 실패하면 다음 틱에 다시 1초 대기 (기존 동작과 같음)
                }
            }

            if (sim.Waypoint < sim.Path.Count)
            {
                Vector3 direction = sim.Grid.ConvertWorldCoord(sim.Path[sim.Waypoint]) - sim.Movement.Position;
                direction.y = 0.0f;
                if (direction.sqrMagnitude < ARRIVE_THRESHOLD * ARRIVE_THRESHOLD)
                {
                    sim.Waypoint++;
                }
                else
                {
                    return new Vector2(direction.x, direction.z).normalized;
                }
            }

            return Vector2.zero;
        }

        private static void Wander(EnemySimulation sim)
        {
            PathGrid grid = sim.Grid;
            sim.Path.Clear();
            sim.Waypoint = 0;

            if (!grid.TryConvertCellCoord(sim.Movement.Position, out int startX, out int startZ)) return;
            if (!grid.TryGetRandomCell(out int goalX, out int goalZ)) return;

            if (PathFinder.TryFindPath(grid, startX, startZ, goalX, goalZ, sim.Path))
            {
                PathFinder.Smooth(grid, sim.Path);
                sim.Waypoint = sim.Path.Count > 1 ? 1 : 0;
            }
        }
    }
}