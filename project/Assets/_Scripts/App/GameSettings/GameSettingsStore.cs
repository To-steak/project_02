using System.IO;
using UnityEngine;

public static class GameSettingsStore
{
    const string FILE_NAME = "settings.json";
    static string FILE_PATH => Path.Combine(Application.persistentDataPath, FILE_NAME);

    public static void Save(GameSettings settings)
    {
        JsonFile.TrySave(FILE_PATH, settings);
    }

    public static GameSettings Load()
    {
        if (!JsonFile.TryLoad(FILE_PATH, out GameSettings loaded))
        {
            return new GameSettings();
        }

        return Migrate(loaded);
    }

    private static GameSettings Migrate(GameSettings settings)
    {
        // 설정 구조가 바뀌어 버전이 오르면 여기서 변환.
        return settings;
    }
}