namespace Project02.Client
{
    public struct InterpolationClock
    {
        public bool HasTick;       // 스냅샷을 한 번이라도 받았는가
        public int LatestTick;     // 받은 스냅샷 중 가장 최근 서버 틱
        public float RenderTick;   // 지금 그리는 서버 틱 (소수부 = 두 스냅샷 사이 비율)
    }
}