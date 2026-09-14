using Manager;
using UnityEngine;

namespace PlayerAPI
{
    public class PlayerCamera : MonoBehaviour
    {
        public float Pitch { get; private set; }
        public float Yaw { get; private set; }

        [SerializeField] private Transform _lookPos;
        [SerializeField] private Transform _aimTarget;

        const float AIM_TARGET_DISTANCE = 2f;

        public void ActiveCamera()
        {
            CameraManager.Instance.SetFollowTarget(_lookPos);
        }

        public void RotatePitch(float lookY, float speed, float min, float max)
        {
            Pitch = Mathf.Clamp(Pitch - lookY * speed, min, max);
            _lookPos.localRotation = Quaternion.Euler(Pitch, 0f, 0f);
        }

        public void RotateYaw(float lookX, float speed)
        {
            Yaw = Mathf.Repeat(Yaw + lookX * speed, 360f);            
        }

        public void ApplyPitch(float pitch)
        {
            Quaternion rotation = Quaternion.Euler(pitch, 0f, 0f);
            Vector3 direction = transform.rotation * rotation * Vector3.forward;
            _aimTarget.position = _lookPos.position + direction * AIM_TARGET_DISTANCE;
        }
    }
}