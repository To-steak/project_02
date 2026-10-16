using System.IO;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project02.Service
{
    public class ServerLauncher : MonoBehaviour
    {
        [SerializeField] private string _firstScene = Scenes.WaitingRoom;

        private void Start()
        {
            if (!Application.isBatchMode)
            {
                return;
            }

            if (!OpenServer())
            {
                Application.Quit(1);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Open Server")]
#endif
        public bool OpenServer()
        {
            // 1. 세이브 열기 (StartServer 전에: Approve가 진행도를 조회한다)
            string savePath = SavePaths.Server();
            MigrateLegacy(savePath);

            ProgressService.OpenResult result = ProgressService.Instance.Open(savePath);
            if (result is ProgressService.OpenResult.Corrupt or ProgressService.OpenResult.Unsupported)
            {
                Debug.LogError($"[{nameof(ServerLauncher)}] cannot open save ({result}): {savePath}", this);
                return false;   // 진행도를 잃느니 시작하지 않는다
            }

            Debug.Log($"[{nameof(ServerLauncher)}] save {result}: {savePath}", this);

            // 2. 서버 시작
            NetworkManager networkManager = NetworkManager.Singleton;
            networkManager.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1", NetworkDefaults.Port, "0.0.0.0");

            if (!networkManager.StartServer())
            {
                Debug.LogError($"[{nameof(ServerLauncher)}] failed to start on port {NetworkDefaults.Port}", this);
                ProgressService.Instance.Close();
                return false;
            }

            Application.targetFrameRate = (int)networkManager.NetworkConfig.TickRate;
            networkManager.SceneManager.LoadScene(_firstScene, LoadSceneMode.Single);
            return true;
        }

        // 일회성: server.sav가 없고 옛 progress.json이 있으면 옮겨 담는다
        private static void MigrateLegacy(string savePath)
        {
            string legacy = SavePaths.Legacy();
            if (File.Exists(savePath) || !File.Exists(legacy)) return;
            if (JsonFile.Load(legacy, out LegacyProgressFile old) != LoadResult.Loaded) return;

            SaveFile file = new SaveFile();
            file.Players.AddRange(old.Progresses);
            if (JsonFile.TrySave(savePath, file))
            {
                File.Move(legacy, legacy + ".migrated");   // 지우지 않고 표시만
                Debug.Log($"[{nameof(ServerLauncher)}] migrated {legacy} → {savePath}");
            }
        }
    }
}