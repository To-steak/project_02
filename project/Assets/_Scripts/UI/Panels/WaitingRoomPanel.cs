using System;
using UnityEngine;
using UnityEngine.UI;

namespace Project02.UI
{
    public class WaitingRoomPanel : MonoBehaviour
    {
        public event Action TutorialClicked;
        public event Action TestClicked;

        [SerializeField] private Button _tutorialWorld;
        [SerializeField] private Button _testWorld;

        private void Awake()
        {
            _tutorialWorld.onClick.AddListener(() => TutorialClicked?.Invoke());
            _testWorld.onClick.AddListener(() => TestClicked?.Invoke());
        }
    }
}