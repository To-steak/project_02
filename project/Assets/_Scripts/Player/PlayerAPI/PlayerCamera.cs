using GameInterface;
using UnityEngine;

namespace PlayerAPI
{
    public class PlayerCamera : MonoBehaviour
    {
        public float Pitch { get; private set; }
        public float Yaw { get; private set; }

        [SerializeField] private Transform _lookPos;
        [SerializeField] private Transform _aimTarget;

        private const float AIM_TARGET_DISTANCE = 2f;

        private bool _aim;
        private ICameraService _service;

        public void Initialize(ICameraService service)
        {
            _service = service;
            _service.SetTarget(_lookPos);
        }

        public void Release()
        {
            _service?.ReleaseTarget();
            _service = null;
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

        public void ApplyAimTarget(float pitch)
        {
            Quaternion rotation = Quaternion.Euler(pitch, 0f, 0f);
            Vector3 direction = transform.rotation * rotation * Vector3.forward;

            _aimTarget.position = _lookPos.position + direction * AIM_TARGET_DISTANCE;
        }

        public void ApplyAim(bool aim)
        {
            if (aim != _aim)
            {
                _aim = aim;
                if (aim)
                {
                    _service.SetAim();
                }
                else
                {
                    _service.ReleaseAim();
                }
            }

            if (aim)
            {
                _aimTarget.position = _service.GetAimPoint();
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
}