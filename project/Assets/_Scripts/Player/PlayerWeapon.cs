using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Project02.Player
{
    public enum FireStance : byte
    {
        Idle,
        Walk,
        Run,
        Ads
    }

    public readonly struct Shot
    {
        public readonly FireStance Stance;
        public readonly Vector2 Spread;

        public Shot(FireStance stance, Vector2 spread)
        {
            Stance = stance;
            Spread = spread;
        }
    }

    public class PlayerWeapon : MonoBehaviour
    {
        public WeaponView View => _view;
        public SOWeapon Data => _data;
        public Vector3 Muzzle => _view != null ? _view.Muzzle : Origin;
        public Vector3 Origin => _origin.position;

        public bool HasWeapon => _data != null;
        public int RapidFireTick => Mathf.Max(1, Mathf.RoundToInt(_data.RapidFire / Time.fixedDeltaTime));

        [SerializeField] private WeaponCatalog _catalog;
        [SerializeField] private Transform _origin;
        [SerializeField] private Transform _socket;

        [SerializeField] private RigBuilder _rigBuilder;
        [SerializeField] private TwoBoneIKConstraint _rightHandIK;
        [SerializeField] private TwoBoneIKConstraint _leftHandIK;

        private SOWeapon _data;
        private WeaponView _view;
        private int _nextFireTick;
        private int _lastFireTick;
        private float _bloom;

        public bool TryFire(in InputPayload input, bool running, ulong ownerId, out Shot shot)
        {
            shot = default;
            if (!HasWeapon)
            {
                return false;
            }

            bool trigger = input.AttackInput || input.AttackPressed;
            if (!trigger || input.Tick < _nextFireTick)
            {
                return false;
            }

            RecoverBloom(input.Tick);

            FireStance stance = GetStance(input, running);
            float angle = GetSpread(stance);
            Vector2 spread = SpreadOffset(ownerId, input.Tick) * angle;

            _bloom = Mathf.Min(_bloom + _data.SpreadPerShot, _data.MaxSpread);
            _lastFireTick = input.Tick;
            _nextFireTick = input.Tick + RapidFireTick;

            shot = new Shot(stance, spread);
            return true;
        }

        public static Vector3 GetDirection(Vector3 from, Vector3 aimPoint, Vector2 spread)
        {
            Quaternion look = Quaternion.LookRotation(aimPoint - from);
            return look * Quaternion.Euler(-spread.y, spread.x, 0.0f) * Vector3.forward;
        }

        public void Equip(int id, bool createView)
        {
            if (_view != null)
            {
                Destroy(_view.gameObject);
                _view = null;
            }

            _data = _catalog.Get(id);
            _nextFireTick = 0;
            _lastFireTick = 0;
            _bloom = 0.0f;

            bool hasView = createView && _data != null && _data.Prefab != null;
            if (hasView)
            {
                _view = Instantiate(_data.Prefab, _socket);
                _view.Initialize(_data);
                _rightHandIK.data.target = _view.RightGrip;
                _leftHandIK.data.target = _view.LeftGrip;
            }

            _rightHandIK.weight = hasView ? 1.0f : 0.0f;
            _leftHandIK.weight = hasView ? 1.0f : 0.0f;

            if (createView)
            {
                _rigBuilder.Build();
            }
        }

        private static FireStance GetStance(in InputPayload input, bool running)
        {
            if (input.AimInput)
            {
                return FireStance.Ads;
            }

            if (input.MoveInput == Vector2.zero)
            {
                return FireStance.Idle;
            }

            return running ? FireStance.Run : FireStance.Walk;
        }

        private float GetSpread(FireStance stance)
        {
            return stance switch
            {
                FireStance.Ads => Mathf.Min(_bloom, _data.MaxSpread) * _data.AdsSpreadMultiplier,
                FireStance.Walk => Mathf.Min(_bloom + _data.HipSpread, _data.MaxSpread) * _data.WalkSpreadMultiplier,
                FireStance.Run => Mathf.Min(_bloom + _data.HipSpread, _data.MaxSpread) * _data.RunSpreadMultiplier,
                _ => Mathf.Min(_bloom + _data.HipSpread, _data.MaxSpread) * _data.IdleSpreadMultiplier,
            };
        }

        private void RecoverBloom(int tick)
        {
            float elapsed = (tick - _lastFireTick) * Time.fixedDeltaTime;
            float recovering = elapsed - _data.SpreadRecoveryDelay;
            if (recovering > 0.0f)
            {
                _bloom = Mathf.Max(0.0f, _bloom - _data.SpreadRecovery * recovering);
            }
        }

        private static Vector2 SpreadOffset(ulong ownerId, int tick)
        {
            uint hash = Hash((uint)ownerId * 0x9E3779B9u ^ (uint)tick);
            float u = (hash & 0xFFFF) / 65535.0f;
            float v = (hash >> 16) / 65535.0f;

            float angle = u * Mathf.PI * 2.0f;
            float radius = Mathf.Sqrt(v);
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        private static uint Hash(uint x)
        {
            x ^= x >> 16;
            x *= 0x7FEB352Du;
            x ^= x >> 15;
            x *= 0x846CA68Bu;
            x ^= x >> 16;
            return x;
        }
    }
}