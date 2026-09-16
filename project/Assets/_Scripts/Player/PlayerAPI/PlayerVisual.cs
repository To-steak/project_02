using UnityEngine;

namespace PlayerAPI
{
    public class PlayerVisual : MonoBehaviour
    {
        [SerializeField] private Transform _visual;

        Vector3 _previous;
        Vector3 _current;
        const float SNAP_DISTANCE = 1f;

        public void ActiveVisual()
        {
            if (_visual == null) return;

            _previous = _current = transform.position;
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
            _previous = _current;

            _current = transform.position;

            if (Vector3.Distance(_previous, _current) > SNAP_DISTANCE)
            {
                _previous = _current;
            }
        }

        // LateUpdate
        public void Interpolate()
        {
            float alpha = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
            _visual.position = Vector3.Lerp(_previous, _current, alpha);
        }
    }
}