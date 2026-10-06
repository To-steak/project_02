public interface IPlayerProgressStore
{
    bool TryGet(string userId, out PlayerRecord profile);
    bool IsNicknameTaken(string nickname);
    void Add(PlayerRecord profile);
    void Save();
}