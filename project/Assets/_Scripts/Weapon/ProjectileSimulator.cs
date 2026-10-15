using System.Collections.Generic;
using Project02.Combat;
using Project02.Server;
using UnityEngine;

namespace Project02.Weapon
{
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

        public void Spawn(in Projectile projectile)
        {
            _projectiles.Add(projectile);
        }

        public void Tick(List<HitInfo> hits, float dt)
        {
            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                Projectile projectile = _projectiles[i];
                float traveled = Vector3.Distance(projectile.Origin, projectile.Position);
                float remaining = projectile.Range - traveled;
                Vector3 step = projectile.Velocity * dt;
                float length = Mathf.Min(step.magnitude, remaining);
                Vector3 direction = step.normalized;

                if (length > 0.0f && Physics.Raycast(projectile.Position, direction, out RaycastHit hit, length, _hitLayer, QueryTriggerInteraction.Ignore))
                {
                    if (hit.collider.TryGetComponent(out IDamageable target))
                    {
                        int damage = projectile.GetDamage(traveled + hit.distance);
                        hits.Add(new HitInfo(target, DamageInfo.FromPlayer(damage, projectile.Owner, projectile.Origin, projectile.IsCritical)));
                    }

                    _projectiles.RemoveAt(i);
                    continue;
                }

                if (length >= remaining)
                {
                    _projectiles.RemoveAt(i);
                    continue;
                }

                projectile.Position += direction * length;
                _projectiles[i] = projectile;
            }
        }
    }
}