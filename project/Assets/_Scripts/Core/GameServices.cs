public static class GameServices
{
    public static ICameraService Camera { get; private set; }
    public static IConnectionService Connection { get; private set; }
    public static ISettingsService Settings { get; private set; }

    public static void Register(ICameraService service) => Camera = service;
    public static void Register(IConnectionService service) => Connection = service;
    public static void Register(ISettingsService service) => Settings = service;

    public static void Unregister(ICameraService service)
    {
        if (ReferenceEquals(Camera, service)) Camera = null;
    }

    public static void Unregister(IConnectionService service)
    {
        if (ReferenceEquals(Connection, service)) Connection = null;
    }

    public static void Unregister(ISettingsService service)
    {
        if (ReferenceEquals(Settings, service)) Settings = null;
    }
}