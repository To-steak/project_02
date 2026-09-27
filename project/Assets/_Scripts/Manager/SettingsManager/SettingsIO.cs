using System.IO;
using UnityEngine;

public static class SettingsIO
{
    const string FILE_NAME = "settings.json";
    static string FILE_PATH => Path.Combine(Application.persistentDataPath, FILE_NAME);

    public static void Save(GameSettings settings)
    {
        try
        {
            string json = JsonUtility.ToJson(settings, true);
            string path = FILE_PATH;
            string temp = path + ".tmp";

            File.WriteAllText(temp, json);
            if (File.Exists(path))
            {
                File.Replace(temp, path, null);
            }
            else
            {
                File.Move(temp, path);
            }
        }
        catch (IOException e)
        {
            Debug.LogError($"Settings IO: {e.Message}");
        }
    }

    public static GameSettings Load()
    {
        try
        {
            if (!File.Exists(FILE_PATH))
            {
                return new GameSettings();
            }

            string json = File.ReadAllText(FILE_PATH);
            GameSettings loaded = JsonUtility.FromJson<GameSettings>(json);
            if (loaded == null)
            {
                return new GameSettings();
            }

            return Migrate(loaded);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Settings IO: {e.Message}");
            return new GameSettings();
        }
    }

    private static GameSettings Migrate(GameSettings settings)
    {
        // 설정 구조가 바뀌어 버전이 오르면 여기서 변환.
        return settings;
    }
}