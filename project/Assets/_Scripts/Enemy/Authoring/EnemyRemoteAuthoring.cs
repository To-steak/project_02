using Unity.Netcode;
using UnityEngine;
using Project02.Character;
using Project02.Core;
using Project02.World;

namespace Project02.Enemy
{
    // Authoring: 클라 World 등록(종류 목록 + 피격 묶음) + 스냅샷 버퍼. 표시는 EnemyViewSystem, 피격 예측은 ShotPredictSystem
    public class EnemyRemoteAuthoring : NetworkBehaviour
    {
        public const int BUFFER_SIZE = 32;
        public readonly TickBuffer<EnemySnapshot> Snapshots = new(BUFFER_SIZE);

        [SerializeField] private SOEnemy _settings;   // 피격 캡슐 크기(Profile). 서버(EnemyServerAuthoring)와 같은 에셋

        public override void OnNetworkSpawn()
        {
            // 클라 World가 있는 프로세스면 등록 (순수 클라 + 호스트). !IsServer로 판정하지 않는다
            enabled = IsClient;
            if (!enabled) return;

            Snapshots.Clear();

            Debug.Assert(_settings != null, $"{name}: EnemyRemoteAuthoring._settings가 연결되지 않음", this);
            CharacterProfile profile = _settings.Profile;
            ClientLoop.Instance.Register(this, profile.Radius, profile.Height);
        }

        public override void OnNetworkDespawn()
        {
            if (!enabled) return;

            if (ClientLoop.Instance != null)
            {
                ClientLoop.Instance.Unregister(this);
            }
        }
    }
}