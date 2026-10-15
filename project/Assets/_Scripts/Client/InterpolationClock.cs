namespace Project02.Client
{
    // 싱글턴 컴포넌트: "지금 서버 틱 몇의 세계를 그리는가"
    // 예측 시계(ClientClock)와는 별개의 시간축이다.
    public struct InterpolationClock
    {
        public bool HasTick;       // 스냅샷을 한 번이라도 받았는가
        public int LatestTick;     // 받은 스냅샷 중 가장 최근 서버 틱
        public float RenderTick;   // 지금 그리는 서버 틱 (소수부 = 두 스냅샷 사이 비율)
    }
}