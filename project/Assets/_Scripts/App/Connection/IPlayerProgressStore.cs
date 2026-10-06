public interface IPlayerProgressStore
{
    bool TryGet(string userId, out PlayerProgress profile);
    bool IsNicknameTaken(string nickname);
    void Add(PlayerProgress profile);
    void Save();
}