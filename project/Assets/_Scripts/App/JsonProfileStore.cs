using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JsonProfileStore : IProfileStore
{
    [Serializable]
    private class ProfileFile
    {
        public int Version = 1;
        public List<PlayerProfile> Profiles = new();
    }

    private readonly string _path;
    private readonly Dictionary<string, PlayerProfile> _profiles = new();

    public JsonProfileStore(string path)
    {
        _path = path;
        Load();
    }

    public bool TryGet(string userId, out PlayerProfile profile)
    {
        return _profiles.TryGetValue(userId, out profile);
    }

    public bool IsNicknameTaken(string nickname)
    {
        foreach (PlayerProfile profile in _profiles.Values)
        {
            if (string.Equals(profile.Nickname, nickname, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public void Add(PlayerProfile profile)
    {
        _profiles[profile.UserId] = profile;
        Save();
    }

    public void Save()
    {
        try
        {
            ProfileFile file = new ProfileFile();
            file.Profiles.AddRange(_profiles.Values);

            string temp = _path + ".tmp";
            File.WriteAllText(temp, JsonUtility.ToJson(file, true));
            if (File.Exists(_path))
            {
                File.Replace(temp, _path, null);
            }
            else
            {
                File.Move(temp, _path);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"{nameof(JsonProfileStore)} save failed: {e.Message}");
        }
    }

    private void Load()
    {
        if (!File.Exists(_path))
        {
            return;
        }

        try
        {
            ProfileFile file = JsonUtility.FromJson<ProfileFile>(File.ReadAllText(_path));
            if (file?.Profiles == null)
            {
                return;
            }

            foreach (PlayerProfile profile in file.Profiles)
            {
                if (!string.IsNullOrEmpty(profile.UserId))
                {
                    _profiles[profile.UserId] = profile;
                }
            }
        }
        catch (Exception e)
        {
            string backup = $"{_path}.corrupt-{DateTime.Now:yyyyMMddHHmmss}";
            File.Copy(_path, backup, true);
            _profiles.Clear();
            Debug.LogError($"{nameof(JsonProfileStore)} load failed, backup: {backup}\n{e}");
        }
    }
}