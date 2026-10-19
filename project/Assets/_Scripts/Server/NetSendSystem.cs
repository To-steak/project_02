namespace Project02.Server
{
    public static class NetSendSystem
    {
        public static void Flush(ServerWorld world)
        {
            Outbox outbox = world.Outbox;

            foreach (StateMessage message in outbox.States)
            {
                if (message.Target == null) continue;   // Unity ==: 파괴된 객체 거름
                message.Target.Network.StateRPC(message.Payload);
            }

            foreach (FireMessage message in outbox.Fires)
            {
                if (message.Shooter == null) continue;
                message.Shooter.Network.FireRPC(message.Tick, message.AimPoint, message.Spread);
            }

            outbox.States.Clear();
            outbox.Fires.Clear();
        }
    }
}