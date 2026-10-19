using UnityEngine;
using Project02.Service;

namespace Project02.UI
{
    public class WaitingRoomPresenter : MonoBehaviour
    {
        [SerializeField] private WaitingRoomPanel _view;

        private void OnEnable()
        {
            _view.TutorialClicked += OnTutorial;
            _view.TestClicked += OnTest;
        }

        private void OnDisable()
        {
            _view.TutorialClicked -= OnTutorial;
            _view.TestClicked -= OnTest;
        }

        private void OnTutorial()
        {
            WorldSelectionService.Instance.RequestWorld(WorldId.Tutorial);
        }

        private void OnTest()
        {
            WorldSelectionService.Instance.RequestWorld(WorldId.Test);
        }
    }
}