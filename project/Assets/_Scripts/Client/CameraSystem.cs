using Project02.Player;
using Project02.Service;

namespace Project02.Client
{
    public static class CameraSystem
    {
        public static void Update(PlayerOwner owner, float deltaTime)
        {
            PlayerController controller = owner.Controller;
            PlayerCamera camera = controller.PlayerCamera;
            PlayerWeapon weapon = controller.PlayerWeapon;

            camera.Look(controller.PlayerInput.LookInput, InputService.Instance.MouseSensitivity, deltaTime);

            float zoom = weapon.HasWeapon ? weapon.Settings.AdsZoom : 1.0f;
            camera.ApplyAim(controller.PlayerInput.AimInput, zoom, deltaTime);
        }
    }
}