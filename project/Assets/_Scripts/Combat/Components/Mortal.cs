// 튜플: Health + NetworkObject + 보상(Exp, Gold)
using Unity.Netcode;

namespace Project02.Combat
{
    // 사망 묶음. 사망 시스템(DeathSystem)은 이 목록만 보고, 적인지 플레이어인지 모른다
    // 등록·해제는 ServerLoop.Register/Unregister (해제는 같은 Health 참조로 찾는다)
    public readonly struct Mortal
    {
        public readonly Health Health;
        public readonly NetworkObject Object;
        public readonly int Exp;    // 처치 보상 총량 (기여자 수로 나눈다)
        public readonly int Gold;

        public Mortal(Health health, NetworkObject obj, int exp, int gold)
        {
            Health = health;
            Object = obj;
            Exp = exp;
            Gold = gold;
        }
    }
}
