using System.Collections.Generic;
using Project02.Combat;
using Project02.Enemy;
using Project02.Player;
using Project02.Weapon;

namespace Project02.World
{
    public sealed class ServerWorld
    {
        // Singleton Components
        public ServerClock Clock;
        public readonly List<EnemyServerAuthoring> Despawns = new();   // DeathSystem이 쌓고 DespawnSystem이 Step 끝에 처리
        public readonly List<Projectile> Projectiles = new();
        public readonly List<HitInfo> Hits = new();
        public readonly List<RewardInfo> Rewards = new();
        public readonly Outbox Outbox = new();
        public bool WriteTransforms;   // 이 프로세스에 클라 World가 없을 때만 true (전용 서버 디버그용)

        // Entities
        public readonly List<PlayerServerAuthoring> Players = new();
        public readonly List<EnemyServerAuthoring> Enemies = new();
    }
}