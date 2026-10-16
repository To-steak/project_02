using Project02.Navigation;
using UnityEngine;

namespace Project02.Enemy
{
    public static class EnemyNavigation
    {
        public const float WANDER_INTERVAL = 1.0f;
        public const float ARRIVE_THRESHOLD = 0.2f;

        public static Vector2 WanderInput(EnemySimulation sim, float deltaTime)
        {
            if (sim.Waypoint >= sim.Path.Count)
            {
                sim.WanderTimer += deltaTime;
                if (sim.WanderTimer >= WANDER_INTERVAL)
                {
                    sim.WanderTimer = 0.0f;
                    Wander(sim);
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