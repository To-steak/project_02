public static class InputService
{
    private static PlayerAction _actions;
    public static PlayerAction Actions => _actions ??= new PlayerAction();
    public static float MouseSensitivity { get; set; } = 1.0f;
}
