using UnityEngine;
using Project02.Character;
using Project02.Core;
using Project02.World;

namespace Project02.Player
{
    public static class ReconcileSystem
    {
        // 서버 상태는 float 그대로 전송되고 클라가 그대로 복원한다. 결정적이라면 이 허용치에서도 호스트·지연 0의 리컨사일은 0이다.
        // 어긋나면 허용치를 키우지 않고 원인을 고친다 (같은 함수, 같은 입력, 같은 설정 에셋)
        private const float POSITION_THRESHOLD = 0.001f;
        private const float ENERGY_THRESHOLD = 0.001f;
        private const float BLOOM_THRESHOLD = 0.001f;
        private const float BUFFER_SMOOTHING = 0.1f;
        private const float RTT_SMOOTHING = 0.1f;

        public static void Process(ClientWorld world, float fallbackRttTicks, float now)
        {
            PlayerOwnerAuthoring owner = world.Local;
            PlayerNetwork network = owner.Network;
            if (!network.HasPendingState) return;

            StatePayload payload = network.PendingState;
            network.HasPendingState = false;

            // ack는 다른 분기보다 먼저 (재동기화·예측 끔이어도 서버가 받은 범위는 갱신). 늦게 온 옛 값으로 내려가지 않게 Max
            world.AckTick = Mathf.Max(world.AckTick, payload.AckTick);

            ref ClientClock clock = ref world.Clock;
            PlayerSimulationState server = payload.State;

            // 왕복 시간 = (받은 시각 − 그 틱 입력을 보낸 시각) − 서버 버퍼에서 기다린 시간
            if (owner.SendTimes.TryGetValue(server.Tick, out float sent))
            {
                float sample = (now - sent) / SimulationClock.DeltaTime - Mathf.Max(0, payload.InputBuffer);
                sample = Mathf.Max(0.0f, sample);
                clock.RttTicks = clock.RttTicks < 0.0f ? sample : Mathf.Lerp(clock.RttTicks, sample, RTT_SMOOTHING);
            }
            float rttTicks = clock.RttTicks >= 0.0f ? clock.RttTicks : fallbackRttTicks;   // 표본 전에는 Transport 값

            // 0. 뒤처짐 복구 (기존 그대로, rttTicks 사용)
            if (server.Tick >= clock.Tick)
            {
                Resync(world, server, rttTicks);
                return;
            }
            clock.SmoothedBuffer = Mathf.Lerp(clock.SmoothedBuffer, payload.InputBuffer, BUFFER_SMOOTHING);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!world.Probe.PredictionEnabled)
            {
                PlayerStepUtil.Restore(owner.Simulation, server);   // 예측 끔: 서버 결과를 그대로 보여 준다
                return;
            }
#endif
            if (!owner.StateHistory.TryGetValue(server.Tick, out PlayerSimulationState predicted)) return;
            if (!IsMispredicted(predicted, server)) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            owner.DEBUG_ReconcileCount++;
            LogMisprediction(predicted, server);
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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // 위치는 축마다 (예측 − 서버). 발밑 판정 차이를 가르려고 수직 속도와 접지도 함께 찍는다
        private static void LogMisprediction(in PlayerSimulationState predicted, in PlayerSimulationState server)
        {
            MovementState p = predicted.Movement;
            MovementState s = server.Movement;
            Vector3 delta = p.Position - s.Position;

            Debug.LogWarning(
                $"reconcile at tick {server.Tick} " +
                $"pos d({delta.x:F4}, {delta.y:F4}, {delta.z:F4}) |{delta.magnitude:F4}| " +
                $"vy {p.VerticalSpeed:F4}/{s.VerticalSpeed:F4} grounded {p.IsGrounded}/{s.IsGrounded} " +
                $"energy {predicted.Stamina.Energy:F4}/{server.Stamina.Energy:F4} max {predicted.Stamina.MaxEnergy:F0}/{server.Stamina.MaxEnergy:F0} exhausted {predicted.Stamina.IsExhausted}/{server.Stamina.IsExhausted} " +
                $"weapon {predicted.Weapon.EquippedId}/{server.Weapon.EquippedId} nextFire {predicted.Weapon.NextFireTick}/{server.Weapon.NextFireTick} " +
                $"bloom {predicted.Weapon.Bloom:F4}/{server.Weapon.Bloom:F4}");
        }
#endif

        // 서버 상태에서 다시 시작하고, 입력이 서버에 '미래'로 도착하도록 RTT + 목표 버퍼만큼 앞으로
        private static void Resync(ClientWorld world, in PlayerSimulationState server, float rttTicks)
        {
            PlayerOwnerAuthoring owner = world.Local;
            ref ClientClock clock = ref world.Clock;
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
            owner.SendTimes.Clear();
            world.AckTick = -1;           // 건너뛴 구간의 ack도 의미 없음
        }
    }
}