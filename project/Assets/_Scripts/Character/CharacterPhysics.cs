using UnityEngine;

/// <summary>
/// 캐릭터 이동을 계산하는 순수 함수 모음이다.
/// 상태를 갖지 않으며 <c>Transform</c>이나 <c>Rigidbody</c>를 직접 건드리지 않는다.
/// 입력과 현재 위치를 받아 "실제로 갈 수 있는 위치"를 돌려줄 뿐이다.
/// 결과를 어디에 반영할지는 호출자가 정한다.
/// </summary>
/// <remarks>
/// 일반적인 호출 순서는 다음과 같다.
/// 1. Walk
/// 2. ApplyGravity
/// 3. Collide
/// 4. IsGrounded
/// 5. Rotate
/// 
/// 위치는 모두 캡슐의 발밑 기준이다(<see cref="GetCapsule"/> 참고).
/// 물리 질의는 <c>CapsuleCast</c>로 수행한다.
/// <c>QueryTriggerInteraction.Ignore</c>로 IsTrigger는 무시한다.
/// </remarks>
public static class CharacterPhysics
{
    private const float SKIN = 0.02f; // 캐릭터과 벽과의 여유 거리
    private const float MIN_DISTANCE = 0.001f; // 1mm 미만 이동은 무시
    private const float MIN_NORMAL = 0.01f; // 수평 성분이 1% 미만이면 슬라이딩 불가
    private const float MIN_NORMAL_SQR = MIN_NORMAL * MIN_NORMAL;
    private const float MIN_INPUT = 0.01f;
    private const float MIN_INPUT_SQR = MIN_INPUT * MIN_INPUT;
    private const int MAX_SLIDE_COUNT = 3;

    /// <summary>
    /// 목표 지점까지 캡슐을 걸어서 이동시킨다.
    /// 부딪힌 면을 경사면-턱-벽으로 분류해 오를 수 있으면 오르고 아니면 미끄러진다.
    /// 경사와 계단을 무시하고 전부 벽으로 취급하려면 <see cref="Collide"/>를 쓸 것.
    /// </summary>
    /// <param name="origin">현재 위치</param>
    /// <param name="target">가려는 위치</param>
    /// <param name="radius">캡슐 반지름</param>
    /// <param name="height">캡슐 전체 높이</param>
    /// <param name="slopeLimit">걸어 오를 수 있는 최대 경사각(도)</param>
    /// <param name="stepHeight">그냥 올라설 수 있는 턱의 최대 높이</param>
    /// <param name="layer">충돌로 판정할 레이어</param>
    /// <param name="grounded">접지 여부</param>
    /// <param name="climbed">턱을 올라선 높이</param>
    /// <returns>실제로 도달한 위치</returns>
    /// <remarks>
    /// 미끄러짐은 최대 <c>MAX_SLIDE_COUNT</c>회까지만 계산하며 반복이 모자라면 남은 이동량이 버려진다.
    /// 턱을 올라선 경우에는 그 자리에서 루프를 끝내므로 한 프레임에 한 단씩만 오른다.
    /// 중력은 다루지 않는다.
    /// </remarks>
    public static Vector3 Walk(Vector3 origin, Vector3 target, float radius, float height, float slopeLimit, float stepHeight, LayerMask layer, bool grounded, out float climbed)
    {
        climbed = 0.0f;
        Vector3 position = origin;
        Vector3 delta = target - origin;

        for (int i = 0; i < MAX_SLIDE_COUNT; i++)
        {
            float distance = delta.magnitude;
            if (distance < MIN_DISTANCE)
            {
                break;
            }

            Vector3 direction = delta / distance;
            if (!Cast(position, direction, distance, radius, height, layer, out RaycastHit hit))
            {
                position += delta;
                break;
            }

            float moved = Mathf.Max(hit.distance - SKIN, 0.0f);
            position += direction * moved;
            Vector3 remain = delta - direction * moved;

            // 1. 경사면: 수평 성분을 지우지 않고 면 위로 투영한다.
            if (IsWalkable(hit.normal, slopeLimit))
            {
                delta = Vector3.ProjectOnPlane(remain, hit.normal);
                continue;
            }

            // 2. 턱: 올라설 자리가 있으면 올라선다.
            if (grounded && TryStep(position, remain, radius, height, slopeLimit, stepHeight, layer, out Vector3 stepped))
            {
                climbed = stepped.y - position.y;
                position = stepped;
                break;
            }

            // 3. 벽: 수평으로 미끄러진다.
            Vector3 normal = new Vector3(hit.normal.x, 0.0f, hit.normal.z);
            if (normal.sqrMagnitude < MIN_NORMAL_SQR)
            {
                break;
            }

            delta = Vector3.ProjectOnPlane(remain, normal.normalized);
        }

        return position;
    }

