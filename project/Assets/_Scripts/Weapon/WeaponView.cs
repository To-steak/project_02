using UnityEngine;

public class WeaponView : MonoBehaviour
{
    [SerializeField] private Transform _muzzle;
    [SerializeField] private Transform _rightGrip;
    [SerializeField] private Transform _leftGrip;

    public Vector3 Muzzle => _muzzle.position;
    public Transform RightGrip => _rightGrip;
    public Transform LeftGrip => _leftGrip;
}