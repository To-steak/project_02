using UnityEngine;

namespace PlayerAPI
{
    public class PlayerVisual : MonoBehaviour
    {
        [SerializeField] private Transform _visual;

        Vector3 _prevPos, _currPos;
        const float SNAP_DISTANCE = 1f;

        public void ActiveVisual()
        {
            if (_visual == null) return;

            _prevPos = _currPos = transform.position;
        }

        public void InactiveVisual()
        {
            if (_visual != null)
            {
                _visual.localPosition = Vector3.zero;
                _visual.localRotation = Quaternion.identity;
            }
        }

        // FixedUpdate - Simulate() 직후
        public void Record()
        {
            _prevPos = _currPos;

            _currPos = transform.position;

            if (Vector3.Distance(_prevPos, _currPos) > SNAP_DISTANCE)
            {
                _prevPos = _currPos;
            }
        }

        // LateUpdate
        public void Interpolate()
        {
            float alpha = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
            _visual.position = Vector3.Lerp(_prevPos, _currPos, alpha);
        }
    }
}