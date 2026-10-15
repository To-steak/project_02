using Project02.Combat;

namespace Project02.Server
{
    public readonly struct HitInfo
    {
        public readonly IDamageable Target;
        public readonly DamageInfo Info;

        public HitInfo(IDamageable target, in DamageInfo info)
        {
            Target = target;
            Info = info;
        }
    }
}