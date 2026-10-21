namespace Project02.World
{
    // 그린 시점(world.ViewTick)의 정본을 프레임마다 한 번 정한다. 보간 시계(InterpolationSystem.Advance) 바로 뒤에 돈다.
    // 쓰는 곳: EnemyViewSystem(같은 LateUpdate), PredictSystem → InputPayload.ViewTick, ShotPredictSystem(다음 프레임 Update).
    // 다음 프레임 입력도 이 값을 보내므로 "보낸 시점 = 화면에 그려져 있던 시점"이 된다. 다른 곳에서 다시 계산하지 않는다
    public static class ViewTickSystem
    {
        public static void Update(ClientWorld world)
        {
            if (!world.Interp.HasTick)
            {
                world.ViewTick = -1.0f;
                world.ViewExtrapolation = 0.0f;
                world.ExpectedLag = -1.0f;
                return;
            }

            float extrapolation = LagUtil.GetExtrapolation(world.Interp, world.Clock, out world.ExpectedLag);
            world.ViewExtrapolation = extrapolation;
            world.ViewTick = world.Interp.RenderTick + extrapolation;   // 초과 0이면 RenderTick 그대로 (상한 이하 지연의 동작은 바뀌지 않는다)
        }
    }
}