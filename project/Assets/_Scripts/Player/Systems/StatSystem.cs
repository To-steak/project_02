using Project02.Combat;
using Project02.World;

namespace Project02.Player
{
    // 레벨이 바뀐 플레이어의 스탯 반영 (콜백 대신 질의: AppliedLevel != Level)
    public static class StatSystem
    {
        public static void Process(ServerWorld world)
        {
            foreach (PlayerServerAuthoring player in world.Players)
            {
                PlayerProgression progression = player.Progression;

                int level = progression.Level.Value;
                if (progression.AppliedLevel == level) continue;

                SOLevelTable.Row stats = progression.Table.Get(level);
                PlayerStatUtil.ApplyStamina(player.Simulation, stats);
                PlayerStatUtil.ApplyHealth(player.Health, stats);
                progression.AppliedLevel = level;
            }
        }
    }
}