public static class InputService
{
    private static PlayerAction _actions;
    public static PlayerAction Actions => _actions ??= new PlayerAction();
}
