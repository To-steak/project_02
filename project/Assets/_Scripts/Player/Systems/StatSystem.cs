using Project02.Combat;
using Project02.World;

namespace Project02.Player
{
    // 레벨이 바뀐 플레이어의 스탯 반영 (콜백 대신 질의: AppliedLevel != Network.Level)
    public static class StatSystem
    {
        public static void Process(ServerWorld world)
        {
            foreach (PlayerServerAuthoring player in world.Players)
            {
                int level = player.Network.Level.Value;
                if (player.AppliedLevel == level) continue;

                SOLevelTable.Row stats = player.Settings.LevelTable.Get(level);
                PlayerStatUtil.ApplyStamina(player.Simulation, stats);
                PlayerStatUtil.ApplyHealth(player.Health, stats);
                player.AppliedLevel = level;
            }
        }
    }
}