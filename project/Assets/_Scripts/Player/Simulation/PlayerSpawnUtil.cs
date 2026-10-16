using Project02.Character;

namespace Project02.Player
{
    // 유틸(쓰기): 스폰 시 1회, 이 프로세스의 사본 하나를 서버와 같은 값으로 시작한다
    // 호출처: PlayerServer(서버 사본), PlayerOwner(예측 사본)의 OnNetworkPostSpawn
    public static class PlayerSpawnUtil
    {
        public static void Initialize(PlayerSimulation sim, PlayerController controller)
        {
            sim.Movement = controller.PlayerLocomotion.Settings;
            sim.Catalog = controller.PlayerWeapon.Catalog;

            sim.State = default;
            sim.State.Movement = new MovementState { Position = controller.transform.position };

            PlayerEquipUtil.Apply(sim, controller.EquippedWeapon.Value);

            PlayerProgression progression = controller.PlayerProgression;
            PlayerStatUtil.ApplyStamina(sim, progression.Table.Get(progression.Level.Value));
        }
    }
}