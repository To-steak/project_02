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
        public PlayerOwner Local;
        public readonly Dictionary<ulong, PlayerRemote> Players = new();   // 원격 플레이어 (NetworkObjectId)
        public readonly Dictionary<ulong, EnemyRemote> Enemies = new();
    }
}