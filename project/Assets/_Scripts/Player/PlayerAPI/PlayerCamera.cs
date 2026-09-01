using Manager;
using UnityEngine;

namespace PlayerAPI
{
    public class PlayerCamera : MonoBehaviour
    {
        [SerializeField] private Transform _lookPos;
        
        float _pitch;

        public void RotateCamera(float lookY, float speed, float min, float max)
        {
            _pitch = Mathf.Clamp(_pitch - lookY * speed * Time.deltaTime, min, max);
            _lookPos.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        public void GetCamera()
        {
            CameraManager.Instance.SetFollowTarget(_lookPos);
        }

        public void ReleaseCamera()
        {
            CameraManager.Instance.ClearFollowTarget();
        }
    }
}
