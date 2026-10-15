using UnityEngine;

namespace Project02.Weapon
{
    public struct Projectile
    {
        public Vector3 Origin;          // 발사 지점 (피격자가 바라볼 방향, 거리 감쇠 계산용)
        public Vector3 Position;
        public Vector3 Velocity;
        public float Damage;            // 감쇠 전 데미지. 맞았을 때 감쇠를 곱한 뒤 반올림한다.
        public ulong Owner;             // 1/n 분배용 기여자
        public bool IsCritical;
        public float Range;             // 최대 사거리
        public AnimationCurve Falloff;  // 가로: 날아간 거리 ÷ Range, 세로: 피해 배율

        public Projectile(Vector3 origin, Vector3 velocity, float damage, ulong owner, bool isCritical, float range, AnimationCurve falloff)
        {
            Origin = origin;
            Position = origin;
            Velocity = velocity;
            Damage = damage;
            Owner = owner;
            IsCritical = isCritical;
            Range = range;
            Falloff = falloff;
        }

        public int GetDamage(float distance)
        {
            float ratio = Range > 0.0f ? distance / Range : 1.0f;
            return Mathf.Max(1, Mathf.RoundToInt(Damage * Falloff.Evaluate(ratio)));
        }
    }
}