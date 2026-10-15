using Project02.Player;

namespace Project02.Client
{
    public static class ViewSystem
    {
        public static void Present(PlayerOwner owner, float alpha, float deltaTime)
        {
            PlayerController controller = owner.Controller;
            PlayerCamera camera = controller.PlayerCamera;
            PlayerLocomotion locomotion = controller.PlayerLocomotion;

            owner.View.Present(camera.Yaw, camera.Pitch, camera.AimPoint, controller.PlayerInput.MoveInput, locomotion.Movement, locomotion.IsRunning, alpha, deltaTime);
        }
    }
}