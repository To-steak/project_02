// 튜플: TickBuffer<EnemySnapshot> + Radius + Height
using Project02.Core;
using Project02.Enemy;

namespace Project02.World
{
    // 클라 피격 묶음 (규칙 11). ShotPredictSystem은 이 목록만 본다.
    // 등록·해제는 ClientLoop.Register/Unregister(EnemyRemoteAuthoring)에서 Enemies와 함께 (해제는 같은 Snapshots 참조로 찾는다).
    // 플레이어 총알은 플레이어를 맞히지 않으므로 원격 플레이어는 넣지 않는다
    public readonly struct ClientHitTarget
    {
        public readonly TickBuffer<EnemySnapshot> Snapshots;
        public readonly float Radius;
        public readonly float Height;

        public ClientHitTarget(TickBuffer<EnemySnapshot> snapshots, float radius, float height)
        {
            Snapshots = snapshots;
            Radius = radius;
            Height = height;
        }
    }
}
