using System.Collections.Generic;
using Project02.Service;

namespace Project02.Server
{
    public static class RewardSystem
    {
        public static void Process(List<RewardInfo> rewards, SessionService session)
        {
            foreach (RewardInfo reward in rewards)
            {
                session.AddReward(reward.ClientIds, reward.Exp, reward.Gold);
            }
            rewards.Clear();
        }
    }
}