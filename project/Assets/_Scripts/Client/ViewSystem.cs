using Project02.Player;
using UnityEngine;

namespace Project02.Client
{
    public static class ViewSystem
    {
        public static void Present(ClientWorld world, float frameTime)
        {
            PlayerSimulation sim = world.Local.Controller.Simulation;
            ref CameraState camera = ref world.Camera;

            float alpha = Mathf.Clamp01(world.Clock.Accumulator / world.Clock.Interval);
            world.Local.View.Present(camera.Yaw, camera.Pitch, camera.AimPoint, world.Input.Move, sim.State.Movement, sim.IsRunning, alpha, frameTime);
        }
    }
}