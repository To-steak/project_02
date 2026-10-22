using System.Collections.Generic;
using UnityEngine;
using Project02.Character;
using Project02.Weapon;

namespace Project02.Combat
{
    // 읽기 Utility: 투사체 1틱 진행. 벽 레이캐스트 → 벽보다 가까운 첫 캡슐 → 전진
    // 서버 판정(ProjectileSystem)과 클라 피격 예측(ShotPredictSystem)이 같은 함수를 쓴다 → 같은 입력이면 같은 결과.
    // 대상 위치는 부르는 쪽이 샘플해서 넘긴다 (서버: RewindUtil, 클라: SnapshotUtil). 같은 편·사망 거르기도 부르는 쪽이 한다.
    // ref로 받은 투사체만 바꾼다 (Flying일 때 Position 전진)
    public static class ProjectileStepUtil
    {
        private const float EPSILON = 1e-6f;

        public static ProjectileStepResult Step(ref Projectile projectile, List<CapsuleSample> targets, float deltaTime, out ProjectileHit hit)
        {
            hit = default;

            float traveled = Vector3.Distance(projectile.Origin, projectile.Position);
            float remaining = projectile.Range - traveled;
            Vector3 step = projectile.Velocity * deltaTime;
            float length = Mathf.Min(step.magnitude, remaining);
            Vector3 direction = step.normalized;

            if (length <= 0.0f)
            {
                return ProjectileStepResult.Expired;
            }

            // 1. 지형: 맞으면 그 거리가 이번 틱 선분의 끝
            Vector3 start = projectile.Position;
            bool hitWall = Physics.Raycast(start, direction, out RaycastHit wall, length, projectile.HitLayer, QueryTriggerInteraction.Ignore);
            float maxDistance = hitWall ? wall.distance : length;

            // 2. 대상: 벽보다 가까운 것 중 가장 먼저 닿는 캡슐
            if (TryHitCapsules(targets, start, direction, maxDistance, out int sample, out float distance))
            {
                hit = new ProjectileHit(sample, traveled + distance, start + direction * distance, -direction);
                return ProjectileStepResult.Target;
            }

            // 3. 벽에 막힘
            if (hitWall)
            {
                hit = new ProjectileHit(-1, traveled + wall.distance, wall.point, wall.normal);
                return ProjectileStepResult.Wall;
            }

            // 4. 사거리 끝
            if (length >= remaining)
            {
                return ProjectileStepResult.Expired;
            }

            projectile.Position += direction * length;
            return ProjectileStepResult.Flying;
        }

        // 선분 [start, start + direction * maxDistance] 위에서 가장 먼저 닿는 캡슐. 목록 순서대로 보고, 더 가까워야 바뀐다
        private static bool TryHitCapsules(List<CapsuleSample> targets, Vector3 start, Vector3 direction, float maxDistance, out int sample, out float distance)
        {
            sample = -1;
            distance = maxDistance;

            for (int i = 0; i < targets.Count; i++)
            {
                CapsuleSample candidate = targets[i];
                CharacterPhysics.GetCapsule(candidate.Position, candidate.Radius, candidate.Height, out Vector3 bottom, out Vector3 top);
                if (TryIntersectCapsule(start, direction, distance, bottom, top, candidate.Radius, out float hit))
                {
                    distance = hit;   // 줄여 가며 찾으므로 다음 후보는 더 가까워야 통과
                    sample = i;
                }
            }

            return sample >= 0;
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
