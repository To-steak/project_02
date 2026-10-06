using UnityEngine;

/// <summary>
/// 틱마다 이전 위치와 다음 위치 사이를 레이캐스트한다.
/// </summary>
public struct Projectile
{
    public Vector3 Origin;   // 발사 지점 (피격자가 바라볼 방향 계산용)
    public Vector3 Position;
    public Vector3 Velocity;
    public float Lifespan; // 발사체의 남은 수명(초)
    public int Damage;
    public ulong Owner; // 1/n 분배용 기여자
    public bool IsCritical;
    
    public Projectile(Vector3 origin, Vector3 velocity, float lifespan, int damage, ulong owner, bool isCritical)
    {
        Origin = origin;
        Position = origin;
        Velocity = velocity;
        Lifespan = lifespan;
        Damage = damage;
        Owner = owner;
        IsCritical = isCritical;
    }
}