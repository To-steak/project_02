using Project02.Character;
using UnityEngine;

namespace Project02.Player
{
    public static class PlayerMovement
    {
        public static void Step(SOMovement settings, ref MovementState movement, ref StaminaState stamina, in InputPayload input, float maxEnergy, float deltaTime, out bool isRunning)
        {
            Vector2 move = Vector2.ClampMagnitude(input.MoveInput, 1.0f);
            bool moving = move != Vector2.zero;

            stamina.Energy = Mathf.Min(stamina.Energy, maxEnergy);
            if (stamina.IsExhausted && stamina.Energy >= maxEnergy * settings.RecoveryRatio)
            {
                stamina.IsExhausted = false;
            }

            bool jumping = input.JumpInput && movement.IsGrounded && stamina.Energy >= settings.JumpCost;
            if (jumping)
            {
                stamina.Energy -= settings.JumpCost;
            }

            bool running = moving && input.RunInput && !stamina.IsExhausted && stamina.Energy > 0.0f;
            if (running)
            {
                stamina.Energy = Mathf.Max(0.0f, stamina.Energy - settings.RunCost * deltaTime);
                if (stamina.Energy <= 0.0f)
                {
                    stamina.IsExhausted = true;
                }
            }
            else
            {
                stamina.Energy = Mathf.Min(maxEnergy, stamina.Energy + settings.RegenEnergy * deltaTime);
            }

            isRunning = running; // TODO: 결과 구조체를 만들어 반환

            float moveSpeed = !moving ? 0.0f : (running ? settings.RunSpeed : settings.WalkSpeed);
            float jumpSpeed = jumping ? settings.JumpSpeed : 0.0f;
            Vector3 direction = Quaternion.Euler(0.0f, input.YawInput, 0.0f) * new Vector3(move.x, 0.0f, move.y);

            movement = CharacterMotor.Step(movement, moveSpeed * deltaTime * direction, jumpSpeed, settings.Profile, deltaTime);
        }
    }
}