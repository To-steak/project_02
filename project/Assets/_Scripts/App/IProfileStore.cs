public interface IProfileStore
{
    bool TryGet(string userId, out PlayerProfile profile);
    bool IsNicknameTaken(string nickname);
    void Add(PlayerProfile profile);
    void Save();
}