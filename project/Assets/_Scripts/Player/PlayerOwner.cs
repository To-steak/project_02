using Unity.Netcode;
using UnityEngine;

namespace Project02.Player
{
    public class PlayerOwner : NetworkBehaviour
    {
        [SerializeField] private PlayerController _controller;
        [SerializeField] private PlayerView _view;

        private int _tick;
        private const int BUFFER_SIZE = 1024;
        private const float RECONCILE_THRESHOLD = 1.0f;
        private const float MP_THRESHOLD = 0.5f;

        private readonly InputPayload[] _inputHistory = new InputPayload[BUFFER_SIZE];
        private readonly StatePayload[] _stateHistory = new StatePayload[BUFFER_SIZE];

        public override void OnNetworkSpawn()
        {
            enabled = IsOwner;
            if (!enabled)
            {
                return;
            }

            _controller.PlayerInput.Initialize();
            _controller.PlayerInput.Active();
            _controller.PlayerInput.Enable();
            _controller.PlayerCamera.Initialize(_controller.PlayerSettings);
            _controller.PlayerVisual.Initialize(transform.position, _controller.PlayerSettings);
        }

        public override void OnNetworkDespawn()
        {
            if (!enabled)
            {
                return;
            }

            _controller.PlayerInput.Inactive();
            _controller.PlayerInput.Disable();
            _controller.PlayerCamera.Release();
        }

        private void Update()
        {
            PlayerCamera camera = _controller.PlayerCamera;
            PlayerWeapon weapon = _controller.PlayerWeapon;

            camera.Look(_controller.PlayerInput.LookInput, InputService.Instance.MouseSensitivity, Time.deltaTime);

            float zoom = weapon.HasWeapon ? weapon.Data.AdsZoom : 1.0f;
            camera.ApplyAim(_controller.PlayerInput.AimInput, zoom, Time.deltaTime);
        }

        private void LateUpdate()
        {
            PlayerCamera camera = _controller.PlayerCamera;
            PlayerLocomotion locomotion = _controller.PlayerLocomotion;

            _view.Present(camera.Yaw, camera.Pitch, camera.AimPoint, _controller.PlayerInput.MoveInput, locomotion.Movement, locomotion.IsRunning, Time.deltaTime);
        }

        private void FixedUpdate()
        {
            PlayerCamera camera = _controller.PlayerCamera;
            PlayerWeapon weapon = _controller.PlayerWeapon;

            Vector3 aimPoint = camera.AimPoint;
            InputPayload input = _controller.PlayerInput.Capture(_tick, camera.Yaw, camera.Pitch, aimPoint);
            _inputHistory[_tick & BUFFER_SIZE - 1] = input;

            if (_controller.Step(input, out Shot shot))
            {
                _view.OnFired(shot, aimPoint);
                camera.AddRecoil(weapon.View.RollRecoil(shot.Stance));
            }

            _stateHistory[_tick & BUFFER_SIZE - 1] = _controller.PlayerLocomotion.Capture(_tick);

            _view.OnTick(_controller.PlayerLocomotion.Movement);
            SendInputs();
            _tick++;
        }

        private void SendInputs()
        {
            int count = Mathf.Min(InputBundle.CAPACITY, _tick + 1);
            InputBundle bundle = new InputBundle { Count = (byte)count };
            for (int i = 0; i < count; i++)
            {
                bundle.Set(i, _inputHistory[(_tick - i) & BUFFER_SIZE - 1]);
            }
            _controller.PlayerServer.InputRPC(bundle);
        }

        [Rpc(SendTo.Owner, Delivery = RpcDelivery.Unreliable)]
        public void StateRPC(StatePayload payload)
        {
            var predicted = _stateHistory[payload.Tick & BUFFER_SIZE - 1];
            if (predicted.Tick != payload.Tick)
            {
                return;
            }

            bool positionMiss = Vector3.Distance(predicted.Position, payload.Position) >= RECONCILE_THRESHOLD;
            bool mpMiss = Mathf.Abs(predicted.Mp - payload.Mp) >= MP_THRESHOLD || predicted.IsExhausted != payload.IsExhausted;
            if (!positionMiss && !mpMiss)
            {
                return;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            DEBUG_RECONCILE++;
            Debug.LogWarning($"reconcile at tick {payload.Tick}, error {Vector3.Distance(predicted.Position, payload.Position):F4}, y diff {payload.Position.y - predicted.Position.y:F4}");
#endif

            _controller.PlayerLocomotion.RollbackState(payload);

            for (int tick = payload.Tick + 1; tick < _tick; tick++)
            {
                _controller.Simulate(_inputHistory[tick & BUFFER_SIZE - 1]);
                _stateHistory[tick & BUFFER_SIZE - 1] = _controller.PlayerLocomotion.Capture(tick);
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private int DEBUG_RECONCILE;
        private void OnGUI()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = Color.black;
            style.alignment = TextAnchor.UpperRight;

            float width = 300f;
            float height = 20f;
            float paddingRight = 10f;
            float xPos = Screen.width - width - paddingRight;
            float fps = 1.0f / Time.unscaledDeltaTime;

            GUI.Label(new Rect(xPos, 30, width, height), $"reconcile: {DEBUG_RECONCILE}", style);
            GUI.Label(new Rect(xPos, 50, width, height), $"tick: {_tick}", style);
            GUI.Label(new Rect(xPos, 70, width, height), $"rtt: {NetworkManager.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.ServerClientId)}ms", style);
            GUI.Label(new Rect(xPos, 90, width, height), $"fps: {fps:F1}", style);
            GUI.Label(new Rect(xPos, 110, width, height), $"energy: {_controller.PlayerLocomotion.Energy:F1}", style);
            GUI.Label(new Rect(xPos, 130, width, height), $"yaw: {_controller.PlayerCamera.Yaw:F1}", style);
        }
#endif
    }
}