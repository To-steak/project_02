namespace Project02.Player
{
    // 유틸(쓰기): 레벨 스탯 반영. 호출처는 PlayerProgression(스폰 1회)과 StatSystem(서버, 레벨 변경 시)뿐
    public static class PlayerStatUtil
    {
        public static void Apply(PlayerController controller, in SOLevelTable.Row stats, bool isServer)
        {
            if (isServer)
            {
                controller.Health.Max.Value = stats.MaxHp;
                controller.Health.Current.Value = stats.MaxHp;
            }

            controller.Simulation.State.Stamina = new StaminaState
            {
                Energy = stats.MaxMp,
                MaxEnergy = stats.MaxMp,
                IsExhausted = false,
            };
        }
    }
}