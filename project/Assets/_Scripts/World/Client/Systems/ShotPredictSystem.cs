using System.Collections.Generic;
using UnityEngine;
using Project02.Combat;
using Project02.Enemy;
using Project02.Weapon;

namespace Project02.World
{
    // 클라 피격 예측: 예측 탄(world.PredictedShots)을 서버와 같은 ProjectileStepUtil로 진행하고,
    // 맞은 지점과 종류만 world.PredictedImpacts에 적는다 (연출은 LateUpdate의 ImpactViewSystem).
    // 피해·체력·경험치·히트 표시는 바꾸지 않는다 (서버 결과를 기다림). 오예측(이펙트는 났는데 피해 없음)은 되돌리지 않는다
    public static class ShotPredictSystem
    {
        public static void Tick(ClientWorld world, int ticks, float deltaTime)
        {
            List<Projectile> shots = world.PredictedShots;
            if (shots.Count == 0) return;

            // 적은 서버가 되감는 시점(쏜 사람이 보던 화면)에 샘플한다. InputPayload.ViewTick과 같은 값(world.ViewTick).
            // 외삽 중(ViewExtrapolation > 0)이면 적 피격 예측을 끈다: 외삽 위치는 서버 기록과 다를 수 있다. 벽은 정지 물체라 계속 예측
            List<CapsuleSample> samples = world.HitSamples;
            bool predictTargets = world.Interp.HasTick && world.ViewExtrapolation <= 0.0f;
            Sample(world.HitTargets, world.ViewTick, predictTargets, samples);

            // 한 프레임의 틱은 같은 시점에 샘플한다 (같은 프레임의 입력은 모두 같은 ViewTick을 보낸다).
            // 프레임 안에서 늦게 쏜 탄도 ticks번 진행하므로 다틱 프레임에서는 최대 (ticks − 1)틱 먼저 나간다. 60fps 이상이면 ticks ≤ 1
            List<ImpactInfo> impacts = world.PredictedImpacts;
            for (int step = 0; step < ticks; step++)
            {
                for (int i = shots.Count - 1; i >= 0; i--)
                {
                    Projectile shot = shots[i];
                    ProjectileStepResult result = ProjectileStepUtil.Step(ref shot, samples, deltaTime, out ProjectileHit hit);

                    switch (result)
                    {
                        case ProjectileStepResult.Flying:
                            shots[i] = shot;
                            continue;
                        case ProjectileStepResult.Target:
                            impacts.Add(new ImpactInfo(hit.Point, hit.Normal, ImpactKind.Target));
                            break;
                        case ProjectileStepResult.Wall:
                            impacts.Add(new ImpactInfo(hit.Point, hit.Normal, ImpactKind.Wall));
                            break;
                    }

                    shots.RemoveAt(i);
                }

                if (shots.Count == 0) return;
            }
        }

        // 원격 적의 캡슐을 viewTick에 보간 (EnemyViewSystem과 같은 식). 플레이어 총알은 플레이어를 맞히지 않으므로 적만
        private static void Sample(List<ClientHitTarget> targets, float viewTick, bool predictTargets, List<CapsuleSample> samples)
        {
            samples.Clear();
            if (!predictTargets) return;

            for (int i = 0; i < targets.Count; i++)
            {
                ClientHitTarget target = targets[i];
                if (!SnapshotUtil.TrySample(target.Snapshots, viewTick, out EnemySnapshot from, out EnemySnapshot to, out float t)) continue;

                Vector3 position = Vector3.Lerp(from.Movement.Position, to.Movement.Position, t);
                samples.Add(new CapsuleSample(position, target.Radius, target.Height, i));
            }
        }
    }
}
