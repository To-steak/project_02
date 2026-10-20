using UnityEngine;
using Project02.Weapon;

namespace Project02.Player
{
    // 컴포넌트(데이터): 플레이어 시뮬레이션 상태. World마다 하나씩 소유한다
    // PlayerServerAuthoring.Simulation = 서버 권한 / PlayerOwnerAuthoring.Simulation = 예측 사본
    public sealed class PlayerSimulation
    {
        // 설정 (Authoring이 채운다)
        public SOMovement Movement;
        public SOCatalog Catalog;

        // 접근 도우미 (규칙 1): 정본은 State.Weapon.EquippedId 하나. 없으면 null
        public SOWeapon Weapon => Catalog.Get(State.Weapon.EquippedId);

        // 상태 (스냅샷, 리컨사일 대상)
        public PlayerSimulationState State;

        // 파생값 / 레벨 스탯
        public bool IsRunning;           // 마지막 Step 결과. 연출용

        // 마지막 Step의 입력 (스냅샷·연출용. 리컨사일 대상 아님)
        public float Yaw;
        public float Pitch;
        public Vector2 Move;
    }
}