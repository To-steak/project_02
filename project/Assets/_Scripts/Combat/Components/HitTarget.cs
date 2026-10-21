// 튜플: Team + RewindTarget + Health
namespace Project02.Combat
{
    // 피격 묶음. 판정 시스템(ProjectileSystem)은 이 목록만 보고, 적인지 플레이어인지 모른다
    // 등록·해제는 ServerLoop.Register/Unregister (해제는 같은 Health 참조로 찾는다)
    public readonly struct HitTarget
    {
        public readonly Team Team;
        public readonly RewindTarget Rewind;
        public readonly Health Health;

        public HitTarget(Team team, RewindTarget rewind, Health health)
        {
            Team = team;
            Rewind = rewind;
            Health = health;
        }
    }
}
