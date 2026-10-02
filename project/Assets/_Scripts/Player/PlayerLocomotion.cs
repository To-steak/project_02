using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    public CharacterState State => _state;
    public float Mp => _mp;
    public bool IsRunning { get; private set; }

    private CharacterState _state;
    private float _mp;
    private bool _exhausted;

    public void Rotate(float yaw)
    {
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    public void ResetMp(float maxMp)
    {
        _mp = maxMp;
        _exhausted = false;
    }

    public StatePayload Capture(int tick) => new StatePayload
    {
        Tick = tick,
        Position = transform.position,
        VerticalSpeed = _state.VerticalSpeed,
        IsGrounded = _state.IsGrounded,
        Mp = _mp,
        IsExhausted = _exhausted
    };

    public void RollbackState(StatePayload payload)
    {
        transform.position = payload.Position;
        _state.VerticalSpeed = payload.VerticalSpeed;
        _state.IsGrounded = payload.IsGrounded;
        _mp = payload.Mp;
        _exhausted = payload.IsExhausted;
    }

    public void Simulate(InputPayload payload, PlayerSettings setting, float maxMp)
    {
        float time = Time.fixedDeltaTime;
        Vector2 move = Vector2.ClampMagnitude(payload.MoveInput, 1.0f);
        bool moving = move != Vector2.zero;

        _mp = Mathf.Min(_mp, maxMp);
        if (_exhausted && _mp >= maxMp * setting.RecoveryRatio)
        {
            _exhausted = false;
        }

        bool jumping = payload.JumpInput && _state.IsGrounded && _mp >= setting.JumpCost;
        if (jumping)
        {
            _mp -= setting.JumpCost;
        }

        bool running = moving && payload.RunInput && !_exhausted && _mp > 0.0f;
        if (running)
        {
            _mp = Mathf.Max(0.0f, _mp - setting.RunCost * time);
            if (_mp <= 0.0f)
            {
                _exhausted = true;
            }
        }
        else
        {
            _mp = Mathf.Min(maxMp, _mp + setting.RegenMp * time);
        }

        IsRunning = running;

        float moveSpeed = !moving ? 0.0f : (running ? setting.RunSpeed : setting.WalkSpeed);
        float jumpSpeed = jumping ? setting.JumpSpeed : 0.0f;
        Vector3 direction = Quaternion.Euler(0.0f, payload.YawInput, 0.0f) * new Vector3(move.x, 0.0f, move.y);
        _state.Position = transform.position;
        _state = CharacterMotor.Step(_state, moveSpeed * time * direction, jumpSpeed, setting.Profile, time);
        transform.position = _state.Position;
    }
}