using UnityEngine;

/// <summary>
/// 캐릭터 이동을 계산하는 순수 함수 모음.
/// 상태를 갖지 않으며 <c>Transform</c>이나 <c>Rigidbody</c>를 직접 건드리지 않는다.
/// 입력과 현재 위치를 받아 "실제로 갈 수 있는 위치"를 돌려줄 뿐이므로,
/// 결과를 어디에 반영할지는 호출자가 정한다.
/// </summary>
/// <remarks>
/// 전형적인 호출 순서는 다음과 같다.
/// <code>
/// // 1. 의도: 입력을 목표 지점으로
/// Vector3 target = PhysicsManager.Move(position, input, speed, time);
///
/// // 2. 수평 이동: 경사·턱·벽을 해결
/// position = PhysicsManager.Walk(position, target, radius, height, slopeLimit, stepHeight, layer, grounded, out float climbed);                            
///
/// // 3. 수직 이동: 중력 적용 후 낙하
/// velocity = PhysicsManager.ApplyGravity(velocity, gravity, maxFallSpeed, time);
/// Vector3 fallen = PhysicsManager.Fall(position, velocity, time);
/// Vector3 resolved = PhysicsManager.Collide(position, fallen, radius, height, layer);
///
/// // 4. 접지 판정: 떨어지려 했으나 막혔으면 땅이다
/// grounded = PhysicsManager.IsGrounded(fallen, resolved, velocity);
/// if (grounded) velocity = 0.0f;
///
/// position = resolved;
/// transform.position = position;
///
/// // 5. 회전은 이동과 독립적으로
/// transform.rotation = PhysicsManager.Rotate(transform.rotation, input, rotationSpeed, time);
/// </code>
///
/// 위치는 모두 캡슐의 발밑 기준이다(<see cref="GetCapsule"/> 참고).
/// 모든 함수가 값 타입만 다루므로 힙 할당이 발생하지 않는다.
/// 물리 질의는 <c>CapsuleCast</c>로 수행하며 트리거는 무시한다.
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
    /// 입력 방향으로 이동한 목표 지점을 계산한다.
    /// 충돌은 고려하지 않으므로 결과를 <see cref="Walk"/>나 <see cref="Collide"/>에 넘겨 실제로 갈 수 있는 위치로 보정해야 한다.
    /// </summary>
    /// <param name="source">현재 위치.</param>
    /// <param name="input">이동 입력. x는 좌우, y는 앞뒤로 XZ 평면에 매핑된다. 
    /// 정규화하지 않아 벡터 크기가 그대로 속도 배율이 된다. 2D Vector Composite는 기본 모드(Digital Normalized)에서 대각 입력을 크기 1로 맞춰줌. 
    /// Digital 모드를 쓴다면 호출 전에 정규화할 것.</param>
    /// <param name="speed">초당 이동 거리.</param>
    /// <param name="time">경과 시간. 보통 <see cref="Time.fixedDeltaTime"/>사용</param>
    /// <returns>충돌을 반영하기 전의 목표 위치.</returns>
    public static Vector3 Move(Vector3 source, Vector2 input, float speed, float time)
    {
        Vector3 delta = new Vector3(input.x, 0.0f, input.y) * speed * time;
        return source + delta;
    }

    /// <summary>
    /// 입력 방향을 바라보도록 회전을 보간한다.
    /// 입력이 데드존 이하이면 회전하지 않고 <paramref name="source"/>을 그대로 돌려주므로,
    /// 멈춘 캐릭터는 마지막으로 향하던 방향을 유지한다.
    /// </summary>
    /// <param name="source">현재 회전.</param>
    /// <param name="input">바라볼 방향. x는 좌우, y는 앞뒤로 XZ 평면에 매핑되며 크기는 회전 속도에 영향을 주지 않는다(방향만 사용).</param>
    /// <param name="speed">초당 회전 각도(도).</param>
    /// <param name="time">경과 시간. 보통 <see cref="Time.fixedDeltaTime"/>.</param>
    /// <returns>보간된 회전. 입력이 없으면 <paramref name="source"/>.</returns>
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

    /// <summary>
    /// 목표 지점까지 캡슐을 이동시키되, 막히면 벽면을 따라 미끄러진다.
    /// 충돌면의 수평 성분만 사용하므로 경사면도 벽처럼 취급해 밀어낸다.
    /// 경사를 올라야 하거나 턱을 넘어야 한다면 <see cref="Walk"/>를 쓸 것.
    /// </summary>
    /// <param name="source">현재 위치. 캡슐의 발밑 기준이다(<see cref="GetCapsule"/> 참고).</param>
    /// <param name="target">가려는 위치. 보통 <see cref="Move"/>나 <see cref="Fall"/>의 결과.</param>
    /// <param name="radius">캡슐 반지름.</param>
    /// <param name="height">캡슐 전체 높이. 지름보다 작으면 지름으로 보정된다.</param>
    /// <param name="layer">충돌로 판정할 레이어. 트리거는 무시된다.</param>
    /// <returns>실제로 도달한 위치. 벽에서 <c>SKIN</c>만큼 떨어진 지점이다.</returns>
    /// <remarks>
    /// 미끄러짐은 최대 <c>MAX_SLIDE_COUNT</c>회까지만 계산한다.
    /// 좁은 구석처럼 반복이 부족한 상황에서는 남은 이동량이 버려져 실제보다 덜 움직인다.
    /// 이미 벽에 파묻힌 상태는 밀어내지 않으므로, 스폰 위치나 텔레포트 지점은 호출자가 확인해야 한다.
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
    /// 목표 지점까지 캡슐을 걸어서 이동시킨다.
    /// 부딪힌 면을 경사면 / 턱 / 벽으로 분류해, 오를 수 있으면 오르고 아니면 미끄러진다.
    /// 경사와 계단을 무시하고 전부 벽으로 취급하려면 <see cref="Collide"/>를 쓸 것.
    /// </summary>
    /// <param name="origin">현재 위치. 캡슐의 발밑 기준이다(<see cref="GetCapsule"/> 참고).</param>
    /// <param name="target">가려는 위치. 보통 <see cref="Move"/>의 결과.</param>
    /// <param name="radius">캡슐 반지름.</param>
    /// <param name="height">캡슐 전체 높이. 지름보다 작으면 지름으로 보정된다.</param>
    /// <param name="slopeLimit">걸어 오를 수 있는 최대 경사각(도). 이보다 가파르면 벽으로 취급한다.</param>
    /// <param name="stepHeight">그냥 올라설 수 있는 턱의 최대 높이.</param>
    /// <param name="layer">충돌로 판정할 레이어. 트리거는 무시된다.</param>
    /// <param name="grounded">접지 여부. 공중에서는 턱 오르기를 시도하지 않는다.</param>
    /// <param name="climbed">턱을 올라선 높이. 오르지 않았으면 0.
    /// 접지 판정이나 계단 소리 같은 후처리에 쓴다.</param>
    /// <returns>실제로 도달한 위치.</returns>
    /// <remarks>
    /// 미끄러짐은 최대 <c>MAX_SLIDE_COUNT</c>회까지만 계산하며, 반복이 모자라면 남은 이동량이 버려진다.
    /// 턱을 올라선 경우에는 그 자리에서 루프를 끝내므로 한 프레임에 한 단씩만 오른다.
    /// 중력은 다루지 않는다. 낙하는 <see cref="Fall"/>과 <see cref="ApplyGravity"/>로 따로 처리할 것.
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

            // 1. 걸어 오를 수 있는 경사면: 수평 성분을 지우지 않고 면 위로 투영한다.
            if (IsWalkable(hit.normal, slopeLimit))
            {
                delta = Vector3.ProjectOnPlane(remain, hit.normal);
                continue;
            }

            // 2. 턱/계단: 올라설 자리가 있으면 올라선다.
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
    /// 충돌면이 걸어 오를 수 있는 경사인지 판정한다.
    /// 면의 법선이 위쪽에서 기울어진 각도를 경사각으로 본다.
    /// </summary>
    /// <param name="normal">충돌면의 법선. 보통 <c>RaycastHit.normal</c>.</param>
    /// <param name="slopeLimit">걸어 오를 수 있는 최대 경사각(도). 평지는 0, 수직 벽은 90이다.</param>
    /// <returns>경사가 <paramref name="slopeLimit"/> 이하이면 true.</returns>
    /// <remarks>
    /// 천장이나 뒤집힌 면은 각도가 90도를 넘으므로 자연스럽게 false가 된다.
    /// <c>CapsuleCast</c>의 법선은 캡슐이 닿은 지점의 것이라 계단의 모서리처럼 좁은 면에서는
    /// 실제 지형과 다른 값이 나올 수 있다. 그런 경우를 처리하는 것이 턱 오르기다.
    /// </remarks>
    public static bool IsWalkable(Vector3 normal, float slopeLimit)
    {
        return Vector3.Angle(normal, Vector3.up) <= slopeLimit;
    }

    /// <summary>
    /// 수직 속도에 중력을 누적하고 최대 낙하 속도로 제한한다.
    /// 위로 향하는 속도(점프)는 제한하지 않는다.
    /// </summary>
    /// <param name="velocity">현재 수직 속도(초당). 양수가 위, 음수가 아래다.</param>
    /// <param name="gravity">중력 가속도. 아래로 당기려면 음수여야 한다(예: -9.81).</param>
    /// <param name="maxSpeed">최대 낙하 속도. 부호 없는 크기로 넘긴다(예: 50).
    /// 종단 속도 역할을 하며, 빠른 낙하에서 바닥을 뚫는 것을 막는다.</param>
    /// <param name="time">경과 시간. 보통 <see cref="Time.fixedDeltaTime"/>.</param>
    /// <returns>갱신된 수직 속도. <see cref="Fall"/>에 그대로 넘긴다.</returns>
    /// <remarks>
    /// 접지 상태에서 계속 호출하면 속도가 음수로 무한히 쌓이므로,
    /// 땅에 닿은 프레임에 호출자가 0으로 되돌려야 한다.
    /// 실무에서는 0 대신 -2 정도의 작은 음수를 유지해 경사면에서 캡슐이 들뜨는 것을 막기도 한다.
    /// </remarks>
    public static float ApplyGravity(float velocity, float gravity, float maxSpeed, float time)
    {
        return Mathf.Max(velocity + gravity * time, -maxSpeed);
    }

    /// <summary>
    /// 수직 속도만큼 이동한 목표 지점을 계산한다. 수평 성분과 충돌은 건드리지 않는다.
    /// 결과를 <see cref="Collide"/>에 넘겨 실제로 갈 수 있는 위치로 보정해야 한다.
    /// </summary>
    /// <param name="origin">현재 위치.</param>
    /// <param name="velocity">수직 속도(초당). 양수면 상승, 음수면 하강한다.
    /// 보통 <see cref="ApplyGravity"/>의 결과.</param>
    /// <param name="time">경과 시간. 보통 <see cref="Time.fixedDeltaTime"/>.</param>
    /// <returns>충돌을 반영하기 전의 목표 위치.</returns>
    public static Vector3 Fall(Vector3 origin, float velocity, float time)
    {
        return origin + Vector3.up * (velocity * time);
    }

    /// <summary>
    /// 접지 여부를 판정한다.
    /// 떨어지려 했는데 충돌 때문에 덜 내려갔다면 아래에 무언가 있다는 뜻이다.
    /// </summary>
    /// <param name="target">낙하 후 가려던 위치. <see cref="Fall"/>의 결과.</param>
    /// <param name="resolved">충돌을 반영해 실제로 도달한 위치. <see cref="Collide"/>의 결과.</param>
    /// <param name="velocity">수직 속도. 양수면(상승 중) 접지로 보지 않는다.</param>
    /// <returns>땅에 닿아 있으면 true.</returns>
    /// <remarks>
    /// 별도의 지면 검사 없이 이동 결과만으로 판정하므로 캐스팅 비용이 들지 않는다.
    /// 대신 실제로 낙하를 시도한 프레임에만 유효하다. 속도가 0이면 <paramref name="target"/>과
    /// <paramref name="resolved"/>가 같아져 false가 나오므로, 호출자는 접지 상태에서도
    /// 속도를 완전히 0으로 두지 말고 작은 음수를 유지해야 한다.
    /// </remarks>
    public static bool IsGrounded(Vector3 target, Vector3 resolved, float velocity)
    {
        return velocity <= 0.0f && resolved.y > target.y;
    }

    /// <summary>
    /// 앞을 막은 것이 넘어갈 수 있는 턱인지 판정하고, 올라선 위치를 계산한다.
    /// 위로 들어올리고 → 앞으로 가보고 → 다시 내려놓는 3단 탐색이며,
    /// 하나라도 실패하면 턱이 아니라 벽이다.
    /// </summary>
    /// <param name="position">충돌해서 멈춘 위치.</param>
    /// <param name="remain">아직 가지 못한 이동량. 수평 성분만 사용한다.</param>
    /// <param name="radius">캡슐 반지름.</param>
    /// <param name="height">캡슐 전체 높이.</param>
    /// <param name="slopeLimit">올라선 자리의 바닥이 걸을 수 있는 경사인지 판정할 기준(도).</param>
    /// <param name="stepHeight">올라설 수 있는 턱의 최대 높이.</param>
    /// <param name="layer">충돌로 판정할 레이어.</param>
    /// <param name="stepped">성공 시 올라선 위치. 실패하면 <paramref name="position"/> 그대로다.</param>
    /// <returns>턱을 올라설 수 있으면 true</returns>
    /// <remarks>
    /// 전진 거리는 <paramref name="remain"/>이 반지름보다 짧아도 반지름만큼 확보한다.
    /// 캡슐 밑면이 둥글어서 조금만 전진하면 턱 모서리에 걸친 채 미끄러져 내려오기 때문이다.
    /// 다만 실제로 옮기는 거리는 원래의 <paramref name="remain"/>만큼이며 늘린 거리는 탐색에만 쓴다. 그래서 마지막에 XZ를 원래 이동량으로 되돌린다.
    /// 천장이 낮으면 그 높이까지만 올라가 보므로 낮은 통로에서는 턱 오르기가 실패할 수 있다.
    /// </remarks>
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

        // 1. 올라설 머리 위 공간. 천장에 막히면 그만큼만 올라간다.
        float lift = stepHeight;
        if (Cast(position, Vector3.up, lift, radius, height, layer, out RaycastHit ceiling))
        {
            lift = Mathf.Max(ceiling.distance - SKIN, 0.0f);
            if (lift < MIN_DISTANCE)
            {
                return false;
            }
        }

        // 2. 올라선 높이에서 가려던 만큼 갈 수 있는가. 막히면 그냥 벽이다. 프레임당 이동량이 반지름보다 작으면 계단 위로 올라서지 못하므로 최소 전진량을 보장한다.
        float forwardDistance = Mathf.Max(distance, radius);
        Vector3 raised = position + Vector3.up * lift;
        if (Cast(raised, direction, forwardDistance, radius, height, layer, out _))
        {
            return false;
        }

        // 3. 디딜 바닥이 있는가. 없으면 턱을 넘어 허공으로 나가는 셈이라 취소한다.
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
    /// <param name="position">캡슐의 발밑 위치.</param>
    /// <param name="direction">쓸어갈 방향. 정규화되어 있어야 한다.</param>
    /// <param name="distance">검사할 거리. 실제로는 <c>SKIN</c>만큼 더 멀리 쏜다.</param>
    /// <param name="radius">캡슐 반지름.</param>
    /// <param name="height">캡슐 전체 높이. 지름보다 작으면 지름으로 보정된다.</param>
    /// <param name="layer">충돌로 판정할 레이어. 트리거는 무시된다.</param>
    /// <param name="hit">충돌 정보. 반환값이 false면 내용은 의미 없다.</param>
    /// <returns>무언가에 부딪히면 true.</returns>
    /// <remarks>
    /// 거리에 <c>SKIN</c>을 더하는 것은 벽에 완전히 밀착하는 것을 막기 위해서다.
    /// 호출자는 <c>hit.distance</c>에서 다시 <c>SKIN</c>을 빼 여유를 남긴 위치로 이동한다.
    /// 시작 지점이 이미 콜라이더 안에 있으면 <c>CapsuleCast</c>가 충돌을 놓치므로,
    /// 지형에 파묻힌 상태에서는 그대로 통과한다.
    /// </remarks>
    private static bool Cast(Vector3 position, Vector3 direction, float distance, float radius, float height, LayerMask layer, out RaycastHit hit)
    {
        GetCapsule(position, radius, height, out Vector3 bottom, out Vector3 top);
        return Physics.CapsuleCast(bottom, top, radius, direction, out hit, distance + SKIN, layer, QueryTriggerInteraction.Ignore);
    }

    /// <summary>
    /// 발밑 위치로부터 캡슐의 두 구 중심을 계산한다.
    /// <c>Physics.CapsuleCast</c>와 <c>Physics.OverlapCapsule</c>이 요구하는 형식이다.
    /// </summary>
    /// <param name="position">캡슐의 발밑 위치. 이 지점이 지면에 닿는다.</param>
    /// <param name="radius">캡슐 반지름.</param>
    /// <param name="height">캡슐 전체 높이. 지름보다 작으면 지름으로 보정된다.</param>
    /// <param name="bottom">아래쪽 구의 중심. 발밑에서 반지름만큼 위다.</param>
    /// <param name="top">위쪽 구의 중심. 정수리에서 반지름만큼 아래다.</param>
    /// <remarks>
    /// <paramref name="height"/>가 지름보다 작으면 구 두 개가 뒤집히므로 지름으로 올려 잡는다.
    /// 즉 이 캡슐이 가질 수 있는 최소 형태는 반지름이 같은 구다.
    /// </remarks>
    public static void GetCapsule(Vector3 position, float radius, float height, out Vector3 bottom, out Vector3 top)
    {
        float clamped = Mathf.Max(height, radius * 2.0f);
        bottom = position + Vector3.up * radius;
        top = position + Vector3.up * (clamped - radius);
    }
}