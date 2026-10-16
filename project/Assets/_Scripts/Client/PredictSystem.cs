using Project02.Player;

namespace Project02.Client
{
    public static class PredictSystem
    {
        public static void Tick(ClientWorld world, float deltaTime)
        {
            PlayerOwner owner = world.Local;
            PlayerController controller = owner.Controller;
            PlayerSimulation sim = controller.Simulation;
            int tick = world.Clock.Tick;

            ref InputState state = ref world.Input;
            ref CameraState camera = ref world.Camera;

            // 1. 이번 틱 입력 확정 + 래치 소비 (숨은 부수효과 없이 여기서 명시적으로)
            InputPayload input = new InputPayload
            {
                Tick = tick,
                MoveInput = state.Move,
                RunInput = state.Run,
                JumpInput = state.JumpPressed,
                YawInput = camera.Yaw,
                PitchInput = camera.Pitch,
                AttackInput = state.Attack,
                AttackPressed = state.AttackPressed,
                AimInput = state.Aim,
                AimPoint = camera.AimPoint,
            };
            state.JumpPressed = false;
            state.AttackPressed = false;
            owner.InputHistory.SetValue(tick, input);

            // 2. 시뮬레이션
            if (PlayerStepUtil.Step(sim, input, controller.OwnerClientId, deltaTime, controller.transform, out ShotInfo shot))
            {
                owner.View.OnFired(shot, input.AimPoint);
                camera.Recoil += controller.PlayerWeapon.View.RollRecoil(shot.Stance);
            }

            // 3. 기록
            owner.StateHistory.SetValue(tick, sim.State);
            owner.View.OnTick(sim.State.Movement);

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