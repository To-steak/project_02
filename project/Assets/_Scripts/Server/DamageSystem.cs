using System.Collections.Generic;

namespace Project02.Server
{
    public static class DamageSystem
    {
        public static void Process(List<HitInfo> hits)
        {
            foreach (HitInfo hit in hits)
            {
                hit.Target.ApplyDamage(hit.Info);
            }
            hits.Clear();
        }
    }
}