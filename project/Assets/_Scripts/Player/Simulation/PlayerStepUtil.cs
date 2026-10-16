using UnityEngine;

namespace Project02.Player
{
    // 유틸(쓰기 많음): 한 틱 시뮬레이션. 호출처는 PredictSystem, ReconcileSystem, ServerPlayerSystem뿐
    public static class PlayerStepUtil
    {
        public static bool Step(PlayerSimulation sim, in InputPayload input, ulong ownerId, float deltaTime, Transform root, out ShotInfo shot)
        {
            PlayerMovement.Step(sim.Movement, ref sim.State.Movement, ref sim.State.Stamina, input, sim.MaxEnergy, deltaTime, out bool isRunning);
            sim.IsRunning = isRunning;
            sim.State.Tick = input.Tick;

            bool fired = WeaponFire.TryFire(sim.Weapon, ref sim.State.Weapon, input, isRunning, ownerId, deltaTime, out shot);

            root.SetPositionAndRotation(sim.State.Movement.Position, Quaternion.Euler(0.0f, input.YawInput, 0.0f));
            return fired;
        }

        public static void Restore(PlayerSimulation sim, in PlayerSimulationState state, Transform root)
        {
            sim.State = state;
            root.position = state.Movement.Position;
        }
    }
}