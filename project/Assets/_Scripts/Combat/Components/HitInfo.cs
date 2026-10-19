using Project02.Combat;

namespace Project02.Server
{
    public readonly struct HitInfo
    {
        public readonly Health Target;
        public readonly DamageInfo Info;

        public HitInfo(Health target, in DamageInfo info)
        {
            Target = target;
            Info = info;
        }
    }
}