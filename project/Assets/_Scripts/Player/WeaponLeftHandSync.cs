using UnityEngine;

public class WeaponLeftHandSync : MonoBehaviour
{
    [SerializeField] private Transform _leftHandIkTarget; // LeftHandTarget 오브젝트
    [SerializeField] private Transform _weaponLeftHandGrip; // 총의 LeftHandGrip 오브젝트

    void LateUpdate()
    {
        _leftHandIkTarget.position = _weaponLeftHandGrip.position;
        _leftHandIkTarget.rotation = _weaponLeftHandGrip.rotation;
    }
}