    /// <summary>
    /// 수직 속도에 중력을 누적하고 최대 낙하 속도로 제한한다.
    /// 위로 향하는 속도(점프)는 제한하지 않는다.
    /// </summary>
    /// <param name="velocity">현재 수직 속도(seconds)</param>
    /// <param name="gravity">중력 가속도</param>
    /// <param name="maxSpeed">최대 낙하 속도</param>
    /// <param name="time">경과 시간</param>
    /// <returns>갱신된 수직 속도</returns>
    /// <remarks>
    /// 속도가 양수이면 위, 음수이면 아래로 향한다.
    /// 중력가속도는 음수로 넣으면 안 된다.
    /// </remarks>
    public static float ApplyGravity(float velocity, float gravity, float maxSpeed, float time)
    {
        return Mathf.Max(velocity - gravity * time, -maxSpeed);
    }

    /// <summary>
    /// 목표 지점까지 캡슐을 이동시키되 막히면 벽면을 따라 미끄러진다.
    /// 충돌면의 수평 성분만 사용하므로 경사면도 벽처럼 취급해 밀어낸다.
    /// 경사를 올라야 하거나 턱을 넘어야 한다면 <see cref="Walk"/>를 쓸 것.
    /// </summary>
    /// <param name="source">현재 위치</param>
    /// <param name="target">가려는 위치</param>
    /// <param name="radius">캡슐 반지름</param>
    /// <param name="height">캡슐 전체 높이</param>
    /// <param name="layer">충돌로 판정할 레이어</param>
    /// <returns>벽에서 <c>SKIN</c>만큼 떨어진 실제로 도달한 위치</returns>
    /// <remarks>
    /// 미끄러짐은 최대 <c>MAX_SLIDE_COUNT</c>회까지만 계산한다.
    /// 좁은 구석처럼 반복이 부족한 상황에서는 남은 이동량이 버려져 실제보다 덜 움직인다.
    /// 이미 벽에 파묻힌 상태는 밀어내지 않으므로 스폰 위치나 텔레포트 지점은 호출자가 확인해야 한다.
    /// </remarks>
    public static Vector3 Collide(Vector3 source, Vector3 target, float radius, float height, LayerMask layer)
    {
        Vector3 position = source;
        Vector3 delta = target - source;

        for (int i = 0; i < MAX_SLIDE_COUNT; i++)
        {
            float distance = delta.magnitude;
            if (distance < MIN_DISTANCE)
            {
                break;
            }

            Vector3 direction = delta / distance;
            if (!Cast(position, direction, distance, radius, height, layer, out RaycastHit hit))
            {
                position += delta;
                break;
            }

            float moved = Mathf.Max(hit.distance - SKIN, 0.0f);
            position += direction * moved;

            Vector3 remain = delta - direction * moved;
            Vector3 normal = new Vector3(hit.normal.x, 0.0f, hit.normal.z);
            if (normal.sqrMagnitude < MIN_NORMAL_SQR)
            {
                break;
            }

            delta = Vector3.ProjectOnPlane(remain, normal.normalized);
        }

        return position;
    }

    /// <summary>
    /// 충돌면이 걸어 오를 수 있는 경사인지 판정한다.
    /// 면의 기울기를 법선의 기울기로 바꿔서 Vector3.up과 비교해 경사 여부를 판단한다.
    /// </summary>
    /// <param name="normal">충돌면의 법선</param>
    /// <param name="slopeLimit">걸어 오를 수 있는 최대 경사각(도)</param>
    /// <returns>경사가 <paramref name="slopeLimit"/> 이하이면 true</returns>
    public static bool IsWalkable(Vector3 normal, float slopeLimit)
    {
        return Vector3.Angle(normal, Vector3.up) <= slopeLimit;
    }

    /// <summary>
    /// 접지 여부를 판정한다.
    /// </summary>
    /// <param name="target">낙하 후 가려던 위치</param>
    /// <param name="resolved">충돌을 반영해 실제로 도달한 위치</param>
    /// <param name="velocity">수직 속도</param>
    /// <returns>땅에 닿아 있으면 true.</returns>
    /// <remarks>
    /// 수직 속도가 0 이상이면 접지로 보지 않는다.
    /// 접지 상태에서도 속도를 완전히 0으로 두지 말고 작은 음수를 유지해야 한다.
    /// </remarks>
    public static bool IsGrounded(Vector3 target, Vector3 resolved, float velocity)
    {
        return velocity <= 0.0f && resolved.y > target.y;
    }

