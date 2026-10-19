using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project02.Service
{
    public class WorldSelectionService : MonoBehaviour
    {
        public static WorldSelectionService Instance { get; private set; }

        private const string MESSAGE = "RequestWorld";

        private NetworkManager _network;

        private void Awake()
        {
            _network = GetComponent<NetworkManager>();
            _network.OnServerStarted += () => _network.CustomMessagingManager.RegisterNamedMessageHandler(MESSAGE, OnRequest);

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
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
}
