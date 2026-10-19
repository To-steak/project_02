using UnityEngine;

namespace Project02.Player
{
    public class PlayerBodyView : MonoBehaviour
    {
        [SerializeField] private Transform _presentation;
        [SerializeField] private Transform _model;

        [SerializeField] private Transform _lookPos;
        [SerializeField] private Transform _aimTarget;

        private Vector3 _previous;
        private Vector3 _current;

        private const float SNAP_DISTANCE = 1f;
        private const float MIN_FACE_SQR = 0.01f;
        private const float AIM_TARGET_DISTANCE = 2f;

        private SOPlayer _settings;

        public void Initialize(Vector3 position, SOPlayer settings)
        {
            _settings = settings;
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
#endif
    }
}