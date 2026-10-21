using Project02.World;

namespace Project02.Combat
{
    // 죽은 대상을 찾아(질의) 보상과 Despawn을 큐에 넣는다. 실제 Despawn은 DespawnSystem (Step 끝)
    // 사망 묶음(world.Mortals)만 본다. 적인지 플레이어인지 모른다 (규칙 11)
    public static class DeathSystem
    {
        public static void Process(ServerWorld world)
        {
            foreach (Mortal mortal in world.Mortals)
            {
                Health health = mortal.Health;
                if (!health.IsDead) continue;

                int count = health.Contributors.Count;
                if (count > 0)
                {
                    int exp = mortal.Exp / count;
                    int gold = mortal.Gold / count;

                    // 복사: SessionSystem이 처리하기 전에 원본이 바뀌어도 안전하게
                    world.Rewards.Add(new RewardInfo(health.Contributors.ToArray(), exp, gold));
                }

                // 디스폰은 틱 끝(DespawnSystem)에 일어나므로 같은 대상이 한 틱에 두 번 들어가지 않는다
                // (이 System은 틱마다 한 번 돌고, Despawn → Unregister로 다음 틱에는 Mortals에 없다)
                world.Despawns.Add(mortal.Object);
            }
        }
    }
}
