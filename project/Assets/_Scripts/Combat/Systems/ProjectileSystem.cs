using System.Collections.Generic;
using UnityEngine;
using Project02.Character;
using Project02.Enemy;
using Project02.Weapon;
using Project02.World;

namespace Project02.Combat
{
    // 투사체 이동 + 판정. 지형은 레이캐스트, 적은 쏜 사람 시점으로 되감은 캡슐
    public static class ProjectileSystem
    {
        private const float EPSILON = 1e-6f;

        public static void Tick(ServerWorld world, LayerMask hitLayer, float deltaTime)
        {
            List<Projectile> projectiles = world.Projectiles;
            int tick = world.Clock.Tick;

            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                Projectile projectile = projectiles[i];
                float traveled = Vector3.Distance(projectile.Origin, projectile.Position);
                float remaining = projectile.Range - traveled;
                Vector3 step = projectile.Velocity * deltaTime;
                float length = Mathf.Min(step.magnitude, remaining);
                Vector3 direction = step.normalized;

                if (length <= 0.0f)
                {
                    projectiles.RemoveAt(i);
                    continue;
                }

                // 1. 지형: 맞으면 그 거리가 이번 틱 선분의 끝
                bool hitWall = Physics.Raycast(projectile.Position, direction, out RaycastHit wall, length, hitLayer, QueryTriggerInteraction.Ignore);
                float maxDistance = hitWall ? wall.distance : length;

                // 2. 적: 쏜 사람이 보던 시점으로 되감아 판정 (벽보다 가까운 것만)
                float sampleTick = tick - projectile.Lag;
                if (TryHitEnemies(world.Enemies, sampleTick, projectile.Position, direction, maxDistance, out EnemyServerAuthoring target, out float distance))
                {
                    int damage = projectile.GetDamage(traveled + distance);
                    world.Hits.Add(new HitInfo(target.Health, DamageInfo.FromPlayer(damage, projectile.Owner, projectile.Origin, projectile.IsCritical)));
                    projectiles.RemoveAt(i);
                    continue;
                }

                // 3. 벽에 막힘 / 4. 사거리 끝
                if (hitWall || length >= remaining)
                {
                    projectiles.RemoveAt(i);
                    continue;
                }

                projectile.Position += direction * length;
                projectiles[i] = projectile;
            }
        }

        // 선분 [start, start + direction * maxDistance] 위에서 가장 먼저 닿는 적
        private static bool TryHitEnemies(List<EnemyServerAuthoring> enemies, float sampleTick, Vector3 start, Vector3 direction, float maxDistance,
                                          out EnemyServerAuthoring target, out float distance)
        {
            target = null;
            distance = maxDistance;

            foreach (EnemyServerAuthoring enemy in enemies)
            {
                if (enemy.Health.IsDead) continue;

                RewindTarget rewind = enemy.Rewind;
                if (!RewindUtil.TrySample(rewind, sampleTick, out Vector3 position)) continue;

                CharacterPhysics.GetCapsule(position, rewind.Radius, rewind.Height, out Vector3 bottom, out Vector3 top);
                if (TryIntersectCapsule(start, direction, distance, bottom, top, rewind.Radius, out float hit))
                {
                    distance = hit;   // 줄여 가며 찾으므로 다음 후보는 더 가까워야 통과
                    target = enemy;
                }
            }

            return target != null;
        }

        // 선분–캡슐 교차: 두 선분(탄 경로, 캡슐 축) 사이 최근접점 거리 ≤ radius
        // (Ericson, Real-Time Collision Detection 5.1.9)
        // hitDistance는 탄 선분 위 최근접점까지의 거리. 표면 진입점보다 약간 깊지만 거리 감쇠용이라 충분하다
        private static bool TryIntersectCapsule(Vector3 start, Vector3 direction, float maxDistance, Vector3 bottom, Vector3 top, float radius, out float hitDistance)
        {
            Vector3 d1 = direction * maxDistance;   // 탄 선분
            Vector3 d2 = top - bottom;              // 캡슐 축
            Vector3 r = start - bottom;

            float a = d1.sqrMagnitude;
            float e = d2.sqrMagnitude;
            float f = Vector3.Dot(d2, r);
            float s, t;

            if (a <= EPSILON)
            {
                s = 0.0f;
                t = e <= EPSILON ? 0.0f : Mathf.Clamp01(f / e);
            }
            else
            {
                float c = Vector3.Dot(d1, r);
                if (e <= EPSILON)   // 캡슐이 구 (높이 = 지름)
                {
                    t = 0.0f;
                    s = Mathf.Clamp01(-c / a);
                }
                else
                {
                    float b = Vector3.Dot(d1, d2);
                    float denominator = a * e - b * b;   // 평행이면 0

                    s = denominator > EPSILON ? Mathf.Clamp01((b * f - c * e) / denominator) : 0.0f;
                    t = (b * s + f) / e;

                    if (t < 0.0f)
                    {
                        t = 0.0f;
                        s = Mathf.Clamp01(-c / a);
                    }
                    else if (t > 1.0f)
                    {
                        t = 1.0f;
                        s = Mathf.Clamp01((b - c) / a);
                    }
                }
            }

            Vector3 closestOnShot = start + d1 * s;
            Vector3 closestOnAxis = bottom + d2 * t;

            if ((closestOnShot - closestOnAxis).sqrMagnitude > radius * radius)
            {
                hitDistance = 0.0f;
                return false;
            }

            hitDistance = s * maxDistance;
            return true;
        }
    }
}