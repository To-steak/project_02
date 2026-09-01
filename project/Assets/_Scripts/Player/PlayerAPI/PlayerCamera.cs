using Manager;
using UnityEngine;

namespace PlayerAPI
{
    public class PlayerCamera : MonoBehaviour
    {
        [SerializeField] private Transform _lookPos;
        [SerializeField] private Transform _aimTarget;

        float _aimTargetPitch;
        float _lookPosPitch;
        const float AIM_DISTANCE = 4f;

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

        // 서버 측 pitch 계산 결과를 그냥 float으로 반환만 함 (네트워크 개념 모름)
        public float CalculateServerPitch(float currentPitch, float lookY, float speed, float min, float max)
        {
            _aimTargetPitch = Mathf.Clamp(currentPitch - lookY * speed * Time.fixedDeltaTime, min, max);
            return _aimTargetPitch;
        }

        // pitch 값을 외부(네트워크 계층)에서 받아서 AimTarget 위치만 계산
        public void CalculateAimTarget(float pitch)
        {
            Quaternion rotation = Quaternion.Euler(pitch, 0f, 0f);
            Vector3 direction = transform.rotation * rotation * Vector3.forward;
            _aimTarget.position = transform.position + direction * AIM_DISTANCE;
        }

        public float LocalPitch => _lookPosPitch;
    }
}