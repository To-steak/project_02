using Project02.World;

namespace Project02.Player
{
    // 장착 변경 반영 (콜백 대신 질의: WeaponState.EquippedId != EquippedWeapon)
    public static class EquipSystem
    {
        public static void Process(ServerWorld world)
        {
            foreach (PlayerServerAuthoring player in world.Players)
            {
                PlayerSimulation sim = player.Simulation;

                int desired = player.Network.EquippedWeapon.Value;
                if (sim.State.Weapon.EquippedId == desired) continue;

                PlayerEquipUtil.Apply(sim, desired);
            }
        }
    }
}