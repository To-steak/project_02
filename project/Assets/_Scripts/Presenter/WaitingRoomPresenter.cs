using Project02.Service;
using Project02.View;
using UnityEngine;

namespace Project02.Presenter
{
    public class WaitingRoomPresenter : MonoBehaviour
    {
        [SerializeField] private WaitingRoomView _view;

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