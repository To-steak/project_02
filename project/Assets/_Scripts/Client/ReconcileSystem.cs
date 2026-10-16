using Project02.Core;
using Project02.Player;
using UnityEngine;

namespace Project02.Client
{
    public static class ReconcileSystem
    {
        private const float POSITION_THRESHOLD = 1.0f;
        private const float ENERGY_THRESHOLD = 0.5f;
        private const float BLOOM_THRESHOLD = 0.1f;
        private const float BUFFER_SMOOTHING = 0.1f;

        public static void Process(ClientWorld world)
        {
            PlayerOwner owner = world.Local;
            if (!owner.HasPendingState) return;

            StatePayload payload = owner.PendingState;
            owner.HasPendingState = false;

            ref ClientClock clock = ref world.Clock;
            clock.SmoothedBuffer = Mathf.Lerp(clock.SmoothedBuffer, payload.InputBuffer, BUFFER_SMOOTHING);

            PlayerSimulationState server = payload.State;
            if (!owner.StateHistory.TryGetValue(server.Tick, out PlayerSimulationState predicted)) return;
            if (!IsMispredicted(predicted, server)) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            owner.DEBUG_ReconcileCount++;
            Debug.LogWarning($"reconcile at tick {server.Tick} " + $"pos {Vector3.Distance(predicted.Movement.Position, server.Movement.Position):F3} " + $"energy {predicted.Stamina.Energy:F2}/{server.Stamina.Energy:F2} max {predicted.Stamina.MaxEnergy:F0}/{server.Stamina.MaxEnergy:F0} " + $"nextFire {predicted.Weapon.NextFireTick}/{server.Weapon.NextFireTick} " + $"bloom {predicted.Weapon.Bloom:F2}/{server.Weapon.Bloom:F2}");
#endif
            PlayerController controller = owner.Controller;
            PlayerSimulation sim = controller.Simulation;
            PlayerStepUtil.Restore(sim, server);

            for (int tick = server.Tick + 1; tick < clock.Tick; tick++)
            {
                if (!owner.InputHistory.TryGetValue(tick, out InputPayload input)) break;

                PlayerStepUtil.Step(sim, input, controller.OwnerClientId, SimulationClock.DeltaTime, out _);
                owner.StateHistory.SetValue(tick, sim.State);
            }
        }

        private static bool IsMispredicted(in PlayerSimulationState predicted, in PlayerSimulationState server)
        {
            bool position = Vector3.Distance(predicted.Movement.Position, server.Movement.Position) >= POSITION_THRESHOLD;
            bool energy = Mathf.Abs(predicted.Stamina.Energy - server.Stamina.Energy) >= ENERGY_THRESHOLD
                          || predicted.Stamina.IsExhausted != server.Stamina.IsExhausted
                          || !Mathf.Approximately(predicted.Stamina.MaxEnergy, server.Stamina.MaxEnergy);   // 레벨업
            bool weapon = predicted.Weapon.EquippedId != server.Weapon.EquippedId
                          || predicted.Weapon.NextFireTick != server.Weapon.NextFireTick
                          || Mathf.Abs(predicted.Weapon.Bloom - server.Weapon.Bloom) >= BLOOM_THRESHOLD;
            return position || energy || weapon;
        }
    }
}