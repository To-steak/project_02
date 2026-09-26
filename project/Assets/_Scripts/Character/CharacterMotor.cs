using UnityEngine;

public static class CharacterMotor
{
    private const float MIN_MOVE = 0.001f;
    private const float MIN_MOVE_SQR = MIN_MOVE * MIN_MOVE;
    private const float CEILING_EPSILON = 0.001f;

    public static CharacterState Step(CharacterState state, Vector3 move, float jumpSpeed, AgentProfile profile, float time)
    {
        LayerMask layer = profile.GroundLayer | profile.ObstacleLayer;
        Vector3 position = state.Position;

        move.y = 0.0f;
        if (move.sqrMagnitude > MIN_MOVE_SQR)
        {
            position = CharacterPhysics.Walk(position, position + move, profile.Radius, profile.Height, profile.SlopeLimit, profile.StepHeight, layer, state.IsGrounded, out _);
        }

        float vertical = state.VerticalSpeed;
        if (jumpSpeed > 0.0f && state.IsGrounded)
        {
            vertical = jumpSpeed;
        }

        vertical = CharacterPhysics.ApplyGravity(vertical, profile.Gravity, profile.MaxFallSpeed, time);
        Vector3 target = position + Vector3.up * vertical * time;
        position = CharacterPhysics.Collide(position, target, profile.Radius, profile.Height, layer);
        if (vertical > 0.0f && position.y < target.y - CEILING_EPSILON)
        {
            vertical = 0.0f;
        }

        bool isGround = CharacterPhysics.IsGrounded(target, position, vertical);
        if (isGround)
        {
            vertical = -profile.GroundStickSpeed;
        }

        return new CharacterState
        {
            Position = position,
            VerticalSpeed = vertical,
            IsGrounded = isGround
        };
    }
}
