namespace Project02.Player
{
    // 유틸(쓰기): 장착 반영. 호출처는 EquipSystem(서버, 변경 시)과 PlayerSpawnUtil(스폰 1회)뿐
    public static class PlayerEquipUtil
    {
        public static void Apply(PlayerSimulation sim, int id)
        {
            sim.State.Weapon = new WeaponState { EquippedId = id };   // 무기를 바꾸면 반동·연사 상태 초기화. 무기 참조는 따라온다
        }
    }
}