public interface IDamageable
{
    /// <summary>
    /// 서버 전용 피해를 입히는 함수.
    /// </summary>
    /// <param name="amount">피해량</param>
    /// <param name="attacker">공격자(null이면 플레이어가 아닌 공격이다.)</param>
    void ApplyDamage(int amount, ulong? attacker);
}