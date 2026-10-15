using Project02.Server;
using Unity.Netcode;
using UnityEngine;

namespace Project02.Player
{
    public class PlayerServer : NetworkBehaviour
    {
        [SerializeField] private PlayerController _controller;

        private readonly InputPayload[] _inputBuffer = new InputPayload[BUFFER_SIZE];
        private InputPayload _lastInput;
        private int _consumedTick = -1;
        private int _latestTick = -1;
        private bool _started;

        private const int START_BUFFER = 2;
        private const int BUFFER_SIZE = 64;

        private void Awake()
        {
            for (int i = 0; i < BUFFER_SIZE; i++)
            {
                _inputBuffer[i].Tick = -1;
            }
        }

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
                // 첫 입력들이 몇 개 쌓일 때까지 기다림. 바로 시작하면 버퍼가 0이라 곧바로 굶음
                if (_latestTick - _consumedTick < START_BUFFER) return;
                _started = true;
            }

            InputPayload input = Dequeue();
            if (_controller.Step(input, deltaTime, out Shot shot))
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
            InputPayload slot = _inputBuffer[tick & BUFFER_SIZE - 1];

            InputPayload input;
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
                    continue;
                }

                if (payload.Tick > _consumedTick + BUFFER_SIZE)
                {
                    Resync(payload.Tick);
                }

                _inputBuffer[payload.Tick & BUFFER_SIZE - 1] = payload;
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

        private void Fire(in InputPayload input, in Shot shot)
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