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

        // Entities
        public PlayerOwnerAuthoring Local;
        public readonly Dictionary<ulong, PlayerRemoteAuthoring> Players = new();   // 원격 플레이어 (NetworkObjectId)
        public readonly Dictionary<ulong, EnemyRemoteAuthoring> Enemies = new();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public readonly LatencyProbe Probe = new();   // 디버그: 지연 측정
#endif
    }
}