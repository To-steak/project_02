using UnityEngine;

namespace Project02.Character
{
    public static class CharacterPhysics
    {
        private const float SKIN = 0.02f; // 캐릭터과 벽과의 여유 거리
        private const float MIN_DISTANCE = 0.001f; // 1mm 미만 이동은 무시
        private const float MIN_NORMAL = 0.01f; // 수평 성분이 1% 미만이면 슬라이딩 불가
        private const float MIN_NORMAL_SQR = MIN_NORMAL * MIN_NORMAL;
        private const float MIN_INPUT = 0.01f;
        private const float MIN_INPUT_SQR = MIN_INPUT * MIN_INPUT;
        private const int MAX_SLIDE_COUNT = 3;

        public static Vector3 Walk(Vector3 origin, Vector3 target, float radius, float height, float slopeLimit, float stepHeight, LayerMask layer, bool grounded)
        {
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

        public static Vector3 MoveVertical(Vector3 source, Vector3 target, float radius, float height, float slopeLimit, LayerMask layer, out bool landed)
        {
            landed = false;

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
                if (IsWalkable(hit.normal, slopeLimit))
                {
                    landed = direction.y < 0.0f;
                    break;
                }

                delta = Vector3.ProjectOnPlane(remain, hit.normal);
            }

            return position;
        }

        public static float ApplyGravity(float velocity, float gravity, float maxSpeed, float time)
        {
            return Mathf.Max(velocity - gravity * time, -maxSpeed);
        }

        public static bool IsWalkable(Vector3 normal, float slopeLimit)
        {
            return Vector3.Angle(normal, Vector3.up) <= slopeLimit;
        }

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

        private static bool Cast(Vector3 position, Vector3 direction, float distance, float radius, float height, LayerMask layer, out RaycastHit hit)
        {
            GetCapsule(position, radius, height, out Vector3 bottom, out Vector3 top);
            return Physics.CapsuleCast(bottom, top, radius, direction, out hit, distance + SKIN, layer, QueryTriggerInteraction.Ignore);
        }

        public static void GetCapsule(Vector3 position, float radius, float height, out Vector3 bottom, out Vector3 top)
        {
            float clamped = Mathf.Max(height, radius * 2.0f);
            bottom = position + Vector3.up * radius;
            top = position + Vector3.up * (clamped - radius);
        }

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
}