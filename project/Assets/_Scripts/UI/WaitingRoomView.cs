using UnityEngine;
using UnityEngine.UI;

public class WaitingRoomView : MonoBehaviour
{
    [SerializeField] private Button _tutorialWorld;
    [SerializeField] private Button _testWorld;

    private void Awake()
    {
        _tutorialWorld.onClick.AddListener(() => Request(WorldId.Tutorial));
        _testWorld.onClick.AddListener(() => Request(WorldId.Test));
    }

    private void Request(WorldId world)
    {
        var service = GameServices.WorldSelection;
        if (service == null)
        {
            Debug.LogWarning($"[{nameof(WaitingRoomView)}] {nameof(IWorldSelectionService)}가 아직 등록되지 않았습니다.", this);
            return;
        }

        service.RequestWorld(world);
    }
}