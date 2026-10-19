using UnityEngine;
using Project02.World;

namespace Project02.Player
{
    public static class ViewSystem
    {
        public static void Present(ClientWorld world, float frameTime)
        {
            PlayerSimulation sim = world.Local.Simulation;
            ref CameraState camera = ref world.Camera;

            float alpha = Mathf.Clamp01(world.Clock.Accumulator / world.Clock.Interval);
            world.Local.View.Present(camera.Yaw, camera.Pitch, camera.AimPoint, world.Input.Move, sim.State.Movement, sim.IsRunning, alpha, frameTime);
        }
    }
}