namespace GameInterface
{
    public static class GameServices
    {
        public static ICameraService Camera { get; private set; }

        public static void Register(ICameraService service)
        {
            Camera = service;
        }

        public static void Unregister(ICameraService service)
        {
            if (ReferenceEquals(Camera, service))
            {
                Camera = null;
            }
        }
    }
}
