using System.Collections.Generic;
using Project02.Enemy;
using Project02.Player;

namespace Project02.World
{
    public sealed class ClientWorld
    {
        // Singleton Components
        public ClientClock Clock;
        public InterpolationClock Interp;
        public InputState Input;
        public CameraState Camera;
        public int AckTick = -1;   // 서버가 빠짐없이 받은 마지막 입력 틱 (클라 축). ReconcileSystem이 갱신, 입력 윈도우의 시작

        // Entities
        public PlayerOwnerAuthoring Local;
        public readonly Dictionary<ulong, PlayerRemoteAuthoring> Players = new();   // 원격 플레이어 (NetworkObjectId)
        public readonly Dictionary<ulong, EnemyRemoteAuthoring> Enemies = new();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public readonly LatencyProbe Probe = new();   // 디버그: 지연 측정
#endif
    }
}