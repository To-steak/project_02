using Project02.Player;
using UnityEngine;

namespace Project02.Client
{
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