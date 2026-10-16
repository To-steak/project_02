using Project02.Player;
using Project02.Weapon;
using UnityEngine;

namespace Project02.Client
{
    public static class CameraSystem
    {
        public static void Update(ClientWorld world, float sensitivity, float frameTime)
        {
            PlayerController controller = world.Local.Controller;
            SOPlayer settings = controller.PlayerSettings;
            ref CameraState camera = ref world.Camera;
            ref InputState input = ref world.Input;

            // 1. 반동 흡수 (프레임 독립 지수 감쇠)
            float t = 1.0f - Mathf.Exp(-settings.RecoilSpeed * frameTime);
            Vector2 kick = camera.Recoil * t;
            camera.Recoil -= kick;

            // 2. 시점
            float yaw = input.Look.x * settings.RotationSpeed * sensitivity + kick.x;
            float pitch = input.Look.y * settings.PitchSpeed * sensitivity + kick.y;
            camera.Yaw = Mathf.Repeat(camera.Yaw + yaw, 360.0f);
            camera.Pitch = Mathf.Clamp(camera.Pitch - pitch, settings.MinPitch, settings.MaxPitch);

            // 3. 정조준 블렌드
            camera.AdsBlend = Mathf.MoveTowards(camera.AdsBlend, input.Aim ? 1.0f : 0.0f, settings.AdsSpeed * frameTime);

            // 4. 프록시 출력 → 조준점
            SOWeapon weapon = world.Local.Simulation.Weapon;
            float zoom = weapon != null ? weapon.AdsZoom : 1.0f;
            controller.PlayerCamera.Apply(camera.AdsBlend, zoom);
            camera.AimPoint = controller.PlayerCamera.GetAimPoint();
        }
    }
}