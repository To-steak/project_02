using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerBootstrap : MonoBehaviour
{
    private const string GAME_SCENE = "TEST";
    private const ushort DEFAULT_PORT = 7777;

    private void Start()
    {
        bool isServer = Application.isBatchMode || HasArg("-server");
        if (!isServer) return;

        ushort port = ushort.TryParse(GetArg("-port"), out var p) ? p : DEFAULT_PORT;

        var nm = NetworkManager.Singleton;
        nm.GetComponent<UnityTransport>().SetConnectionData("0.0.0.0", port);

        if (!nm.StartServer())
        {
            Debug.LogError($"failed: port {port}");
            Application.Quit(1);
            return;
        }

        Debug.Log($"success: port {port}");
        nm.SceneManager.LoadScene(GAME_SCENE, LoadSceneMode.Single);
    }

    private static bool HasArg(string name)
        => System.Array.IndexOf(System.Environment.GetCommandLineArgs(), name) >= 0;

    private static string GetArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        int i = System.Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }
}