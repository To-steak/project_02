using System.Collections.Generic;
using Project02.Enemy;
using Project02.Player;

namespace Project02.Client
{
    public sealed class ClientWorld
    {
        public ClientClock Clock;
        public InterpolationClock Interp;

        public PlayerOwner Local;
        public readonly Dictionary<ulong, EnemyRemote> Enemies = new();
    }
}