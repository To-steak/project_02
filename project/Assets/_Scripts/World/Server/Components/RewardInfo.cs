namespace Project02.Server
{
    public readonly struct RewardInfo
    {
        public readonly ulong[] ClientIds;
        public readonly int Exp;
        public readonly int Gold;

        public RewardInfo(ulong[] clientIds, int exp, int gold)
        {
            ClientIds = clientIds;
            Exp = exp;
            Gold = gold;
        }
    }
}