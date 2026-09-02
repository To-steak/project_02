using UnityEngine;

public class WeaponHandSync : MonoBehaviour
{
    [SerializeField] private Transform _leftHandIkTarget;
    [SerializeField] private Transform _rightHandIkTarget;
    [SerializeField] private WeaponSocket _currentWeapon;

    void LateUpdate()
    {
        if (_currentWeapon == null) return;

        _leftHandIkTarget.position = _currentWeapon.LeftHandSocket.position;
        _leftHandIkTarget.rotation = _currentWeapon.LeftHandSocket.rotation;

        _rightHandIkTarget.position = _currentWeapon.RightHandSocket.position;
        _rightHandIkTarget.rotation = _currentWeapon.RightHandSocket.rotation;
    }
}