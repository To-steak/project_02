using System.Collections.Generic;
using Project02.Player;
using UnityEngine;

namespace Project02.Server
{
    public sealed class Outbox
    {
        public readonly List<StateMessage> States = new();
        public readonly List<FireMessage> Fires = new();
    }

    public readonly struct StateMessage
    {
        public readonly PlayerServer Target;
        public readonly StatePayload Payload;

        public StateMessage(PlayerServer target, in StatePayload payload)
        {
            Target = target;
            Payload = payload;
        }
    }

    public readonly struct FireMessage
    {
        public readonly PlayerServer Shooter;
        public readonly Vector3 AimPoint;
        public readonly Vector2 Spread;

        public FireMessage(PlayerServer shooter, Vector3 aimPoint, Vector2 spread)
        {
            Shooter = shooter;
            AimPoint = aimPoint;
            Spread = spread;
        }
    }
}