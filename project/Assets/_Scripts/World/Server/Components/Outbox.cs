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
        public readonly PlayerServerAuthoring Target;
        public readonly StatePayload Payload;

        public StateMessage(PlayerServerAuthoring target, in StatePayload payload)
        {
            Target = target;
            Payload = payload;
        }
    }

    public readonly struct FireMessage
    {
        public readonly PlayerServerAuthoring Shooter;
        public readonly int Tick;
        public readonly Vector3 AimPoint;
        public readonly Vector2 Spread;

        public FireMessage(PlayerServerAuthoring shooter, int tick, Vector3 aimPoint, Vector2 spread)
        {
            Shooter = shooter;
            Tick = tick;
            AimPoint = aimPoint;
            Spread = spread;
        }
    }
}