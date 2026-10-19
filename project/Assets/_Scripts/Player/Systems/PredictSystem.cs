using Project02.Weapon;
using Project02.World;

namespace Project02.Player
{
    public static class PredictSystem
    {
        public static void Tick(ClientWorld world, float deltaTime, float now)
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
            owner.SendTimes.SetValue(tick, now);   // 이 틱 입력이 처음 나가는 시각

            // 2. 시뮬레이션
            bool predict = true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            predict = world.Probe.PredictionEnabled;   // 디버그: 꺼지면 서버 결과만 보여 준다 (ReconcileSystem)
#endif
            if (predict)
            {
                if (PlayerStepUtil.Step(sim, input, owner.OwnerClientId, deltaTime, out ShotInfo shot))
                {
                    owner.View.OnFired(shot, input.AimPoint);
                    camera.Recoil += WeaponFeedbackUtil.RollRecoil(sim.Weapon, shot.Stance);   // 발사 성공 = sim.Weapon != null
                }
            }
            else
            {
                sim.Yaw = input.YawInput;     // 회전은 화면 표시용으로만 따라간다
                sim.Pitch = input.PitchInput;
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