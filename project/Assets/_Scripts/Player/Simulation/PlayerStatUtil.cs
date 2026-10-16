using Project02.Combat;

namespace Project02.Player
{
    // 유틸(쓰기): 레벨 스탯 반영. 호출처: StatSystem(서버, 레벨 변경 시), PlayerSpawnUtil·PlayerServer(스폰 1회)
    public static class PlayerStatUtil
    {
        public static void ApplyStamina(PlayerSimulation sim, in SOLevelTable.Row stats)
        {
            sim.State.Stamina = new StaminaState
            {
                Energy = stats.MaxMp,
                MaxEnergy = stats.MaxMp,
                IsExhausted = false,
            };
        }

        // 서버 전용 (NetworkVariable 쓰기)
        public static void ApplyHealth(Health health, in SOLevelTable.Row stats)
        {
            health.Max.Value = stats.MaxHp;
            health.Current.Value = stats.MaxHp;   // 레벨업 시 완전 회복 (기존 동작)
        }
    }
}