using System.Collections.Generic;
using UnityEngine;
using Project02.Character;
using Project02.Weapon;
using Project02.World;

namespace Project02.Combat
{
    // 투사체 이동 + 판정. 1틱 처리는 ProjectileStepUtil (클라 피격 예측과 같은 함수)
    // 대상은 쏜 사람 시점으로 되감아 샘플한다. 피격 묶음(world.Hittables)만 본다.
    // 적인지 플레이어인지 모르고, 같은 편이면 건너뛴다 (규칙 11)
    public static class ProjectileSystem
    {
        public static void Tick(ServerWorld world, float deltaTime)
        {
            List<Projectile> projectiles = world.Projectiles;
            List<HitTarget> targets = world.Hittables;
            List<CapsuleSample> samples = world.HitSamples;
            int tick = world.Clock.Tick;

            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                Projectile projectile = projectiles[i];

                // 쏜 사람이 보던 시점으로 되감은 대상 (같은 편·사망 제외). 투사체마다 시점과 편이 다르므로 매번 다시 채운다
                Sample(targets, projectile.Team, tick - projectile.Lag, samples);

                ProjectileStepResult result = ProjectileStepUtil.Step(ref projectile, samples, deltaTime, out ProjectileHit hit);
                if (result == ProjectileStepResult.Flying)
                {
                    projectiles[i] = projectile;
                    continue;
                }

                if (result == ProjectileStepResult.Target)
                {
                    CapsuleSample sample = samples[hit.Sample];
                    HitTarget target = targets[sample.Index];
                    int damage = projectile.GetDamage(hit.Distance);
                    world.Hits.Add(new HitInfo(target.Health, DamageInfo.From(projectile.Source, damage, projectile.Owner, projectile.Origin, projectile.IsCritical)));
#if UNITY_EDITOR
                    DrawRewind(target.Rewind, sample, tick, projectile.Position, hit.Point);
#endif
                }

                // 대상·벽에 맞음 / 사거리 끝
                projectiles.RemoveAt(i);
            }
        }

        private static void Sample(List<HitTarget> targets, Team team, float sampleTick, List<CapsuleSample> samples)
        {
            samples.Clear();
            for (int i = 0; i < targets.Count; i++)
            {
                HitTarget candidate = targets[i];
                if (candidate.Team == team) continue;   // 같은 편
                if (candidate.Health.IsDead) continue;

                RewindTarget rewind = candidate.Rewind;
                if (!RewindUtil.TrySample(rewind, sampleTick, out Vector3 position)) continue;   // 그 시점에 없었음

                samples.Add(new CapsuleSample(position, rewind.Radius, rewind.Height, i));
            }
        }

#if UNITY_EDITOR
        private const float DRAW_SECONDS = 3.0f;

        // 지연 보상 시각화 (맞힌 대상만): 빨강 = 직전 틱에 기록된 위치(적은 이 단계 뒤에 움직이므로 서버의 현재 위치와 같다),
        // 초록 = 쏜 사람이 보던 위치(판정), 노랑 = 탄 경로, 흰색 = 되감은 거리
        private static void DrawRewind(RewindTarget rewind, in CapsuleSample sample, int tick, Vector3 start, Vector3 point)
        {
            if (RewindUtil.TrySample(rewind, tick - 1, out Vector3 now))
            {
                CharacterPhysics.GetCapsule(now, rewind.Radius, rewind.Height, out Vector3 nowBottom, out Vector3 nowTop);
                DrawCapsule(nowBottom, nowTop, rewind.Radius, Color.red);
                Debug.DrawLine(sample.Position, now, Color.white, DRAW_SECONDS);
            }
            CharacterPhysics.GetCapsule(sample.Position, sample.Radius, sample.Height, out Vector3 bottom, out Vector3 top);
            DrawCapsule(bottom, top, sample.Radius, Color.green);
            Debug.DrawLine(start, point, Color.yellow, DRAW_SECONDS);
        }

        // 축 + 위아래 십자로 캡슐 윤곽을 대략 그린다
        private static void DrawCapsule(Vector3 bottom, Vector3 top, float radius, Color color)
        {
            Debug.DrawLine(bottom, top, color, DRAW_SECONDS);
            foreach (Vector3 axis in new[] { Vector3.right, Vector3.forward })
            {
                Vector3 r = axis * radius;
                Debug.DrawLine(bottom - r, top - r, color, DRAW_SECONDS);
                Debug.DrawLine(bottom + r, top + r, color, DRAW_SECONDS);
                Debug.DrawLine(top - r, top + r, color, DRAW_SECONDS);
                Debug.DrawLine(bottom - r, bottom + r, color, DRAW_SECONDS);
            }
        }
#endif
    }
}
