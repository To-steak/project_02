using UnityEngine;

namespace PlayerAPI
{
    public class PlayerVisual : MonoBehaviour
    {
        [SerializeField] private Transform _visual;

        private Vector3 _previous;
        private Vector3 _current;

        private const float SNAP_DISTANCE = 1f;

        public void Initialzie(Vector3 position)
        {
            _previous = _current = position;
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