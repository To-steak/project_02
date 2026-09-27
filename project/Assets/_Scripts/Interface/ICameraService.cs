using UnityEngine;

    public interface ICameraService
    {
        void SetTarget(Transform target);
        void SetAim();
        void ReleaseAim();
        void ReleaseTarget();
        Vector3 GetAimPoint();
    }