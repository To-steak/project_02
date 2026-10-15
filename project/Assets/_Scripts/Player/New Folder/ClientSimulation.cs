using Project02.Character;
using Project02.Player;
using UnityEngine;

namespace Project02.Client
{
    public static class CameraSystem
    {
        public static void Update(PlayerOwner owner, float deltaTime)
        {
            PlayerController controller = owner.Controller;
            PlayerCamera camera = controller.PlayerCamera;
            PlayerWeapon weapon = controller.PlayerWeapon;

            camera.Look(controller.PlayerInput.LookInput, InputService.Instance.MouseSensitivity, deltaTime);

            float zoom = weapon.HasWeapon ? weapon.Settings.AdsZoom : 1.0f;
            camera.ApplyAim(controller.PlayerInput.AimInput, zoom, deltaTime);
        }
    }

    public static class ViewSystem
    {
        public static void Present(PlayerOwner owner, float alpha, float deltaTime)
        {
            PlayerController controller = owner.Controller;
            PlayerCamera camera = controller.PlayerCamera;
            PlayerLocomotion locomotion = controller.PlayerLocomotion;

            owner.View.Present(camera.Yaw, camera.Pitch, camera.AimPoint, controller.PlayerInput.MoveInput, locomotion.Movement, locomotion.IsRunning, alpha, deltaTime);
        }
    }

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

    public static class ClockSystem
    {
        private const float TARGET_BUFFER = 3.0f;
        private const float MAX_DILATION = 0.05f;
        private const float DILATION_GAIN = 0.025f;
        private const int MAX_TICKS_PER_FRAME = 5;

        public static ClientClock Create() => new ClientClock
        {
            Tick = 0,
            Accumulator = 0.0f,
            Interval = SimulationClock.DeltaTime,
            SmoothedBuffer = TARGET_BUFFER,
        };

        public static int Advance(ref ClientClock clock, float frameTime)
        {
            // 버퍼가 모자라면 짧게(빨리), 남으면 길게(느리게)
            float error = TARGET_BUFFER - clock.SmoothedBuffer;
            float scale = 1.0f - Mathf.Clamp(error * DILATION_GAIN, -MAX_DILATION, MAX_DILATION);
            clock.Interval = SimulationClock.DeltaTime * scale;

            clock.Accumulator += frameTime;
            int ticks = 0;
            while (clock.Accumulator >= clock.Interval && ticks < MAX_TICKS_PER_FRAME)
            {
                clock.Accumulator -= clock.Interval;
                ticks++;
            }

            if (ticks == MAX_TICKS_PER_FRAME)
            {
                clock.Accumulator = 0.0f;
            }

            return ticks;
        }
    }

    public static class ReconcileSystem
    {
        private const float POSITION_THRESHOLD = 1.0f;
        private const float ENERGY_THRESHOLD = 0.5f;
        private const float BLOOM_THRESHOLD = 0.1f;
        private const float BUFFER_SMOOTHING = 0.1f;

        public static void Process(PlayerOwner owner, ref ClientClock clock)
        {
            if (!owner.HasPendingState) return;
            StatePayload payload = owner.PendingState;
            owner.HasPendingState = false;

            clock.SmoothedBuffer = Mathf.Lerp(clock.SmoothedBuffer, payload.InputBuffer, BUFFER_SMOOTHING);

            PlayerSimulationState server = payload.State;
            PlayerSimulationState predicted = owner.StateHistory[server.Tick & PlayerOwner.BUFFER_MASK];
            if (predicted.Tick != server.Tick) return;

            if (!IsMispredicted(predicted, server)) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            owner.ReconcileCount++;
            Debug.LogWarning($"reconcile at tick {server.Tick} " + $"pos {Vector3.Distance(predicted.Movement.Position, server.Movement.Position):F3} " + $"energy {predicted.Stamina.Energy:F2}/{server.Stamina.Energy:F2} " + $"nextFire {predicted.Weapon.NextFireTick}/{server.Weapon.NextFireTick} " + $"bloom {predicted.Weapon.Bloom:F2}/{server.Weapon.Bloom:F2}");
#endif
            PlayerController controller = owner.Controller;
            controller.Restore(server);

            for (int tick = server.Tick + 1; tick < clock.Tick; tick++)
            {
                controller.Step(owner.InputHistory[tick & PlayerOwner.BUFFER_MASK], SimulationClock.DeltaTime, out _);
                owner.StateHistory[tick & PlayerOwner.BUFFER_MASK] = controller.Capture(tick);
            }
        }

        private static bool IsMispredicted(in PlayerSimulationState predicted, in PlayerSimulationState server)
        {
            bool position = Vector3.Distance(predicted.Movement.Position, server.Movement.Position) >= POSITION_THRESHOLD;
            bool energy = Mathf.Abs(predicted.Stamina.Energy - server.Stamina.Energy) >= ENERGY_THRESHOLD || predicted.Stamina.IsExhausted != server.Stamina.IsExhausted;
            bool weapon = predicted.Weapon.NextFireTick != server.Weapon.NextFireTick || Mathf.Abs(predicted.Weapon.Bloom - server.Weapon.Bloom) >= BLOOM_THRESHOLD;
            return position || energy || weapon;
        }
    }

    public struct ClientClock
    {
        public int Tick;
        public float Accumulator;
        public float Interval;
        public float SmoothedBuffer;
    }

    public class ClientSimulation : MonoBehaviour
    {
        public static ClientSimulation Instance { get; private set; }

        private ClientClock _clock;
        private PlayerOwner _local;

        private void Awake() => Instance = this;
        private void OnDestroy() { if (Instance == this) Instance = null; }

        public void Register(PlayerOwner owner)
        {
            _local = owner;
            _clock = ClockSystem.Create();
        }

        public void Unregister(PlayerOwner owner)
        {
            if (_local == owner) _local = null;
        }

        private void Update()
        {
            if (_local == null) return;
            float frameTime = Time.deltaTime;

            // 1. CameraSystem
            CameraSystem.Update(_local, frameTime);

            // 2. ReconcileSystem.Process(_local, ref _clock)
            ReconcileSystem.Process(_local, ref _clock);

            // 3. int ticks = ClockSystem.Advance(ref _clock, frameTime)
            int ticks = ClockSystem.Advance(ref _clock, frameTime);
            for (int i = 0; i < ticks; i++)                            // 4
            {
                PredictSystem.Tick(_local, _clock.Tick, SimulationClock.DeltaTime);
                _clock.Tick++;
            }
        }

        private void LateUpdate()
        {
            if (_local == null) return;
            // 5. alpha = _clock.Accumulator / _clock.Interval → ViewSystem
            float alpha = Mathf.Clamp01(_clock.Accumulator / _clock.Interval);
            ViewSystem.Present(_local, alpha, Time.deltaTime);
        }
        
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (_local == null) return;
            GUIStyle style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperRight };
            style.normal.textColor = Color.black;
            float x = Screen.width - 310f;

            GUI.Label(new Rect(x, 150, 300, 20), $"tick: {_clock.Tick}", style);
            GUI.Label(new Rect(x, 170, 300, 20), $"buffer: {_clock.SmoothedBuffer:F2}", style);
            GUI.Label(new Rect(x, 190, 300, 20), $"interval: {_clock.Interval * 1000.0f:F2}ms", style);
        }
#endif
    }
}