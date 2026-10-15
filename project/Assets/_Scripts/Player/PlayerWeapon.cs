using Project02.Weapon;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Project02.Player
{
    public class PlayerWeapon : MonoBehaviour
    {
        public WeaponView View => _view;
        public SOWeapon Settings => _settings;
        public Vector3 Muzzle => _view != null ? _view.Muzzle : Origin;
        public Vector3 Origin => _origin.position;
        public bool HasWeapon => _settings != null;
        public WeaponState State => _state;

        [SerializeField] private SOCatalog _catalog;
        [SerializeField] private Transform _origin;
        [SerializeField] private Transform _socket;

        [SerializeField] private RigBuilder _rigBuilder;
        [SerializeField] private TwoBoneIKConstraint _rightHandIK;
        [SerializeField] private TwoBoneIKConstraint _leftHandIK;

        private SOWeapon _settings;
        private WeaponView _view;
        private WeaponState _state;

        public bool TryFire(in InputPayload input, bool isRunning, ulong ownerId, float deltaTime, out ShotInfo shot)
        {
            return WeaponFire.TryFire(_settings, ref _state, input, isRunning, ownerId, deltaTime, out shot);
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

            _settings = _catalog.Get(id);
            _state = default;

            bool hasView = createView && _settings != null && _settings.Prefab != null;
            if (hasView)
            {
                _view = Instantiate(_settings.Prefab, _socket);
                _view.Initialize(_settings);
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

        public void Restore(in WeaponState state)
        {
            _state = state;
        }
    }
}