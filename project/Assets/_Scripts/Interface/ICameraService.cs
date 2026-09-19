using UnityEngine;

namespace GameInterface
{
    public interface ICameraService
    {
        void SetTarget(Transform target);
        void SetAim();
        void ReleaseAim();
        void ReleaseTarget();
        Vector3 GetAimPoint();
    }
}