public static class InputService
{
    private static PlayerAction _actions;
    public static PlayerAction Actions
    {
        get
        {
            if (_actions == null)
            {
                _actions = new PlayerAction();
                UnityEngine.Application.quitting += Release;
            }
            return _actions;
        }
    }

    public static float MouseSensitivity { get; set; } = 1.0f;

    private static void Release()
    {
        _actions?.Disable();
        _actions?.Dispose();
        _actions = null;
    }
}