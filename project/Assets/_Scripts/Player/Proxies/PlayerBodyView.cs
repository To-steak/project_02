using UnityEngine;
using Project02.Character;

namespace Project02.Player
{
    // 프록시: 몸체 위치 보간, 회전, 조준 IK 목표. 에디터에서는 충돌 캡슐도 그린다
    public class PlayerBodyView : MonoBehaviour
    {
        [SerializeField] private SOPlayer _settings;   // TurnSpeed, Movement.Profile(기즈모)
        [SerializeField] private Transform _presentation;
        [SerializeField] private Transform _model;

        [SerializeField] private Transform _lookPos;
        [SerializeField] private Transform _aimTarget;

        private Vector3 _previous;
        private Vector3 _current;

        private const float SNAP_DISTANCE = 1f;
        private const float MIN_FACE_SQR = 0.01f;
        private const float AIM_TARGET_DISTANCE = 2f;

        public void Initialize(Vector3 position)
        {
            _previous = _current = position;
            _presentation.rotation = transform.rotation;
        }
        
        // PredictSystem이 틱마다 (Step 직후)
        public void Record(Vector3 position)
        {
            bool snap = Vector3.Distance(_current, position) > SNAP_DISTANCE;
            _previous = snap ? position : _current;
            _current = position;
        }

        // LateUpdate
        public void Interpolate(float alpha)
        {
            _presentation.position = Vector3.Lerp(_previous, _current, alpha);
        }

        public void Face(Vector3 aimPoint, Vector3 muzzle, float deltaTime)
        {
            Vector3 direction = aimPoint - muzzle;
            direction.y = 0.0f;
            if (direction.sqrMagnitude < MIN_FACE_SQR)
            {
                return;
            }

            Quaternion look = Quaternion.LookRotation(direction);
            _model.rotation = Quaternion.RotateTowards(_model.rotation, look, _settings.TurnSpeed * deltaTime);
        }

        /// <summary>상체(와 소유자 카메라)의 상하 각도.</summary>
        public void ApplyPitch(float pitch)
        {
            _lookPos.localRotation = Quaternion.Euler(pitch, 0.0f, 0.0f);
        }

        public void ApplyYaw(float yaw)
        {
            _presentation.rotation = Quaternion.Euler(0.0f, yaw, 0.0f);
        }

        /// <summary>소유자. 상체가 조준점을 보게 한다.</summary>
        public void AimAt(Vector3 point)
        {
            _aimTarget.position = point;
        }

        /// <summary>다른 플레이어. 조준점이 없으니 pitch로 앞쪽 지점을 만든다.</summary>
        public void AimAtPitch(float pitch)
        {
            Vector3 direction = transform.rotation * Quaternion.Euler(pitch, 0.0f, 0.0f) * Vector3.forward;
            _aimTarget.position = _lookPos.position + direction * AIM_TARGET_DISTANCE;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_lookPos == null || _aimTarget == null) return;

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(_lookPos.position, _aimTarget.position);
            Gizmos.DrawWireSphere(_aimTarget.position, 0.05f);
        }

        // 충돌 캡슐(겹치면 빨강)과 올라설 수 있는 최대 턱 높이(마젠타)
        private void OnDrawGizmosSelected()
        {
            if (_settings == null || _settings.Movement == null) return;

            CharacterProfile profile = _settings.Movement.Profile;
            float radius = profile.Radius;
            Vector3 position = transform.position;
            LayerMask layer = profile.GroundLayer | profile.ObstacleLayer;

            CharacterPhysics.GetCapsule(position, radius, profile.Height, out Vector3 bottom, out Vector3 top);

            bool overlapped = Physics.CheckCapsule(bottom, top, radius, layer, QueryTriggerInteraction.Ignore);
            Gizmos.color = overlapped ? Color.red : Color.blue;

            Gizmos.DrawWireSphere(bottom, radius);
            Gizmos.DrawWireSphere(top, radius);

            Gizmos.DrawLine(bottom + Vector3.right * radius, top + Vector3.right * radius);
            Gizmos.DrawLine(bottom + Vector3.left * radius, top + Vector3.left * radius);
            Gizmos.DrawLine(bottom + Vector3.forward * radius, top + Vector3.forward * radius);
            Gizmos.DrawLine(bottom + Vector3.back * radius, top + Vector3.back * radius);

            Vector3 step = position + Vector3.up * profile.StepHeight;
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(step + Vector3.right * radius, step + Vector3.forward * radius);
            Gizmos.DrawLine(step + Vector3.forward * radius, step + Vector3.left * radius);
            Gizmos.DrawLine(step + Vector3.left * radius, step + Vector3.back * radius);
            Gizmos.DrawLine(step + Vector3.back * radius, step + Vector3.right * radius);
        }
#endif
    }
}