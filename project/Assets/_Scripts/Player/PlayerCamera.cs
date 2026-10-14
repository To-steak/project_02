using Unity.Cinemachine;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float Pitch { get; private set; }
    public float Yaw { get; private set; }
    public Vector3 AimPoint { get; private set; }

    [SerializeField] private Transform _lookPos;
    [SerializeField] private Transform _aimTarget;
    [SerializeField] private CinemachineCamera _cmCamera;

    private const float AIM_TARGET_DISTANCE = 2f;

    private PlayerSettings _settings;
    private CinemachineThirdPersonFollow _follow;
    private Camera _camera;
    private float _fov;
    private float _cameraDistance;
    private float _verticalArmLength;
    private Vector3 _shoulderOffset;
    private float _adsBlend;
    private Vector2 _recoil;

    public void Initialize(PlayerSettings settings)
    {
        _settings = settings;
        _follow = _cmCamera.GetComponent<CinemachineThirdPersonFollow>();
        _camera = Camera.main;

        _fov = _cmCamera.Lens.FieldOfView;
        _cameraDistance = _follow.CameraDistance;
        _verticalArmLength = _follow.VerticalArmLength;
        _shoulderOffset = _follow.ShoulderOffset;

        _cmCamera.gameObject.SetActive(true);
    }

    public void Release()
    {
        _cmCamera.gameObject.SetActive(false);
    }

    public void Look(Vector2 lookInput, float sensitivity, float deltaTime)
    {
        float t = 1.0f - Mathf.Exp(-_settings.RecoilSpeed * deltaTime);
        Vector2 kick = _recoil * t;
        _recoil -= kick;

        float yaw = lookInput.x * _settings.RotationSpeed * sensitivity + kick.x;
        float pitch = lookInput.y * _settings.PitchSpeed * sensitivity + kick.y;

        Yaw = Mathf.Repeat(Yaw + yaw, 360f);
        ApplyPitch(Mathf.Clamp(Pitch - pitch, _settings.MinPitch, _settings.MaxPitch));
    }

    public void AddRecoil(Vector2 kick)
    {
        _recoil += kick;
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

    public void ApplyAim(bool aim, float zoom, float deltaTime)
    {
        _adsBlend = Mathf.MoveTowards(_adsBlend, aim ? 1.0f : 0.0f, _settings.AdsSpeed * deltaTime);

        _cmCamera.Lens.FieldOfView = Mathf.Lerp(_fov, _fov / Mathf.Max(1.0f, zoom), _adsBlend);
        _follow.CameraDistance = Mathf.Lerp(_cameraDistance, _settings.AdsCameraDistance, _adsBlend);
        _follow.VerticalArmLength = Mathf.Lerp(_verticalArmLength, _settings.AdsVerticalArmLength, _adsBlend);
        _follow.ShoulderOffset = Vector3.Lerp(_shoulderOffset, _settings.AdsShoulderOffset, _adsBlend);

        AimPoint = GetAimPoint();
        _aimTarget.position = AimPoint;
    }

    public Vector3 GetAimPoint()
    {
        Transform view = _camera.transform;
        if (Physics.Raycast(view.position, view.forward, out RaycastHit hit, _settings.AimDistance, _settings.AimLayer, QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }

        return view.position + view.forward * _settings.AimDistance;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_lookPos == null || _aimTarget == null) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(_lookPos.position, _aimTarget.position);
        Gizmos.DrawWireSphere(_aimTarget.position, 0.05f);

        if (_camera == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(_camera.transform.position, AimPoint);
    }
#endif
}