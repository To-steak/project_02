using Project02.Player;
using Project02.Weapon;

namespace Project02.Client
{
    public static class PredictSystem
    {
        public static void Tick(ClientWorld world, float deltaTime)
        {
            PlayerOwnerAuthoring owner = world.Local;
            PlayerSimulation sim = owner.Simulation;
            int tick = world.Clock.Tick;

            ref InputState state = ref world.Input;
            ref CameraState camera = ref world.Camera;

            // 1. 이번 틱 입력 확정 + 래치 소비 (숨은 부수효과 없이 여기서 명시적으로)
            InputPayload input = new InputPayload
            {
                Tick = tick,
                ViewTick = world.Interp.HasTick ? world.Interp.RenderTick : -1.0f,
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
            if (PlayerStepUtil.Step(sim, input, owner.OwnerClientId, deltaTime, out ShotInfo shot))
            {
                owner.View.OnFired(shot, input.AimPoint);
                camera.Recoil += WeaponFeedback.RollRecoil(sim.Weapon, shot.Stance);   // 발사 성공 = sim.Weapon != null
            }

            // 3. 기록
            owner.StateHistory.SetValue(tick, sim.State);
            owner.View.OnTick(sim.State.Movement);

            SendInputs(owner, tick);
        }

        private static void SendInputs(PlayerOwnerAuthoring owner, int tick)
        {
            InputBundle bundle = default;
            int count = 0;
            for (int i = 0; i < InputBundle.CAPACITY; i++)
            {
                if (!owner.InputHistory.TryGetValue(tick - i, out InputPayload input)) break;
                bundle.SetPayload(count++, input);
            }

            bundle.Count = (byte)count;
            owner.Network.InputRPC(bundle);
        }
    }
}