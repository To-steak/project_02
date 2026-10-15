using Project02.Player;
using UnityEngine;

namespace Project02.Client
{
    public static class PredictSystem
    {
        public static void Tick(PlayerOwner owner, int tick, float deltaTime)
        {
            PlayerController controller = owner.Controller;
            PlayerCamera camera = controller.PlayerCamera;

            Vector3 aimPoint = camera.AimPoint;
            InputPayload input = controller.PlayerInput.Capture(tick, camera.Yaw, camera.Pitch, aimPoint);
            owner.InputHistory[tick & PlayerOwner.BUFFER_MASK] = input;

            if (controller.Step(input, deltaTime, out Shot shot))
            {
                owner.View.OnFired(shot, aimPoint);
                camera.AddRecoil(controller.PlayerWeapon.View.RollRecoil(shot.Stance));
            }

            owner.StateHistory[tick & PlayerOwner.BUFFER_MASK] = controller.Capture(tick);
            owner.View.OnTick(controller.PlayerLocomotion.Movement);

            SendInputs(owner, tick);
        }

        private static void SendInputs(PlayerOwner owner, int tick)
        {
            int count = Mathf.Min(InputBundle.CAPACITY, tick + 1);
            InputBundle bundle = new InputBundle { Count = (byte)count };
            for (int i = 0; i < count; i++)
            {
                bundle.Set(i, owner.InputHistory[(tick - i) & PlayerOwner.BUFFER_MASK]);
            }
            owner.Controller.PlayerServer.InputRPC(bundle);
        }
    }
}