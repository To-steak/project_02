using UnityEngine;

/// <summary>
/// 틱마다 이전 위치와 다음 위치 사이를 레이캐스트한다.
/// </summary>
public struct Projectile
{
    public Vector3 Position;
    public Vector3 Velocity;
    public float Lifespan; // 발사체의 남은 수명(초)
    public int Damage;
    public ulong Owner; // 1/n 분배용 기여자
}