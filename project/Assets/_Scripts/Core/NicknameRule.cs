public static class NicknameRule
{
    public const int MIN_LENGTH = 2;
    public const int MAX_LENGTH = 12;

    public static bool TryNormalize(string input, out string nickname, out string error)
    {
        nickname = input?.Trim() ?? string.Empty;

        if (nickname.Length < MIN_LENGTH || nickname.Length > MAX_LENGTH)
        {
            error = $"nickname must be {MIN_LENGTH}-{MAX_LENGTH} characters";
            return false;
        }

        foreach (char digit in nickname)
        {
            if (!char.IsLetterOrDigit(digit) && digit != '_')
            {
                error = "only letters, digits and _ are allowed";
                return false;
            }
        }

        error = null;
        return true;
    }
}
