using Project02.Player;
using UnityEngine;

namespace Project02.Client
{
    public static class ViewSystem
    {
        public static void Present(ClientWorld world, float frameTime)
        {
            PlayerController controller = world.Local.Controller;
            PlayerCamera camera = controller.PlayerCamera;
            PlayerLocomotion locomotion = controller.PlayerLocomotion;

            float alpha = Mathf.Clamp01(world.Clock.Accumulator / world.Clock.Interval);
            world.Local.View.Present(camera.Yaw, camera.Pitch, camera.AimPoint, controller.PlayerInput.MoveInput, locomotion.Movement, locomotion.IsRunning, alpha, frameTime);
        }
    }
}