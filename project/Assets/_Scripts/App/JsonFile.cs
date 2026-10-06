using System;
using System.IO;
using UnityEngine;

public static class JsonFile
{
    public static bool TrySave<T>(string path, T data)
    {
        string temp = path + ".tmp";
        try
        {
            File.WriteAllText(temp, JsonUtility.ToJson(data, true));
            if (File.Exists(path))
            {
                File.Replace(temp, path, null);
            }
            else
            {
                File.Move(temp, path);
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"{nameof(JsonFile)} save failed ({path}): {e.Message}");
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(temp);
                }
            }
            catch { }
            return false;
        }
    }

    public static bool TryLoad<T>(string path, out T data)
    {
        data = default;
        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            data = JsonUtility.FromJson<T>(File.ReadAllText(path));
            return data != null;
        }
        catch (Exception e)
        {
            data = default;
            Debug.LogError($"{nameof(JsonFile)} load failed ({path}): {e.Message}");
            return false;
        }
    }
}
