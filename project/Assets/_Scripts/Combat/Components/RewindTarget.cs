using UnityEngine;
using Project02.Core;

namespace Project02.Combat
{
    public sealed class RewindTarget
    {
        // 되감기 상한 (서버 축 틱 수). 정본은 여기 하나: 서버 클램프(ServerPlayerSystem)와 클라 외삽 판단(LagUtil)이 같이 쓴다.
        // 19틱 = 380ms. 예상 lag가 이보다 큰 클라는 적을 외삽해서 그린다 (ECS_RULES 4장 "되감기 상한과 외삽")
        // 근거: RTT 약 220ms(11틱)부터 외삽 (강연 기준) = 보간 지연 5 + RTT 11 + 서버 입력 버퍼 3.
        // 상한은 RTT가 아니라 예상 lag 전체와 비교하므로 보간 지연과 버퍼를 더해야 한다 (12틱이면 RTT 약 80ms부터 외삽)
        public const int MAX_LAG_TICKS = 19;

        public const int CAPACITY = 64;   // 64틱 = 1.28초. MAX_LAG_TICKS(19)의 2배(38) 이상이어야 한다 (되감을 시점 + 보간용 다음 틱이 항상 남게)

        public readonly TickBuffer<Vector3> History = new(CAPACITY);   // 틱 끝의 발밑 위치 (스냅샷과 같은 시점)
        public float Radius;
        public float Height;
    }
}