namespace Project02.Combat
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