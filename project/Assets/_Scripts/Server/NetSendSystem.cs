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
                message.Target.Controller.PlayerOwner.StateRPC(message.Payload);
            }

            foreach (FireMessage message in outbox.Fires)
            {
                if (message.Shooter == null) continue;
                message.Shooter.Controller.PlayerRemote.FireRPC(message.AimPoint, message.Spread);
            }

            outbox.States.Clear();
            outbox.Fires.Clear();
        }
    }
}