using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    public WeaponData Data => _data;
    public LayerMask EnemyLayer => _enemyLayer;
    public Vector3 Origin => _origin.position;
    public Vector3 Muzzle => _muzzle.position;
    public int RapidFireTick => Mathf.Max(1, Mathf.RoundToInt(_data.RapidFire / Time.fixedDeltaTime));

    [SerializeField] private WeaponData _data;
    [SerializeField] private Transform _origin;
    [SerializeField] private Transform _muzzle;
    [SerializeField] private LayerMask _enemyLayer;

    public bool TryFire(in InputPayload payload, ref int nextFireTick)
    {
        bool trigger = payload.AimInput && (payload.AttackInput || payload.AttackPressed);
        if (!trigger || payload.Tick < nextFireTick)
        {
            return false;
        }

        nextFireTick = payload.Tick + RapidFireTick;
        return true;
    }
}