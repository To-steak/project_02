using System.Collections.Generic;
using Project02.Combat;
using Project02.Enemy;
using Project02.Player;
using Project02.Weapon;

namespace Project02.World
{
    public sealed class ClientWorld
    {
        // Singleton Components
        public ClientClock Clock;
        public InterpolationClock Interp;
        public InputState Input;
        public CameraState Camera;
        public int AckTick = -1;   // 서버가 빠짐없이 받은 마지막 입력 틱 (클라 축). ReconcileSystem이 갱신, 입력 윈도우의 시작

        // 그린 시점 (서버 축). ViewTickSystem이 LateUpdate에 프레임당 1회 정한다 (정본은 여기 하나)
        public float ViewTick = -1.0f;        // RenderTick + ViewExtrapolation. 스냅샷 전 -1. 적 표시·InputPayload.ViewTick·피격 예측이 쓴다
        public float ViewExtrapolation;       // 되감기 상한을 넘는 만큼 앞으로 민 틱 수 (LagUtil). > 0이면 적을 외삽하고 적 피격 예측을 끈다
        public float ExpectedLag = -1.0f;     // 예상 되감기 양 (틱). 측정 전 -1. 디버그 표시용

        // 피격 예측 (연출만. 피해·체력·경험치는 서버 결과를 기다린다)
        public readonly List<ClientHitTarget> HitTargets = new();   // 피격 묶음 (원격 적). ClientLoop.Register/Unregister에서 Enemies와 함께
        public readonly List<Projectile> PredictedShots = new();    // PredictSystem이 처음 예측할 때 추가, ShotPredictSystem이 진행·제거
        public readonly List<ImpactInfo> PredictedImpacts = new();  // ShotPredictSystem이 적고 ImpactViewSystem이 LateUpdate에 비운다
        public readonly List<CapsuleSample> HitSamples = new();     // ShotPredictSystem의 재사용 버퍼 (매 프레임 할당하지 않는다)

        // Entities
        public PlayerOwnerAuthoring Local;
        public readonly Dictionary<ulong, PlayerRemoteAuthoring> Players = new();   // 원격 플레이어 (NetworkObjectId)
        public readonly Dictionary<ulong, EnemyRemoteAuthoring> Enemies = new();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public readonly LatencyProbe Probe = new();   // 디버그: 지연 측정
#endif
    }
}