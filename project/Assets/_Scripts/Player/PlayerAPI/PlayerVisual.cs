using UnityEngine;

namespace PlayerAPI
{
    public class PlayerVisual : MonoBehaviour
    {
        [SerializeField] private Transform _visual;
        [SerializeField] private float _smoothing = 10f;

        Vector3 _prevPos, _currPos, _offset;
        const float SNAP_DISTANCE = 1f;

        public void ActiveVisual()
        {
            if (_visual == null) return;

            _prevPos = _currPos = transform.position;
            _offset = Vector3.zero;
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
            _offset = Vector3.Lerp(_offset, Vector3.zero, 1f - Mathf.Exp(-_smoothing * Time.deltaTime));

            float alpha = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
            _visual.position = Vector3.Lerp(_prevPos, _currPos, alpha) + _offset;
        }

        public Vector3 CaptureVisualPosition() => _visual.position;

        public void AbsorbCorrection(Vector3 beforePosition)
        {
            _prevPos = _currPos = transform.position;
            _offset = beforePosition - transform.position;

            if (_offset.sqrMagnitude > SNAP_DISTANCE * SNAP_DISTANCE)
            {
                _offset = Vector3.zero;
            }
        }
    }
}