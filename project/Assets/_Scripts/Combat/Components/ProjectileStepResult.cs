namespace Project02.Combat
{
    // ProjectileStepUtil.Step의 결과. Flying이 아니면 투사체를 지운다
    public enum ProjectileStepResult : byte
    {
        Flying,    // 이번 틱 끝까지 날아감 (위치 갱신됨)
        Target,    // 대상 캡슐에 맞음
        Wall,      // 벽(HitLayer)에 막힘
        Expired,   // 사거리 끝
    }
}
