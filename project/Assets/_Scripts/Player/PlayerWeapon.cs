using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerWeapon : MonoBehaviour
{
    public WeaponData Data => _data;
    public Vector3 Muzzle => _view != null ? _view.Muzzle : Origin;
    public Vector3 Origin => _origin.position;

    [SerializeField] private WeaponCatalog _catalog;
    [SerializeField] private Transform _origin;
    [SerializeField] private Transform _socket;

    [SerializeField] private RigBuilder _rigBuilder;
    [SerializeField] private TwoBoneIKConstraint _rightHandIK;
    [SerializeField] private TwoBoneIKConstraint _leftHandIK;

    public bool HasWeapon => _data != null;
    public int RapidFireTick => Mathf.Max(1, Mathf.RoundToInt(_data.RapidFire / Time.fixedDeltaTime));

    private WeaponData _data;
    private WeaponView _view;

    public bool TryFire(in InputPayload payload, ref int nextFireTick)
    {
        if (!HasWeapon)
        {
            return false;
        }

        bool trigger = payload.AimInput && (payload.AttackInput || payload.AttackPressed);
        if (!trigger || payload.Tick < nextFireTick)
        {
            return false;
        }

        nextFireTick = payload.Tick + RapidFireTick;
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="createView"></param>
    public void Equip(int id, bool createView)
    {
        if (_view != null)
        {
            Destroy(_view.gameObject);
            _view = null;
        }

        _data = _catalog.Get(id);

        bool hasView = createView && _data != null && _data.Prefab != null;
        if (hasView)
        {
            _view = Instantiate(_data.Prefab, _socket);
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