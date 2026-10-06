using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldSelectionService : MonoBehaviour, IWorldSelectionService
{
    private const string MESSAGE = "RequestWorld";

    private NetworkManager _network;

    private void Awake()
    {
        _network = GetComponent<NetworkManager>();
        _network.OnServerStarted += () => _network.CustomMessagingManager.RegisterNamedMessageHandler(MESSAGE, OnRequest);
        GameServices.Register(this);
    }

    private void OnDestroy()
    {
        GameServices.Unregister(this);
    }

    public void RequestWorld(WorldId world)
    {
        using var writer = new FastBufferWriter(sizeof(int), Allocator.Temp);
        writer.WriteValueSafe((int)world);
        _network.CustomMessagingManager.SendNamedMessage(MESSAGE, NetworkManager.ServerClientId, writer);
    }

    private void OnRequest(ulong sender, FastBufferReader reader)
    {
        reader.ReadValueSafe(out int value);
        if (Scenes.TryGetWorld((WorldId)value, out string scene))
        {
            _network.SceneManager.LoadScene(scene, LoadSceneMode.Single);
        }
    }
}