using UnityEngine;
using Project02.Core;
using Project02.World;

namespace Project02.Player
{
    public static class ReconcileSystem
    {
        private const float POSITION_THRESHOLD = 1.0f;
        private const float ENERGY_THRESHOLD = 0.5f;
        private const float BLOOM_THRESHOLD = 0.1f;
        private const float BUFFER_SMOOTHING = 0.1f;

        public static void Process(ClientWorld world, float rttTicks)
        {
            PlayerOwnerAuthoring owner = world.Local;
            PlayerNetwork network = owner.Network;
            if (!network.HasPendingState) return;

            StatePayload payload = network.PendingState;
            network.HasPendingState = false;

            ref ClientClock clock = ref world.Clock;
            PlayerSimulationState server = payload.State;

            // 0. 뒤처짐 복구: 서버가 이미 우리 현재 틱 이후까지 (복제 입력으로) 소비함 → 지금부터 보내는 입력은 서버에서 '과거'라 버려진다. 시계 팽창(최대 5%)으로는 못 따라잡으니 점프
            if (server.Tick >= clock.Tick)
            {
                Resync(owner, ref clock, server, rttTicks);
                return;
            }
            clock.SmoothedBuffer = Mathf.Lerp(clock.SmoothedBuffer, payload.InputBuffer, BUFFER_SMOOTHING);

            if (!owner.StateHistory.TryGetValue(server.Tick, out PlayerSimulationState predicted)) return;
            if (!IsMispredicted(predicted, server)) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            owner.DEBUG_ReconcileCount++;
            Debug.LogWarning($"reconcile at tick {server.Tick} " + $"pos {Vector3.Distance(predicted.Movement.Position, server.Movement.Position):F3} " + $"energy {predicted.Stamina.Energy:F2}/{server.Stamina.Energy:F2} max {predicted.Stamina.MaxEnergy:F0}/{server.Stamina.MaxEnergy:F0} " + $"nextFire {predicted.Weapon.NextFireTick}/{server.Weapon.NextFireTick} " + $"bloom {predicted.Weapon.Bloom:F2}/{server.Weapon.Bloom:F2}");
#endif
            PlayerSimulation sim = owner.Simulation;
            PlayerStepUtil.Restore(sim, server);

            for (int tick = server.Tick + 1; tick < clock.Tick; tick++)
            {
                if (!owner.InputHistory.TryGetValue(tick, out InputPayload input)) break;

                PlayerStepUtil.Step(sim, input, owner.OwnerClientId, SimulationClock.DeltaTime, out _);
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

        // 서버 상태에서 다시 시작하고, 입력이 서버에 '미래'로 도착하도록 RTT + 목표 버퍼만큼 앞으로
        private static void Resync(PlayerOwnerAuthoring owner, ref ClientClock clock, in PlayerSimulationState server, float rttTicks)
        {
            int lead = Mathf.CeilToInt(rttTicks + ClockSystem.TARGET_BUFFER) + 1;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"clock resync: client {clock.Tick} → {server.Tick + lead} (server {server.Tick}, rtt {rttTicks:F1} ticks)");
#endif
            clock.Tick = server.Tick + lead;
            clock.Accumulator = 0.0f;
            clock.SmoothedBuffer = ClockSystem.TARGET_BUFFER;

            PlayerStepUtil.Restore(owner.Simulation, server);
            owner.InputHistory.Clear();   // 건너뛴 구간의 기록은 의미 없음
            owner.StateHistory.Clear();
        }
    }
}