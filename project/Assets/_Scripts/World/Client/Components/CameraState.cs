using UnityEngine;

namespace Project02.World
{
    public struct CameraState
    {
        public float Yaw;
        public float Pitch;
        public Vector2 Recoil;       // 아직 흡수되지 않은 반동 (x: yaw, y: 위쪽 pitch, 도)
        public float AdsBlend;       // 0 = 지향사격, 1 = 정조준
        public Vector3 AimPoint;     // 카메라 정면이 가리키는 지점
    }
}