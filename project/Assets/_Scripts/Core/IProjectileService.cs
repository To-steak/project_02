using UnityEngine;

public interface IProjectileService
{
    void Spawn(Vector3 origin, Vector3 velocity, float lifespan, int damage, ulong owner);
}