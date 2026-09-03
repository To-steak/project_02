using Manager;
using UnityEngine;

namespace PlayerAPI
{
    public class PlayerCamera : MonoBehaviour
    {
        public float LookPitch { get; private set; }

        [SerializeField] private Transform _lookPos;
        [SerializeField] private Transform _aimTarget;

        const float AIM_TARGET_DISTANCE = 2f;

        public void ActiveCamera()
        {
            CameraManager.Instance.SetFollowTarget(_lookPos);
            // CameraManager.Instance.SetFollowTarget(transform);
        }

        public void RotateCamera(float lookY, float speed, float min, float max)
        {
            LookPitch = Mathf.Clamp(LookPitch - lookY * speed, min, max);
            _lookPos.localRotation = Quaternion.Euler(LookPitch, 0f, 0f);
        }

        public void SetAimTargetFromPitch(float pitch)
        {
            Quaternion rotation = Quaternion.Euler(pitch, 0f, 0f);
            Vector3 direction = transform.rotation * rotation * Vector3.forward;
            _aimTarget.position = _lookPos.position + direction * AIM_TARGET_DISTANCE;
        }
    }
}