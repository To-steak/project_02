using UnityEngine;

namespace Project02.Player
{
    // 유틸(쓰기 많음): 한 틱 시뮬레이션. 데이터만 바꾼다 (Unity 객체 부수효과 없음)
    // 호출처: PredictSystem, ReconcileSystem, ServerPlayerSystem
    public static class PlayerStepUtil
    {
        public static bool Step(PlayerSimulation sim, in InputPayload input, ulong ownerId, float deltaTime, out ShotInfo shot)
        {
            PlayerMovement.Step(sim.Movement, ref sim.State.Movement, ref sim.State.Stamina, input, deltaTime, out bool isRunning);
            sim.IsRunning = isRunning;
            sim.State.Tick = input.Tick;
            sim.Yaw = input.YawInput;
            sim.Pitch = input.PitchInput;
            sim.Move = input.MoveInput;

            return WeaponFire.TryFire(sim.Weapon, ref sim.State.Weapon, input, isRunning, ownerId, deltaTime, out shot);
        }

        public static void Restore(PlayerSimulation sim, in PlayerSimulationState state)
        {
            sim.State = state;
        }

        public static void WriteTransform(Transform root, PlayerSimulation sim)
        {
            root.SetPositionAndRotation(sim.State.Movement.Position, Quaternion.Euler(0.0f, sim.Yaw, 0.0f));
        }
    }
}