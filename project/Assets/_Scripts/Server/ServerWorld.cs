using System.Collections.Generic;
using Project02.Enemy;
using Project02.Player;
using Project02.Weapon;

namespace Project02.Server
{
    public sealed class ServerWorld
    {
        // Singleton Components
        public ServerClock Clock;
        public readonly List<EnemyController> Despawns = new();   // DeathSystem이 쌓고 DespawnSystem이 Step 끝에 처리
        public readonly List<Projectile> Projectiles = new();
        public readonly List<HitInfo> Hits = new();
        public readonly List<RewardInfo> Rewards = new();
        public readonly Outbox Outbox = new();

        // Entities
        public readonly List<PlayerServer> Players = new();
        public readonly List<EnemyController> Enemies = new();
    }
}