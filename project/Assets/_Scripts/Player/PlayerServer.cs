using Unity.Netcode;
using UnityEngine;

public class PlayerServer : NetworkBehaviour
{
    [SerializeField] private PlayerController _controller;
    private readonly InputPayload[] _inputBuffer = new InputPayload[BUFFER_SIZE];
    private InputPayload _lastInput;
    private int _consumedTick = -1;
    private int _latestTick = -1;
    private const int BUFFER_SIZE = 64;
    private const int BUFFER_MASK = BUFFER_SIZE - 1;
    private int _nextFireTick;

    private void Awake()
    {
        for (int i = 0; i < BUFFER_SIZE; i++)
        {
            _inputBuffer[i].Tick = -1;
        }
    }

    private void FixedUpdate()
    {
        int consume = _latestTick - _consumedTick > 1 ? 2 : 1;
        for (int i = 0; i < consume; i++)
        {
            if (!TryDequeue(out InputPayload input))
            {
                break;
            }
            _controller.Simulate(input);
            HandleFire(input);

            _controller.Pitch.Value = input.PitchInput;
            StatePayload state = _controller.PlayerLocomotion.Capture(input.Tick);
            _controller.PlayerClient.StateRPC(state);
        }
    }

    private bool TryDequeue(out InputPayload input)
    {
        int tick = _consumedTick + 1;
        if (tick > _latestTick)
        {
            input = default;
            return false;
        }

        InputPayload slot = _inputBuffer[tick & BUFFER_MASK];
        if (slot.Tick == tick)
        {
            input = slot;
            _lastInput = slot;
        }
        else
        {
            input = _lastInput;
            input.Tick = tick;
            input.JumpInput = false;
            input.AttackPressed = false;
        }

        _consumedTick = tick;
        return true;
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
    public void InputRPC(InputBundle bundle)
    {
        for (int i = 0; i < bundle.Count; i++)
        {
            InputPayload payload = bundle[i];
            if (payload.Tick <= _consumedTick)
            {
                continue;
            }

            if (payload.Tick > _consumedTick + BUFFER_SIZE)
            {
                Resync(payload.Tick);
            }

            _inputBuffer[payload.Tick & BUFFER_MASK] = payload;
            if (payload.Tick > _latestTick)
            {
                _latestTick = payload.Tick;
            }
        }
    }

    private void Resync(int tick)
    {
        for (int i = 0; i < BUFFER_SIZE; i++)
        {
            _inputBuffer[i].Tick = -1;
        }

        _consumedTick = tick - 1;
        _latestTick = tick - 1;
    }

    private void HandleFire(in InputPayload input)
    {
        PlayerWeapon weapon = _controller.PlayerWeapon;
        if (!weapon.TryFire(input, ref _nextFireTick))
        {
            return;
        }

        BulletData bullet = weapon.Data.Bullet;
        Vector3 origin = weapon.Origin;
        Vector3 direction = (input.AimPoint - origin).normalized;
        int damage = Mathf.RoundToInt(bullet.Damage * weapon.Data.WeaponCoefficient);

        GameServices.Projectiles.Spawn(origin, direction * bullet.Velocity, bullet.Lifespan, damage, OwnerClientId);
        _controller.PlayerClient.FireRPC(input.AimPoint);
    }
}