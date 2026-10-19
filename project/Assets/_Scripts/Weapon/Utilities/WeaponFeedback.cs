using UnityEngine;
using Project02.Player;

namespace Project02.Weapon
{
    // 유틸(연출 전용): 무기 데이터만으로 계산하는 체감 효과. Random 사용 → 시뮬레이션에 쓰지 말 것
    public static class WeaponFeedback
    {
        public static float StanceMultiplier(SOWeapon data, FireStance stance)
        {
            return stance == FireStance.Ads ? data.AdsRecoilMultiplier : 1.0f;
        }

        public static Vector2 RollRecoil(SOWeapon data, FireStance stance)
        {
            return new Vector2(Random.Range(-data.RecoilHorizontal, data.RecoilHorizontal), data.RecoilVertical) * StanceMultiplier(data, stance);
        }
    }
}