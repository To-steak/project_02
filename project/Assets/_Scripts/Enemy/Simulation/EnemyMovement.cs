using Project02.Character;
using UnityEngine;

namespace Project02.Enemy
{
    public static class EnemyMovement
    {
        public static void Step(SOEnemy settings, ref MovementState movement, ref Quaternion rotation, Vector2 input, float speed, float deltaTime)
        {
            Vector3 move = speed * deltaTime * new Vector3(input.x, 0.0f, input.y);
            movement = CharacterMotor.Step(movement, move, 0.0f, settings.Profile, deltaTime);
            rotation = CharacterPhysics.Rotate(rotation, input, settings.RotationSpeed, deltaTime);
        }
    }
}