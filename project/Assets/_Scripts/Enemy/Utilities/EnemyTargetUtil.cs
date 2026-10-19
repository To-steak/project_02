using UnityEngine;
using Project02.Combat;
using Project02.Player;
using Project02.World;

namespace Project02.Enemy
{
    public static class EnemyTargetUtil
    {
        public static bool TryGetTarget(ServerWorld world, EnemySimulation sim, out Vector3 position)
        {
            position = default;
            if (!sim.TargetClientId.HasValue) return false;

            ulong clientId = sim.TargetClientId.Value;
            foreach (PlayerServerAuthoring player in world.Players)
            {
                if (player.OwnerClientId != clientId) continue;

                if (player.Health.IsDead) return false;

                position = player.Simulation.State.Movement.Position;
                return true;
            }

            return false;
        }

        public static bool TryFindClosest(ServerWorld world, Vector3 origin, float range, out ulong clientId)
        {
            clientId = default;
            float best = range * range;
            bool found = false;

            foreach (PlayerServerAuthoring player in world.Players)
            {
                if (player.Health.IsDead) continue;

                Vector3 offset = player.Simulation.State.Movement.Position - origin;
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