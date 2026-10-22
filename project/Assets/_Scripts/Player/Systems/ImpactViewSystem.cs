using System.Collections.Generic;
using Project02.Combat;
using Project02.Weapon;
using Project02.World;

namespace Project02.Player
{
    // 예측 피격 연출 (LateUpdate): world.PredictedImpacts → 로컬 플레이어의 WeaponView → ProjectileVisualService
    // ProjectileVisualService는 WeaponView에서만 부른다 (ECS_RULES 8장)
    public static class ImpactViewSystem
    {
        public static void Present(ClientWorld world)
        {
            List<ImpactInfo> impacts = world.PredictedImpacts;
            if (impacts.Count == 0) return;

            WeaponView view = world.Local.View.WeaponView;   // 프리팹 없는 무기는 뷰가 없다 → 연출 없이 비운다
            if (view != null)
            {
                foreach (ImpactInfo impact in impacts)
                {
                    view.SpawnImpact(impact);
                }
            }

            impacts.Clear();
        }
    }
}
