using System.Collections.Generic;
using Project02.Enemy;
using Project02.Player;

namespace Project02.Server
{
    public sealed class ServerWorld
    {
        // Singleton Components
        public ServerClock Clock;
        public readonly List<HitInfo> Hits = new();
        public readonly List<RewardInfo> Rewards = new();

        // Entities
        public readonly List<PlayerServer> Players = new();
        public readonly List<EnemyController> Enemies = new();
    }
}