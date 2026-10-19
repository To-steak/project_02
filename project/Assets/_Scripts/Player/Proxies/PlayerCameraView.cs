using Unity.Cinemachine;
using UnityEngine;

namespace Project02.Player
{
    // 프록시: CameraState → Cinemachine, 조준 레이캐스트. 상태는 ClientWorld.Camera
    public class PlayerCameraView : MonoBehaviour
    {
        [Tooltip("프리팹에서 비활성으로 둘 것. Tracking Target은 PlayerVisual의 LookPos")]
        [SerializeField] private CinemachineCamera _cmCamera;

        private SOPlayer _settings;
        private CinemachineThirdPersonFollow _follow;
        private Camera _camera;
        private float _fov;
        private float _cameraDistance;
        private float _verticalArmLength;
        private Vector3 _shoulderOffset;
        private Vector3 _aimPoint;   // 기즈모용

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

        public void Apply(float adsBlend, float zoom)
        {
            _cmCamera.Lens.FieldOfView = Mathf.Lerp(_fov, _fov / Mathf.Max(1.0f, zoom), adsBlend);
            _follow.CameraDistance = Mathf.Lerp(_cameraDistance, _settings.AdsCameraDistance, adsBlend);
            _follow.VerticalArmLength = Mathf.Lerp(_verticalArmLength, _settings.AdsVerticalArmLength, adsBlend);
            _follow.ShoulderOffset = Vector3.Lerp(_shoulderOffset, _settings.AdsShoulderOffset, adsBlend);
        }

        public Vector3 GetAimPoint()
        {
            Transform view = _camera.transform;
            _aimPoint = Physics.Raycast(view.position, view.forward, out RaycastHit hit, _settings.AimDistance, _settings.AimLayer, QueryTriggerInteraction.Ignore) ? hit.point : view.position + view.forward * _settings.AimDistance;
            return _aimPoint;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_camera == null) return;   // 소유자가 아니면 조준점이 없다

            Gizmos.color = Color.red;
            Gizmos.DrawLine(_camera.transform.position, _aimPoint);
        }
#endif
    }
}