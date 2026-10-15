using System.Collections.Generic;
using Project02.Player;
using Unity.Netcode;
using UnityEngine;

namespace Project02.Server
{
    public class ServerSimulation : MonoBehaviour
    {
        public static ServerSimulation Instance { get; private set; }

        private readonly List<PlayerServer> _players = new();

        private void Awake() => Instance = this;
        private void OnDestroy() { if (Instance == this) Instance = null; }

        public void Register(PlayerServer player) => _players.Add(player);
        public void Unregister(PlayerServer player) => _players.Remove(player);

        private void FixedUpdate()
        {
            NetworkManager networkManager = NetworkManager.Singleton;
            if (networkManager == null || !networkManager.IsServer) return;

            // 1~3. 플레이어: 입력 → 이동 → 발사 → 상태 전송
            foreach (PlayerServer player in _players)
            {
                player.Tick();
            }

            // 4~5. 투사체 (6-2)
            // 6. 피해 (6-3)
            // 7~8. 적 (6-2)
            // 9. 사망 (6-4)
        }
    }
}