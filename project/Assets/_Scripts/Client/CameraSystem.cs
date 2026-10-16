using Project02.Player;

namespace Project02.Client
{
    public static class CameraSystem
    {
        public static void Update(ClientWorld world, float sensitivity, float frameTime)
        {
            PlayerController controller = world.Local.Controller;
            PlayerCamera camera = controller.PlayerCamera;
            PlayerWeapon weapon = controller.PlayerWeapon;

            camera.Look(controller.PlayerInput.LookInput, sensitivity, frameTime);

            float zoom = weapon.HasWeapon ? weapon.Settings.AdsZoom : 1.0f;
            camera.ApplyAim(controller.PlayerInput.AimInput, zoom, frameTime);
        }
    }
}