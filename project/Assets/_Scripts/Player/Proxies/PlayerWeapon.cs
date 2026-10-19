using Project02.Weapon;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Project02.Player
{
    // 프록시: 장착 무기의 설정 조회 + 뷰 생성 + IK. 발사 상태는 PlayerSim
    public class PlayerWeapon : NetworkBehaviour
    {
        public SOCatalog Catalog => _catalog;
        public WeaponView View => _view;
        public SOWeapon Settings => _settings;
        public Vector3 Muzzle => _view != null ? _view.Muzzle : Origin;
        public Vector3 Origin => _origin.position;
        public bool HasWeapon => _settings != null;

        [SerializeField] private PlayerNetwork _network;
        [SerializeField] private SOCatalog _catalog;
        [SerializeField] private Transform _origin;
        [SerializeField] private Transform _socket;

        [SerializeField] private RigBuilder _rigBuilder;
        [SerializeField] private TwoBoneIKConstraint _rightHandIK;
        [SerializeField] private TwoBoneIKConstraint _leftHandIK;

        private SOWeapon _settings;
        private WeaponView _view;

        protected override void OnNetworkPostSpawn()
        {
            _network.EquippedWeapon.OnValueChanged += OnEquippedChanged;
            Equip(_network.EquippedWeapon.Value, IsClient);
        }

        public override void OnNetworkDespawn()
        {
            _network.EquippedWeapon.OnValueChanged -= OnEquippedChanged;
        }

        private void OnEquippedChanged(int previous, int current) => Equip(current, IsClient);

        public static Vector3 GetDirection(Vector3 from, Vector3 aimPoint, Vector2 spread)
        {
            Quaternion look = Quaternion.LookRotation(aimPoint - from);
            return look * Quaternion.Euler(-spread.y, spread.x, 0.0f) * Vector3.forward;
        }

        private void Equip(int id, bool createView)
        {
            if (_view != null)
            {
                Destroy(_view.gameObject);
                _view = null;
            }

            _settings = _catalog.Get(id);

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
    }
}