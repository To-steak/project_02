using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Project02.Weapon;

namespace Project02.Player
{
    // 프록시: 장착 무기의 뷰 생성 + IK. 무기 설정의 정본은 PlayerSimulation.Weapon
    public class PlayerWeaponView : NetworkBehaviour
    {
        public WeaponView View => _view;
        public Vector3 Muzzle => _view != null ? _view.Muzzle : _origin.position;

        [SerializeField] private PlayerNetwork _network;
        [SerializeField] private SOPlayer _player;   // Catalog
        [SerializeField] private Transform _origin;
        [SerializeField] private Transform _socket;

        [SerializeField] private RigBuilder _rigBuilder;
        [SerializeField] private TwoBoneIKConstraint _rightHandIK;
        [SerializeField] private TwoBoneIKConstraint _leftHandIK;

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

        private void Equip(int id, bool createView)
        {
            if (_view != null)
            {
                Destroy(_view.gameObject);
                _view = null;
            }

            SOWeapon settings = _player.Catalog.Get(id);

            bool hasView = createView && settings != null && settings.Prefab != null;
            if (hasView)
            {
                _view = Instantiate(settings.Prefab, _socket);
                _view.Initialize(settings);
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