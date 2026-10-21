using System.Collections.Generic;
using UnityEngine;
using Project02.World;

namespace Project02.Enemy
{
    public static class EnemyTargetUtil
    {
        public static bool TryGetTarget(List<TargetInfo> targets, EnemySimulation sim, out Vector3 position)
        {
            position = default;
            if (!sim.TargetClientId.HasValue) return false;

            ulong clientId = sim.TargetClientId.Value;
            foreach (TargetInfo target in targets)
            {
                if (target.ClientId != clientId) continue;

                if (target.IsDead) return false;

                position = target.Position;
                return true;
            }

            return false;
        }

        public static bool TryFindClosest(List<TargetInfo> targets, Vector3 origin, float range, out ulong clientId)
        {
            clientId = default;
            float best = range * range;
            bool found = false;

            foreach (TargetInfo target in targets)
            {
                if (target.IsDead) continue;

                Vector3 offset = target.Position - origin;
                offset.y = 0.0f;
                float sqr = offset.sqrMagnitude;
                if (sqr <= best)
                {
                    best = sqr;
                    clientId = target.ClientId;
                    found = true;
                }
            }

            return found;
        }
    }
}