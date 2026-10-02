using System.Collections.Generic;

public interface ISessionService
{
    string GetNickname(ulong clientId);
    void AddReward(IReadOnlyList<ulong> clientIds, int exp, int gold);
}