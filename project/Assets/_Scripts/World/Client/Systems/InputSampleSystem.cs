using UnityEngine;

namespace Project02.Client
{
    public static class InputSampleSystem
    {
        public static void Sample(ref InputState input, PlayerAction actions)
        {
            PlayerAction.GeneralActions general = actions.General;

            input.Move = general.Move.ReadValue<Vector2>();
            input.Look = general.Look.ReadValue<Vector2>();
            input.Run = general.Run.IsPressed();
            input.Aim = general.Aim.IsPressed();
            input.Attack = general.Attack.IsPressed();

            // |= : 이미 켜진 래치는 틱이 소비할 때까지 유지
            input.JumpPressed |= general.Jump.WasPerformedThisFrame();
            input.AttackPressed |= general.Attack.WasPerformedThisFrame();
        }
    }
}