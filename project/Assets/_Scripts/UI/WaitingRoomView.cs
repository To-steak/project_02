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

    private static void Request(WorldId world) => GameServices.WorldSelection?.RequestWorld(world);
}