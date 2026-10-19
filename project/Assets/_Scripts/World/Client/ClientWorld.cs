using System.Collections.Generic;
using Project02.Enemy;
using Project02.Player;

namespace Project02.Client
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
    }
}