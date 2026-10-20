using UnityEngine;

namespace Project02.Service
{
    public class InputService : MonoBehaviour
    {
        public static InputService Instance { get; private set; }

        public PlayerAction Actions { get; private set; }
        public float MouseSensitivity { get; set; } = 1.0f;

        private void Awake()
        {
            Actions = new PlayerAction();
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            Actions.Disable();
            Actions.Dispose();
        }
    }
}