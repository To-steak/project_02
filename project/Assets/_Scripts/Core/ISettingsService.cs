public interface ISettingsService
{
    GameSettings Saved { get; }
    void Preview(GameSettings settings);
    void Save(GameSettings settings);
}