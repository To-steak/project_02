using System;
using System.Collections.Generic;

public class JsonProfileStore : IProfileStore
{
    [Serializable]
    private class ProfileFile
    {
        public int Version = 1;
        public List<PlayerRecord> Profiles = new();
    }

    private readonly string _path;
    private readonly Dictionary<string, PlayerRecord> _profiles = new();

    public JsonProfileStore(string path)
    {
        _path = path;
        Load();
    }

    public bool TryGet(string userId, out PlayerRecord profile)
    {
        return _profiles.TryGetValue(userId, out profile);
    }

    public bool IsNicknameTaken(string nickname)
    {
        foreach (PlayerRecord profile in _profiles.Values)
        {
            if (string.Equals(profile.Nickname, nickname, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public void Add(PlayerRecord profile)
    {
        _profiles[profile.UserId] = profile;
        Save();
    }
    public void Save()
    {
        ProfileFile file = new ProfileFile();
        file.Profiles.AddRange(_profiles.Values);
        JsonFile.TrySave(_path, file);
    }

    private void Load()
    {
        if (!JsonFile.TryLoad(_path, out ProfileFile file) || file.Profiles == null)
        {
            return;
        }

        foreach (PlayerRecord profile in file.Profiles)
        {
            if (!string.IsNullOrEmpty(profile.UserId))
            {
                _profiles[profile.UserId] = profile;
            }
        }
    }
}