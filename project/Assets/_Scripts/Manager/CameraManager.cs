using Unity.Cinemachine;
using UnityEngine;
using GameInterface;

namespace GameManager
{
    public class CameraManager : MonoBehaviour, ICameraService
    {
        [SerializeField] private LayerMask layer;
        [SerializeField] private CinemachineCamera cmCamera;

        CinemachineThirdPersonFollow _cmtpf;
        Camera _camera;
        const float MAX_RANGE = 200f;
        const float MIN_RANGE = 3f;

        private void Awake()
        {
            GameServices.Register(this);

            cmCamera.Lens.NearClipPlane = 0.3f;
            cmCamera.Lens.FarClipPlane = 50000f;

            _cmtpf = cmCamera.GetComponent<CinemachineThirdPersonFollow>();
            _camera = Camera.main;
        }

        private void OnDestroy()
        {
            GameServices.Unregister(this);
        }

        public void SetTarget(Transform transform)
        {
            cmCamera.Follow = transform;
        }

        public void ReleaseTarget()
        {
            cmCamera.Follow = null;
        }

        public void SetAim()
        {
            _cmtpf.VerticalArmLength = 0f;
            _cmtpf.CameraDistance = 1.25f;
        }

        public void ReleaseAim()
        {
            _cmtpf.VerticalArmLength = 0.5f;
            _cmtpf.CameraDistance = 2.5f;
        }

        public Vector3 GetAimPoint()
        {
            Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (Physics.Raycast(ray, out RaycastHit hit, MAX_RANGE, layer) && hit.distance >= MIN_RANGE)
            {
                return hit.point;
            }

            return ray.origin + ray.direction * MAX_RANGE;
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (_camera == null) return;
            Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Gizmos.color = Color.red;
            Gizmos.DrawRay(ray.origin, ray.direction * MAX_RANGE);
        }
#endif
    }
}