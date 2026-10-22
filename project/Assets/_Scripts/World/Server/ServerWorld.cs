using System.Collections.Generic;
using Unity.Netcode;
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
        public readonly List<NetworkObject> Despawns = new();   // DeathSystem이 쌓고 DespawnSystem이 Step 끝에 처리
        public readonly List<Projectile> Projectiles = new();
        public readonly List<HitInfo> Hits = new();
        public readonly List<RewardInfo> Rewards = new();
        public readonly List<TargetInfo> Targets = new();     // TargetSystem이 매 틱 다시 채우고 적 유틸이 읽는다
        public readonly List<CapsuleSample> HitSamples = new();   // ProjectileSystem의 재사용 버퍼: 투사체마다 되감은 대상 캡슐 (매 틱 할당하지 않는다)
        public readonly Outbox Outbox = new();
        public bool WriteTransforms;   // 이 프로세스에 클라 World가 없을 때만 true (전용 서버 디버그용)

        // 컴포넌트 묶음 목록 (규칙 11): 소비 시스템(판정·사망)은 이것만 본다
        public readonly List<HitTarget> Hittables = new();   // ProjectileSystem
        public readonly List<Mortal> Mortals = new();        // DeathSystem

        // Entities (종류 목록): 정본을 가진 시스템(이동, 되감기 기록, 스냅샷)만 쓴다
        public readonly List<PlayerServerAuthoring> Players = new();
        public readonly List<EnemyServerAuthoring> Enemies = new();
    }
}
