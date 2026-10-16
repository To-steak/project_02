using System.Collections.Generic;
using Project02.Enemy;
using Project02.Player;

namespace Project02.Server
{
    public sealed class ServerWorld
    {
        public ServerClock Clock;
        public readonly List<HitInfo> Hits = new();
        public readonly List<EnemyController> Deaths = new();
        
        public readonly List<PlayerServer> Players = new();
        public readonly List<EnemyController> Enemies = new();
    }
}