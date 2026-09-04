using UnityEngine;

namespace PlayerAPI
{
    public class PlayerVisual : MonoBehaviour
    {
        [SerializeField] private Transform _visual;

        Vector3 _prevPos, _currPos;
        Quaternion _prevRot, _currRot;
        bool _active;
        const float SNAP_DISTANCE = 2f;

        public void ActiveVisual()
        {
            if (_visual == null) return;

            _visual.SetParent(null, true);

            _prevPos = _currPos = transform.position;
            _prevRot = _currRot = transform.rotation;

            _active = true;
        }

        public void InactiveVisual()
        {
            if (!_active) return;

            _active = false;
            if (_visual != null) _visual.SetParent(transform, true);
        }

        // FixedUpdate - Simulate() 직후
        public void Record()
        {
            if (!_active) return;

            _prevPos = _currPos;
            _prevRot = _currRot;

            _currPos = transform.position;
            _currRot = transform.rotation;

            if (Vector3.Distance(_prevPos, _currPos) > SNAP_DISTANCE)
            {
                _prevPos = _currPos;
                _prevRot = _currRot;
            }
        }

        // LateUpdate
        public void Interpolate()
        {
            if (!_active) return;

            float alpha = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);

            _visual.SetPositionAndRotation(
                Vector3.Lerp(_prevPos, _currPos, alpha),
                Quaternion.Slerp(_prevRot, _currRot, alpha));
        }

        void OnDestroy()
        {
            if (_active && _visual != null) Destroy(_visual.gameObject);
        }
    }
}