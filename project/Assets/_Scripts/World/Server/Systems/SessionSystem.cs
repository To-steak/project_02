using Project02.Player;
using Project02.Service;
using Unity.Collections;

namespace Project02.Server
{
    // 세션(Iceberg) ↔ 플레이어 엔티티 사이의 유일한 통로
    public static class SessionSystem
    {
        // 스폰 시 1회 (ServerLoop.Register에서). 스폰 메시지에 실리도록 즉시 처리
        public static void Join(PlayerServerAuthoring player, SessionService session)
        {
            ulong clientId = player.OwnerClientId;

            string nickname = session.GetNickname(clientId) ?? $"Player#{clientId}";
            player.Network.Nickname.Value = new FixedString64Bytes(nickname);

            ApplyExp(player.Progression, session.GetExp(clientId));
        }

        // 매 틱 (Step). 보상 지급 → 받은 플레이어의 레벨 갱신
        public static void Process(ServerWorld world, SessionService session)
        {
            foreach (RewardInfo reward in world.Rewards)
            {
                session.AddReward(reward.ClientIds, reward.Exp, reward.Gold);

                foreach (ulong clientId in reward.ClientIds)
                {
                    if (TryFindPlayer(world, clientId, out PlayerServerAuthoring player))
                    {
                        ApplyExp(player.Progression, session.GetExp(clientId));
                    }
                }
            }

            world.Rewards.Clear();
        }

        // 경험치 → 레벨. 값이 같으면 NetworkVariable이 변경 콜백을 부르지 않는다
        private static void ApplyExp(PlayerProgression progression, int exp)
        {
            progression.Level.Value = progression.Table.GetLevel(exp);
        }

        private static bool TryFindPlayer(ServerWorld world, ulong clientId, out PlayerServerAuthoring player)
        {
            foreach (PlayerServerAuthoring candidate in world.Players)
            {
                if (candidate.OwnerClientId == clientId)
                {
                    player = candidate;
                    return true;
                }
            }

            player = null;
            return false;   // 보상 직전에 접속 종료함. 경험치는 저장됐고, 재접속하면 Join이 반영한다
        }
    }
}