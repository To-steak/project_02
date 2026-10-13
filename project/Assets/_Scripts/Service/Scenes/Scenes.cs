public enum WorldId
{
    Tutorial,
    Test
}

public static class Scenes
{
    public const string MainMenu = "MAIN MENU";
    public const string WaitingRoom = "WAITING ROOM";

    public static bool TryGetWorld(WorldId world, out string scene)
    {
        scene = world switch
        {
            WorldId.Tutorial => "TUTORIAL WORLD",
            WorldId.Test => "TEST WORLD",
            _ => null
        };
        return scene != null;
    }
}