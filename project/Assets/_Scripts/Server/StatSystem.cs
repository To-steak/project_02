using Project02.Player;

namespace Project02.Server
{
    // 레벨이 바뀐 플레이어의 스탯 반영 (콜백 대신 질의: AppliedLevel != Level)
    public static class StatSystem
    {
        public static void Process(ServerWorld world)
        {
            foreach (PlayerServer player in world.Players)
            {
                PlayerController controller = player.Controller;
                PlayerProgression progression = controller.PlayerProgression;

                int level = progression.Level.Value;
                if (progression.AppliedLevel == level) continue;

                SOLevelTable.Row stats = progression.Table.Get(level);
                PlayerStatUtil.ApplyStamina(player.Simulation, stats);
                PlayerStatUtil.ApplyHealth(controller.Health, stats);
                progression.AppliedLevel = level;
            }
        }
    }
}