    /// <summary>
    /// 앞을 막은 것이 넘어갈 수 있는 턱인지 판정하고 올라선 위치를 계산한다.
    /// 1. 위로 들어올린다.
    /// 2. 앞으로 가본다.
    /// 3. 다시 내려놓는다.
    /// 하나라도 실패하면 턱이 아니라 벽이다.
    /// </summary>
    /// <param name="position">충돌해서 멈춘 위치</param>
    /// <param name="remain">아직 가지 못한 이동량</param>
    /// <param name="radius">캡슐 반지름</param>
    /// <param name="height">캡슐 전체 높이</param>
    /// <param name="slopeLimit">올라선 자리의 바닥이 걸을 수 있는 경사인지 판정할 기준(도)</param>
    /// <param name="stepHeight">올라설 수 있는 턱의 최대 높이</param>
    /// <param name="layer">충돌로 판정할 레이어.</param>
    /// <param name="stepped">성공 시 올라선 위치</param>
    /// <returns>턱을 올라설 수 있는지</returns>
    private static bool TryStep(Vector3 position, Vector3 remain, float radius, float height, float slopeLimit, float stepHeight, LayerMask layer, out Vector3 stepped)
    {
        stepped = position;

        Vector3 horizontal = new Vector3(remain.x, 0.0f, remain.z);
        float distance = horizontal.magnitude;
        if (stepHeight < MIN_DISTANCE || distance < MIN_DISTANCE)
        {
            return false;
        }

        Vector3 direction = horizontal / distance;

        // 1. 위로 들어올린다.
        float lift = stepHeight;
        if (Cast(position, Vector3.up, lift, radius, height, layer, out RaycastHit ceiling))
        {
            lift = Mathf.Max(ceiling.distance - SKIN, 0.0f);
            if (lift < MIN_DISTANCE)
            {
                return false;
            }
        }

        // 2. 앞으로 가본다.
        float forwardDistance = Mathf.Max(distance, radius);
        Vector3 raised = position + Vector3.up * lift;
        if (Cast(raised, direction, forwardDistance, radius, height, layer, out _))
        {
            return false;
        }

        // 3. 다시 내려놓는다.
        Vector3 forward = raised + direction * forwardDistance;
        if (!Cast(forward, Vector3.down, lift, radius, height, layer, out RaycastHit ground))
        {
            return false;
        }

        if (!IsWalkable(ground.normal, slopeLimit))
        {
            return false;
        }

        stepped = forward + Vector3.down * Mathf.Max(ground.distance - SKIN, 0.0f);
        stepped = new Vector3(position.x + direction.x * distance, stepped.y, position.z + direction.z * distance);
        return stepped.y > position.y;
    }

    /// <summary>
    /// 지정한 위치의 캡슐을 한 방향으로 쓸어 충돌을 검사한다.
    /// 이 클래스의 모든 물리 질의가 거쳐 가는 지점이다.
    /// </summary>
    /// <param name="position">캡슐의 발밑 위치</param>
    /// <param name="direction">쓸어갈 방향</param>
    /// <param name="distance">검사할 거리</param>
    /// <param name="radius">캡슐 반지름</param>
    /// <param name="height">캡슐 전체 높이</param>
    /// <param name="layer">충돌로 판정할 레이어</param>
    /// <param name="hit">충돌 정보</param>
    /// <returns>무언가에 부딪히면 true</returns>
    /// <remarks>
    /// 캡슐의 발밑 위치에서 검사한다.
    /// 방향은 정규화되어 있어야 한다.
    /// 캡슐은 <c>SKIN</c>만큼 더 멀리 쏜다.
    /// 충돌체가 IsTrigger라면 무시한다.
    /// </remarks>
    private static bool Cast(Vector3 position, Vector3 direction, float distance, float radius, float height, LayerMask layer, out RaycastHit hit)
    {
        GetCapsule(position, radius, height, out Vector3 bottom, out Vector3 top);
        return Physics.CapsuleCast(bottom, top, radius, direction, out hit, distance + SKIN, layer, QueryTriggerInteraction.Ignore);
    }

    /// <summary>
    /// 발밑 위치로부터 캡슐의 두 구 중심을 계산한다.
    /// </summary>
    /// <param name="position">캡슐의 발밑 위치</param>
    /// <param name="radius">캡슐 반지름</param>
    /// <param name="height">캡슐 전체 높이</param>
    /// <param name="bottom">아래쪽 구의 중심</param>
    /// <param name="top">위쪽 구의 중심</param>
    /// <remarks>
    /// 캡슐의 발밑이 지면에 닿는다.
    /// 높이가 지름보다 작으면 지름으로 보정한다.
    /// 발밑에서 반지름만큼 위가 <paramref name="bottom"/>이다.
    /// 정수리에서 반지름만큼 아래가 <paramref name="top"/>이다.
    /// </remarks>
    public static void GetCapsule(Vector3 position, float radius, float height, out Vector3 bottom, out Vector3 top)
    {
        float clamped = Mathf.Max(height, radius * 2.0f);
        bottom = position + Vector3.up * radius;
        top = position + Vector3.up * (clamped - radius);
    }

    /// <summary>
    /// 입력 방향을 바라보도록 회전을 보간한다.
    /// 입력이 Vector.zero이면 회전하지 않고 <paramref name="source"/>을 그대로 돌려준다.
    /// 멈춘 캐릭터는 마지막으로 향하던 방향을 유지한다.
    /// </summary>
    /// <param name="source">현재 회전</param>
    /// <param name="input">바라볼 방향</param>
    /// <param name="speed">초당 회전 각도(도)</param>
    /// <param name="time">경과 시간</param>
    /// <returns>보간된 회전</returns>
    public static Quaternion Rotate(Quaternion source, Vector2 input, float speed, float time)
    {
        Vector3 direction = new Vector3(input.x, 0.0f, input.y);
        if (direction.sqrMagnitude < MIN_INPUT_SQR)
        {
            return source;
        }

        Quaternion rotation = Quaternion.LookRotation(direction);
        return Quaternion.RotateTowards(source, rotation, speed * time);
    }
}