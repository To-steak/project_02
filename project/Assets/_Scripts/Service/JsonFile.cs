using System;
using System.IO;
using UnityEngine;

namespace Project02.Service
{
    public enum LoadResult
    {
        Loaded,
        Missing,   // 파일 없음 (정상: 처음이거나 관리자가 지움)
        Corrupt,   // 파일은 있는데 읽을 수 없음 → 덮어쓰면 안 됨
    }

    public static class JsonFile
    {
        public static bool TrySave<T>(string path, T data)
        {
            string temp = path + ".tmp";
            try
            {
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);   // Saves/ 폴더가 없을 수 있음
                }

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
                    File.Delete(temp);
                }
                catch { }
                return false;
            }
        }
        
        public static LoadResult Load<T>(string path, out T data)
        {
            data = default;
            if (!File.Exists(path))
            {
                return LoadResult.Missing;
            }

            try
            {
                T parsed = JsonUtility.FromJson<T>(File.ReadAllText(path));
                if (parsed == null)   // class: 빈 파일 등. struct는 항상 false
                {
                    return LoadResult.Corrupt;
                }

                data = parsed;
                return LoadResult.Loaded;
            }
            catch (Exception e)
            {
                Debug.LogError($"{nameof(JsonFile)} load failed ({path}): {e.Message}");
                return LoadResult.Corrupt;
            }
        }

        // 기존 호출부 호환 (설정 파일 등 손상돼도 기본값으로 시작해도 되는 데이터용)
        public static bool TryLoad<T>(string path, out T data)
        {
            return Load(path, out data) == LoadResult.Loaded;
        }
    }
}