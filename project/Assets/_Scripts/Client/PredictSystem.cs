using Project02.Player;
using UnityEngine;

namespace Project02.Client
{
    public static class PredictSystem
    {
        public static void Tick(ClientWorld world, float deltaTime)
        {
            PlayerOwner owner = world.Local;
            int tick = world.Clock.Tick;

            PlayerController controller = owner.Controller;
            PlayerCamera camera = controller.PlayerCamera;

            Vector3 aimPoint = camera.AimPoint;
            InputPayload input = controller.PlayerInput.Capture(tick, camera.Yaw, camera.Pitch, aimPoint);
            owner.InputHistory.SetValue(tick, input);

            if (controller.Step(input, deltaTime, out ShotInfo shot))
            {
                owner.View.OnFired(shot, aimPoint);
                camera.AddRecoil(controller.PlayerWeapon.View.RollRecoil(shot.Stance));
            }

            owner.StateHistory.SetValue(tick, controller.Capture(tick));
            owner.View.OnTick(controller.PlayerLocomotion.Movement);

            SendInputs(owner, tick);
        }

        private static void SendInputs(PlayerOwner owner, int tick)
        {
            InputBundle bundle = default;
            int count = 0;
            for (int i = 0; i < InputBundle.CAPACITY; i++)
            {
                if (!owner.InputHistory.TryGetValue(tick - i, out InputPayload input)) break;
                bundle.SetPayload(count++, input);
            }

            bundle.Count = (byte)count;
            owner.Controller.PlayerServer.InputRPC(bundle);
        }
    }
}