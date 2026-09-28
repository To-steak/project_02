using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldSelectionService : NetworkBehaviour, IWorldSelectionService
{
    public override void OnNetworkSpawn()
    {
        GameServices.Register(this);
    }

    public override void OnNetworkDespawn()
    {
        GameServices.Unregister(this);
    }

    public void RequestWorld(WorldId world)
    {
        RequestWorldRpc(world);
    }

    [Rpc(SendTo.Server)]
    private void RequestWorldRpc(WorldId world)
    {
        string scene = world switch
        {
            WorldId.Tutorial => "TUTORIAL WORLD",
            WorldId.Test => "TEST WORLD",
            _ => null
        };

        if (scene == null)
        {
            Debug.LogWarning($"[{nameof(WorldSelectionService)}] invalid world: {world}", this);
            return;
        }

        NetworkManager.SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }
}
