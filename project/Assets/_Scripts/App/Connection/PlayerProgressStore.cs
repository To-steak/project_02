using System;
using System.Collections.Generic;

public class PlayerProgressStore : IPlayerProgressStore
{
    [Serializable]
    private class ProgressFile
    {
        public int Version = 1;
        public List<PlayerProgress> Progresses = new();
    }

    private readonly string _path;
    private readonly Dictionary<string, PlayerProgress> _progresses = new();

    public PlayerProgressStore(string path)
    {
        _path = path;
        Load();
    }

    public bool TryGet(string userId, out PlayerProgress progress)
    {
        return _progresses.TryGetValue(userId, out progress);
    }

    public bool IsNicknameTaken(string nickname)
    {
        foreach (PlayerProgress progress in _progresses.Values)
        {
            if (string.Equals(progress.Nickname, nickname, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public void Add(PlayerProgress progress)
    {
        _progresses[progress.UserId] = progress;
        Save();
    }

    public void Save()
    {
        ProgressFile file = new ProgressFile();
        file.Progresses.AddRange(_progresses.Values);
        JsonFile.TrySave(_path, file);
    }

    private void Load()
    {
        if (!JsonFile.TryLoad(_path, out ProgressFile file) || file.Progresses == null)
        {
            return;
        }

        foreach (PlayerProgress progress in file.Progresses)
        {
            if (!string.IsNullOrEmpty(progress.UserId))
            {
                _progresses[progress.UserId] = progress;
            }
        }
    }
}