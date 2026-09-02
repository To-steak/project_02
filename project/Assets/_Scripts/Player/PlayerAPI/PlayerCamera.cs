using Manager;
using UnityEngine;

namespace PlayerAPI
{
    public class PlayerCamera : MonoBehaviour
    {
        [SerializeField] private Transform _lookPos;
        [SerializeField] private Transform _aimTarget;

        const float MAX_AIM_DISTANCE = 4f;
        const float MIN_AIM_DISTANCE = 2f;

        float _aimTargetPitch;
        float _lookPosPitch;

        public void RotateCamera(float lookY, float speed, float min, float max)
        {
            _lookPosPitch = Mathf.Clamp(_lookPosPitch - lookY * speed * Time.deltaTime, min, max);
            _lookPos.localRotation = Quaternion.Euler(_lookPosPitch, 0f, 0f);
        }

        public void GetCamera()
        {
            CameraManager.Instance.SetFollowTarget(_lookPos);
        }

        public void ReleaseCamera()
        {
            CameraManager.Instance.ClearFollowTarget();
        }

        public float ClampPitch(float currentPitch, float lookY, float speed, float min, float max)
        {
            _aimTargetPitch = Mathf.Clamp(currentPitch - lookY * speed * Time.fixedDeltaTime, min, max);
            return _aimTargetPitch;
        }

        public Vector3 RaycastAimPoint(Camera camera, LayerMask layer)
        {
            Ray ray = new Ray(camera.transform.position, camera.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, MAX_AIM_DISTANCE, layer))
            {
                float distance = Mathf.Max(hit.distance, MIN_AIM_DISTANCE);
                return camera.transform.position + camera.transform.forward * distance;
            }

            return camera.transform.position + camera.transform.forward * MAX_AIM_DISTANCE;
        }

        public Vector3 SetAimTargetFromCamera(Camera camera, LayerMask layer)
        {
            Vector3 hitPoint = RaycastAimPoint(camera, layer);
            _aimTarget.position = hitPoint;
            return hitPoint;
        }

        public void SetAimTargetFromPitch(float pitch)
        {
            Quaternion rotation = Quaternion.Euler(pitch, 0f, 0f);
            Vector3 direction = transform.rotation * rotation * Vector3.forward;
            _aimTarget.position = transform.position + direction * MIN_AIM_DISTANCE;
        }
    }
}