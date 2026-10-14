using Unity.Cinemachine;
using UnityEngine;

public class WeaponView : MonoBehaviour
{
    [SerializeField] private Transform _muzzle;
    [SerializeField] private Transform _rightGrip;
    [SerializeField] private Transform _leftGrip;
    [Tooltip("없으면 흔들림 없음")]
    [SerializeField] private CinemachineImpulseSource _impulse;

    public Vector3 Muzzle => _muzzle.position;
    public Transform RightGrip => _rightGrip;
    public Transform LeftGrip => _leftGrip;

    /// <summary>
    /// 소유자 클라이언트 전용. 임펄스는 같은 채널의 모든 리스너를 흔들기 때문에
    /// 다른 플레이어의 무기에서 호출하면 내 화면도 흔들린다.
    /// </summary>
    public void Shake(float multiplier, float random)
    {
        if (_impulse == null)
        {
            return;
        }

        Vector3 velocity = _impulse.DefaultVelocity + (Vector3)(Random.insideUnitCircle * random);
        _impulse.GenerateImpulseWithVelocity(velocity * multiplier);
    }
}