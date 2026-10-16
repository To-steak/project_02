using Project02.Player;

namespace Project02.Server
{
    // 장착 변경 반영 (콜백 대신 질의: WeaponState.EquippedId != EquippedWeapon)
    public static class EquipSystem
    {
        public static void Process(ServerWorld world)
        {
            foreach (PlayerServer player in world.Players)
            {
                PlayerSimulation sim = player.Simulation;

                int desired = player.Controller.EquippedWeapon.Value;
                if (sim.State.Weapon.EquippedId == desired) continue;

                PlayerEquipUtil.Apply(sim, desired);
            }
        }
    }
}