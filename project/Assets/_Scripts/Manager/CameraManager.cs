using Unity.Cinemachine;
using UnityEngine;

namespace Manager
{
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance { get; private set; }
        [SerializeField] private CinemachineCamera _cinemachineCamera;

        void Awake()
        {
            Instance = this;
        }

        public void SetFollowTarget(Transform transform)
        {
            _cinemachineCamera.Follow = transform;
        }

        public void SetLookAt(Transform transform)
        {
            _cinemachineCamera.LookAt = transform;
        }
        public void ClearFollowTarget()
        {
            _cinemachineCamera.Follow = null;
        }
    }
}