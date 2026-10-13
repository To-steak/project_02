using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float Pitch { get; private set; }
    public float Yaw { get; private set; }

    [SerializeField] private Transform _lookPos;
    [SerializeField] private Transform _aimTarget;

    private const float AIM_TARGET_DISTANCE = 2f;

    private bool _aim;

    /// <summary>소유자 전용. 전역 카메라가 이 플레이어를 따라가게 한다.</summary>
    public void Initialize()
    {
        CameraService.Instance.SetTarget(_lookPos);
    }

    /// <summary>소유자 전용. 디스폰 시 카메라 추적을 해제한다.</summary>
    public void Release()
    {
        // 씬 종료나 앱 종료 중에는 CameraService가 먼저 파괴될 수 있다.
        CameraService.Instance?.ReleaseTarget();
    }

    public void RotatePitch(float lookY, float speed, float min, float max)
    {
        ApplyPitch(Mathf.Clamp(Pitch - lookY * speed, min, max));
    }

    public void RotateYaw(float lookX, float speed)
    {
        Yaw = Mathf.Repeat(Yaw + lookX * speed, 360f);
    }

    public void ApplyAimTarget(float pitch)
    {
        Quaternion rotation = Quaternion.Euler(pitch, 0.0f, 0.0f);
        Vector3 direction = transform.rotation * rotation * Vector3.forward;

        _aimTarget.position = _lookPos.position + direction * AIM_TARGET_DISTANCE;
    }

    public void ApplyPitch(float pitch)
    {
        Pitch = pitch;
        _lookPos.localRotation = Quaternion.Euler(pitch, 0.0f, 0.0f);
    }

    /// <summary>소유자 전용, 매 프레임.</summary>
    public void ApplyAim(bool aim)
    {
        if (aim != _aim)
        {
            _aim = aim;
            if (aim)
            {
                CameraService.Instance.SetAim();
            }
            else
            {
                CameraService.Instance.ReleaseAim();
            }
        }

        if (aim)
        {
            _aimTarget.position = CameraService.Instance.GetAimPoint();
        }
        else
        {
            ApplyAimTarget(Pitch);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_lookPos == null || _aimTarget == null) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(_lookPos.position, _aimTarget.position);
        Gizmos.DrawWireSphere(_aimTarget.position, 0.05f);
    }
#endif
}