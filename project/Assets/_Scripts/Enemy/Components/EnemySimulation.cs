using System.Collections.Generic;
using UnityEngine;
using Project02.Character;
using Project02.Navigation;

namespace Project02.Enemy
{
    public sealed class EnemySimulation
    {
        // 설정 (Authoring이 채운다)
        public SOEnemy Settings;
        public PathGrid Grid;

        // 이동
        public MovementState Movement;
        public Quaternion Rotation;
        public Vector2 MoveInput;        // 상태가 이번 틱에 정한다 (매 틱 0으로 초기화)
        public float MoveSpeed;

        // 상태 머신
        public EnemyStateId StateId;
        public int StateEnterTick;       // 상태에 들어간 서버 틱. 경과 = 현재 틱 − 이것
        public ulong? TargetClientId;

        // 배회
        public readonly List<int> Path = new();
        public int Waypoint;
        public int NextWanderTick = -1;  // 도착 후 다음 배회 틱. -1 = 대기 시작 전
    }
}