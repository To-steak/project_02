using UnityEngine;
using Project02.Character;

namespace Project02.Player
{
    // Authoring: 이동 설정 + 기즈모. 상태는 PlayerSim
    public class PlayerLocomotion : MonoBehaviour
    {
        public SOMovement Settings => _settings;

        [SerializeField] private SOMovement _settings;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_settings == null) return;

            float radius = _settings.Profile.Radius;
            Vector3 position = transform.position;
            LayerMask layer = _settings.Profile.GroundLayer | _settings.Profile.ObstacleLayer;

            CharacterPhysics.GetCapsule(position, radius, _settings.Profile.Height, out Vector3 bottom, out Vector3 top);

            bool overlapped = Physics.CheckCapsule(bottom, top, radius, layer, QueryTriggerInteraction.Ignore);
            Gizmos.color = overlapped ? Color.red : Color.blue;

            Gizmos.DrawWireSphere(bottom, radius);
            Gizmos.DrawWireSphere(top, radius);

            Gizmos.DrawLine(bottom + Vector3.right * radius, top + Vector3.right * radius);
            Gizmos.DrawLine(bottom + Vector3.left * radius, top + Vector3.left * radius);
            Gizmos.DrawLine(bottom + Vector3.forward * radius, top + Vector3.forward * radius);
            Gizmos.DrawLine(bottom + Vector3.back * radius, top + Vector3.back * radius);

            // 올라설 수 있는 최대 턱 높이
            Vector3 step = position + Vector3.up * _settings.Profile.StepHeight;
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(step + Vector3.right * radius, step + Vector3.forward * radius);
            Gizmos.DrawLine(step + Vector3.forward * radius, step + Vector3.left * radius);
            Gizmos.DrawLine(step + Vector3.left * radius, step + Vector3.back * radius);
            Gizmos.DrawLine(step + Vector3.back * radius, step + Vector3.right * radius);
        }
#endif
    }
}