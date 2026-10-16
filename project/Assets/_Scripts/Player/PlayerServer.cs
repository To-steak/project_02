using Project02.Character;
using Project02.Core;
using Project02.Server;
using Project02.Service;
using Project02.Weapon;
using Unity.Netcode;
using UnityEngine;

namespace Project02.Player
{
    public class PlayerServer : NetworkBehaviour
    {
        public PlayerController Controller => _controller;

        [SerializeField] private PlayerController _controller;

        private readonly TickBuffer<InputPayload> _inputBuffer = new(BUFFER_SIZE);
        private InputPayload _lastInput;
        private int _consumedTick = -1;
        private int _latestTick = -1;
        private bool _started;
        private bool _carryJump;
        private bool _carryAttack;

        private const int START_BUFFER = 2;
        private const int BUFFER_SIZE = 64;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            ServerSimulation.Instance.Register(this);
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && ServerSimulation.Instance != null)
            {
                ServerSimulation.Instance.Unregister(this);
            }
        }

        public void Tick(float deltaTime)
        {
            if (!_started)
            {
                if (_latestTick - _consumedTick < START_BUFFER)
                {
                    return;
                }

                _started = true;
            }

            InputPayload input = Dequeue();
            input.JumpInput |= _carryJump;
            input.AttackPressed |= _carryAttack;
            _carryJump = _carryAttack = false;

            if (_controller.Step(input, deltaTime, out ShotInfo shot))
            {
                Fire(input, shot);
            }

            _controller.Pitch.Value = input.PitchInput;
            StatePayload payload = new StatePayload
            {
                State = _controller.Capture(input.Tick),
                InputBuffer = (sbyte)Mathf.Clamp(_latestTick - _consumedTick, sbyte.MinValue, sbyte.MaxValue),
            };
            _controller.PlayerOwner.StateRPC(payload);
        }

        private InputPayload Dequeue()
        {
            int tick = _consumedTick + 1;

            InputPayload input;
            if (_inputBuffer.TryGetValue(tick, out InputPayload slot))
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
            return input;
        }

        [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
        public void InputRPC(InputBundle bundle)
        {
            for (int i = 0; i < bundle.Count; i++)
            {
                InputPayload payload = bundle[i];
                if (payload.Tick <= _consumedTick)
                {
                    bool lateForDuplicatedTick = !_inputBuffer.TryGetValue(payload.Tick, out _)
                                                 && payload.Tick > _consumedTick - BUFFER_SIZE;
                    if (lateForDuplicatedTick)
                    {
                        _carryJump |= payload.JumpInput;
                        _carryAttack |= payload.AttackPressed;
                        _inputBuffer.SetValue(payload.Tick, payload);
                    }
                    continue;
                }

                if (payload.Tick > _consumedTick + BUFFER_SIZE)
                {
                    Resync(payload.Tick);
                }

                _inputBuffer.SetValue(payload.Tick, payload);
                if (payload.Tick > _latestTick)
                {
                    _latestTick = payload.Tick;
                }
            }
        }

        private void Resync(int tick)
        {
            _inputBuffer.Clear();
            _carryJump = _carryAttack = false;
            _consumedTick = tick - 1;
            _latestTick = tick - 1;
        }

        private void Fire(in InputPayload input, in ShotInfo shot)
        {
            PlayerWeapon weapon = _controller.PlayerWeapon;
            SOWeapon data = weapon.Settings;
            SOBullet bullet = data.Bullet;

            MovementState movement = _controller.PlayerLocomotion.Movement;
            Vector3 origin = WeaponFire.GetOrigin(movement, input.YawInput, _controller.PlayerSettings.FireOriginOffset);
            Vector3 direction = PlayerWeapon.GetDirection(origin, input.AimPoint, shot.Spread);
            float damage = bullet.Damage * data.WeaponCoefficient;
            bool isCritical = WeaponFire.RollCritical(OwnerClientId, input.Tick, data.CriticalChance);

            ProjectileService.Instance.Spawn(new Projectile(origin, direction * bullet.Velocity, damage, OwnerClientId, isCritical, data.Range, data.Falloff));
            _controller.PlayerRemote.FireRPC(input.AimPoint, shot.Spread);
        }
    }
}