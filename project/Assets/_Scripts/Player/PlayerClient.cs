using Unity.Netcode;
using UnityEngine;

public class PlayerClient : NetworkBehaviour
{
    [SerializeField] private PlayerController _controller;
    private int _tick = 0;

    private const int BUFFER_SIZE = 1024;
    private const int BUFFER_MASK = BUFFER_SIZE - 1;
    private const float RECONCILE_THRESHOLD = 0.1f;
    private const float MP_THRESHOLD = 0.5f;

    private readonly InputPayload[] _inputHistory = new InputPayload[BUFFER_SIZE];
    private readonly StatePayload[] _stateHistory = new StatePayload[BUFFER_SIZE];

    private VisualInterpolator<float> _pitch = new VisualInterpolator<float>(Mathf.Lerp);

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            _controller.PlayerInput.Initialize();
            _controller.PlayerInput.Active();
            _controller.PlayerInput.Enable();
            _controller.PlayerCamera.Initialize(_controller.PlayerSettings);
            _controller.PlayerVisual.Initialize(transform.position, _controller.PlayerSettings);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            _pitch.Reset(_controller.Pitch.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            _controller.PlayerInput.Inactive();
            _controller.PlayerInput.Disable();
            _controller.PlayerCamera.Release();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            PlayerCamera camera = _controller.PlayerCamera;
            PlayerWeapon weapon = _controller.PlayerWeapon;

            camera.Look(_controller.PlayerInput.LookInput, InputService.Instance.MouseSensitivity, Time.deltaTime);
            _controller.PlayerLocomotion.Rotate(camera.Yaw);

            float zoom = weapon.HasWeapon ? weapon.Data.AdsZoom : 1.0f;
            camera.ApplyAim(_controller.PlayerInput.AimInput, zoom, Time.deltaTime);
            _controller.PlayerVisual.Face(camera.AimPoint, weapon.Muzzle, Time.deltaTime);

            var motion = _controller.PlayerLocomotion.State;
            _controller.PlayerAnimation.SetAirborne(motion.IsGrounded, motion.VerticalSpeed);
            _controller.PlayerAnimation.SetMoveBlendTree(_controller.PlayerInput.MoveInput, _controller.PlayerLocomotion.IsRunning, Time.deltaTime);
        }
        else
        {
            float pitch = _pitch.Interpolate();
            _controller.PlayerCamera.ApplyPitch(pitch);
            _controller.PlayerCamera.ApplyAimTarget(pitch);
        }
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            Vector3 aimPoint = _controller.PlayerCamera.GetAimPoint();
            InputPayload input = _controller.PlayerInput.Capture(_tick, _controller.PlayerCamera.Yaw, _controller.PlayerCamera.Pitch, aimPoint);
            _inputHistory[_tick & BUFFER_MASK] = input;

            _controller.Simulate(input);

            PlayerWeapon weapon = _controller.PlayerWeapon;
            if (weapon.TryFire(input, _controller.PlayerLocomotion.IsRunning, OwnerClientId, out PlayerWeapon.Shot shot))
            {
                SpawnBulletVisual(aimPoint, shot.Spread);
                _controller.PlayerCamera.AddRecoil(weapon.RollRecoil(shot.Stance));
                weapon.PlayFeedback(shot.Stance);
            }

            StatePayload state = _controller.PlayerLocomotion.Capture(_tick);
            _stateHistory[_tick & BUFFER_MASK] = state;

            _controller.PlayerVisual.Record();

            int count = Mathf.Min(InputBundle.CAPACITY, _tick + 1);
            InputBundle bundle = new InputBundle { Count = (byte)count };
            for (int i = 0; i < count; i++)
            {
                bundle.Set(i, _inputHistory[(_tick - i) & BUFFER_MASK]);
            }
            _controller.PlayerServer.InputRPC(bundle);

            _tick++;
        }
        else
        {
            _pitch.Record(_controller.Pitch.Value);
        }
    }

    private void LateUpdate()
    {
        if (IsOwner)
        {
            _controller.PlayerVisual.Interpolate();
        }
    }

    [Rpc(SendTo.Owner, Delivery = RpcDelivery.Unreliable)]
    public void StateRPC(StatePayload payload)
    {
        var predicted = _stateHistory[payload.Tick & BUFFER_MASK];
        if (predicted.Tick != payload.Tick)
        {
            return;
        }

        bool positionMiss = Vector3.Distance(predicted.Position, payload.Position) >= RECONCILE_THRESHOLD;
        bool mpMiss = Mathf.Abs(predicted.Mp - payload.Mp) >= MP_THRESHOLD || predicted.IsExhausted != payload.IsExhausted;
        if (positionMiss || mpMiss)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            DEBUG_RECONCILE++;
#endif
            _controller.PlayerLocomotion.RollbackState(payload);

            for (int tick = payload.Tick + 1; tick < _tick; tick++)
            {
                _controller.Simulate(_inputHistory[tick & BUFFER_MASK]);
                _stateHistory[tick & BUFFER_MASK] = _controller.PlayerLocomotion.Capture(tick);
            }

            Debug.LogWarning($"reconcile at tick {payload.Tick}, error {Vector3.Distance(predicted.Position, payload.Position):F4}, y diff {payload.Position.y - predicted.Position.y:F4}");
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void FireRPC(Vector3 aimPoint, Vector2 spread)
    {
        if (IsOwner)
        {
            return;
        }

        SpawnBulletVisual(aimPoint, spread);
    }

    private void SpawnBulletVisual(Vector3 aimPoint, Vector2 spread)
    {
        if (!_controller.PlayerWeapon.HasWeapon)
        {
            return;
        }

        SOWeapon data = _controller.PlayerWeapon.Data;
        SOBullet bulletData = data.Bullet;
        if (bulletData == null)
        {
            return;
        }

        Vector3 from = _controller.PlayerWeapon.Muzzle;
        Vector3 direction = PlayerWeapon.GetDirection(from, aimPoint, spread);
        float distance = Mathf.Min(Vector3.Distance(from, aimPoint), data.Range);
        ProjectileVisualService.Instance.Fire(from, from + direction * distance, bulletData.Velocity);
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private int DEBUG_RECONCILE;
    private void OnGUI()
    {
        if (!IsOwner) return;

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
    }
#endif
}