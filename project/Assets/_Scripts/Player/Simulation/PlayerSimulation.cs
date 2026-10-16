using Project02.Weapon;
using UnityEngine;

namespace Project02.Player
{
    // 컴포넌트(데이터): 예측과 서버가 공유하는 플레이어 시뮬레이션 상태. 로직 없음
    public sealed class PlayerSimulation
    {
        // 설정 (Authoring이 채운다)
        public SOMovement Movement;
        public SOWeapon Weapon;          // 장착 무기. 없으면 null

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