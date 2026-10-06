using System;
using System.Collections.Generic;

public class PlayerProgressStore : IPlayerProgressStore
{
    [Serializable]
    private class ProfileFile
    {
        public int Version = 1;
        public List<PlayerProgress> Profiles = new();
    }

    private readonly string _path;
    private readonly Dictionary<string, PlayerProgress> _profiles = new();

    public PlayerProgressStore(string path)
    {
        _path = path;
        Load();
    }

    public bool TryGet(string userId, out PlayerProgress profile)
    {
        return _profiles.TryGetValue(userId, out profile);
    }

    public bool IsNicknameTaken(string nickname)
    {
        foreach (PlayerProgress profile in _profiles.Values)
        {
            if (string.Equals(profile.Nickname, nickname, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public void Add(PlayerProgress profile)
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

        foreach (PlayerProgress profile in file.Profiles)
        {
            if (!string.IsNullOrEmpty(profile.UserId))
            {
                _profiles[profile.UserId] = profile;
            }
        }
    }
}