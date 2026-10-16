using Project02.Player;
using Project02.Server;
using UnityEngine;

namespace Project02.Enemy
{
    public static class EnemyTargetUtil
    {
        public static bool TryGetTarget(ServerWorld world, EnemySimulation sim, out Vector3 position)
        {
            position = default;
            if (!sim.TargetClientId.HasValue) return false;

            ulong clientId = sim.TargetClientId.Value;
            foreach (PlayerServer player in world.Players)
            {
                if (player.OwnerClientId != clientId) continue;

                PlayerController controller = player.Controller;
                if (controller.Health.IsDead) return false;

                position = controller.Simulation.State.Movement.Position;
                return true;
            }

            return false;
        }

        public static bool TryFindClosest(ServerWorld world, Vector3 origin, float range, out ulong clientId)
        {
            clientId = default;
            float best = range * range;
            bool found = false;

            foreach (PlayerServer player in world.Players)
            {
                PlayerController controller = player.Controller;
                if (controller.Health.IsDead) continue;

                Vector3 offset = controller.Simulation.State.Movement.Position - origin;
                offset.y = 0.0f;
                float sqr = offset.sqrMagnitude;
                if (sqr <= best)
                {
                    best = sqr;
                    clientId = player.OwnerClientId;
                    found = true;
                }
            }

            return found;
        }
    }
}