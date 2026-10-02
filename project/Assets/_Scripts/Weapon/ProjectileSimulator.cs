using System.Collections.Generic;
using UnityEngine;

public class ProjectileSimulator
{
    public int Count => _projectiles.Count;
    public IReadOnlyList<Projectile> Projectiles => _projectiles;

    private readonly LayerMask _hitLayer;
    private readonly List<Projectile> _projectiles = new();

    public ProjectileSimulator(LayerMask hitLayer)
    {
        _hitLayer = hitLayer;
    }

    public void Spawn(Vector3 origin, Vector3 velocity, float lifespan, int damage, ulong owner)
    {
        _projectiles.Add(new Projectile
        {
            Position = origin,
            Velocity = velocity,
            Lifespan = lifespan,
            Damage = damage,
            Owner = owner
        });
    }

    public void Tick(float dt)
    {
        for (int i = _projectiles.Count - 1; i >= 0; i--)
        {
            Projectile projectile = _projectiles[i];
            Vector3 next = projectile.Position + projectile.Velocity * dt;
            Vector3 segment = next - projectile.Position;
            if (Physics.Raycast(projectile.Position, segment.normalized, out RaycastHit hit, segment.magnitude, _hitLayer, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.TryGetComponent(out IDamageable target))
                {
                    target.ApplyDamage(projectile.Damage, projectile.Owner);
                }

                _projectiles.RemoveAt(i);
                continue;
            }

            projectile.Position = next;
            projectile.Lifespan -= dt;
            if (projectile.Lifespan <= 0f)
            {
                _projectiles.RemoveAt(i);
            }
            else
            {
                _projectiles[i] = projectile;
            }
        }
    }
}
