using UnityEngine;
using UnityEngine.UI;

public class WaitingRoomView : MonoBehaviour
{
    [SerializeField] private Button _tutorialWorld;
    [SerializeField] private Button _testWorld;

    private WaitingRoomPresenter _presenter;

    private void Awake()
    {
        _presenter = new WaitingRoomPresenter();

        _tutorialWorld.onClick.AddListener(() => _presenter.RequestWorld(WorldId.Tutorial));
        _testWorld.onClick.AddListener(() => _presenter.RequestWorld(WorldId.Test));
    }
}