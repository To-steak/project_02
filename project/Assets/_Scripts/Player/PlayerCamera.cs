using Unity.Cinemachine;
using UnityEngine;

namespace Project02.Player
{
    using Unity.Cinemachine;
    using UnityEngine;

    /// <summary>소유자 전용. 시점 회전, 반동, 조준 줌, 조준점.</summary>
    public class PlayerCamera : MonoBehaviour
    {
        public float Pitch { get; private set; }
        public float Yaw { get; private set; }

        /// <summary>이번 프레임 ApplyAim에서 구한 조준점.</summary>
        public Vector3 AimPoint { get; private set; }

        [Tooltip("프리팹에서 비활성으로 둘 것. Tracking Target은 PlayerVisual의 LookPos")]
        [SerializeField] private CinemachineCamera _cmCamera;

        private SOPlayer _settings;
        private CinemachineThirdPersonFollow _follow;
        private Camera _camera;

        // 조준하지 않을 때의 값 (프리팹의 시네머신 설정)
        private float _fov;
        private float _cameraDistance;
        private float _verticalArmLength;
        private Vector3 _shoulderOffset;

        private float _adsBlend;
        private Vector2 _recoil;   // 아직 흡수되지 않은 반동 (x: yaw, y: 위쪽 pitch, 도)

        public void Initialize(SOPlayer settings)
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

        /// <summary>매 프레임. 마우스 입력으로 시점을 돌리고 남은 반동을 조금씩 흡수한다.</summary>
        public void Look(Vector2 lookInput, float sensitivity, float deltaTime)
        {
            float t = 1.0f - Mathf.Exp(-_settings.RecoilSpeed * deltaTime);
            Vector2 kick = _recoil * t;
            _recoil -= kick;

            float yaw = lookInput.x * _settings.RotationSpeed * sensitivity + kick.x;
            float pitch = lookInput.y * _settings.PitchSpeed * sensitivity + kick.y;

            Yaw = Mathf.Repeat(Yaw + yaw, 360f);
            Pitch = Mathf.Clamp(Pitch - pitch, _settings.MinPitch, _settings.MaxPitch);
        }

        /// <summary>발사 반동을 쌓는다. Look이 프레임마다 흡수한다.</summary>
        public void AddRecoil(Vector2 kick)
        {
            _recoil += kick;
        }

        /// <summary>매 프레임. 조준 상태로 FOV·거리·어깨 위치를 부드럽게 전환하고 조준점을 구한다.</summary>
        public void ApplyAim(bool aim, float zoom, float deltaTime)
        {
            _adsBlend = Mathf.MoveTowards(_adsBlend, aim ? 1.0f : 0.0f, _settings.AdsSpeed * deltaTime);

            _cmCamera.Lens.FieldOfView = Mathf.Lerp(_fov, _fov / Mathf.Max(1.0f, zoom), _adsBlend);
            _follow.CameraDistance = Mathf.Lerp(_cameraDistance, _settings.AdsCameraDistance, _adsBlend);
            _follow.VerticalArmLength = Mathf.Lerp(_verticalArmLength, _settings.AdsVerticalArmLength, _adsBlend);
            _follow.ShoulderOffset = Vector3.Lerp(_shoulderOffset, _settings.AdsShoulderOffset, _adsBlend);

            AimPoint = GetAimPoint();
        }

        /// <summary>카메라 정면이 가리키는 지점.</summary>
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
            if (_camera == null) return;   // 소유자가 아니면 조준점이 없다

            Gizmos.color = Color.red;
            Gizmos.DrawLine(_camera.transform.position, AimPoint);
        }
#endif
    }
}