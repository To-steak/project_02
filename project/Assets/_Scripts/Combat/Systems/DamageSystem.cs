using System.Collections.Generic;
using UnityEngine;

namespace Project02.Combat
{
    public static class DamageSystem
    {
        public static void Process(List<HitInfo> hits)
        {
            foreach (HitInfo hit in hits)
            {
                Health health = hit.Target;
                DamageInfo info = hit.Info;

                // 1. 같은 틱에 먼저 맞은 탄으로 이미 죽었거나 Despawn된 대상
                if (health == null || health.IsDead || info.Amount <= 0) continue;

                // 2. 감소 적용
                int amount = Mathf.RoundToInt(info.Amount * (1.0f - Mathf.Clamp01(health.DamageReduction)));

                // 3. 체력 감소 (0 미만 금지)
                health.Current.Value = Mathf.Max(0, health.Current.Value - amount);

                // 4. 기여자 기록 (보상 분배용)
                if (info.IsFromPlayer && !health.Contributors.Contains(info.AttackerId))
                {
                    health.Contributors.Add(info.AttackerId);
                }
            }

            hits.Clear();
        }
    }
}