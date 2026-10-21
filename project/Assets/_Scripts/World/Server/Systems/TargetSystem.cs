using System.Collections.Generic;
using Project02.Player;

namespace Project02.World
{
    // 피해 처리 뒤, 적 앞: 이번 틱 이동(위치)과 피해(IsDead)가 반영된 플레이어로 world.Targets를 다시 채운다.
    // 적 유틸(EnemyTargetUtil, EnemyFsmUtil)은 World 대신 이 목록만 읽는다
    public static class TargetSystem
    {
        public static void Collect(ServerWorld world)
        {
            List<TargetInfo> targets = world.Targets;
            targets.Clear();

            foreach (PlayerServerAuthoring player in world.Players)
            {
                targets.Add(new TargetInfo(
                    player.OwnerClientId,
                    player.Simulation.State.Movement.Position,
                    player.Health.IsDead));
            }
        }
    }
}