using UnityEngine;

namespace Project02.World
{
    public struct InputState
    {
        public Vector2 Move;
        public Vector2 Look;
        public bool Run;
        public bool Aim;
        public bool Attack;

        // 눌림 래치: 프레임에서 켜지고, PredictSystem이 틱에 실어 보낸 뒤 끈다
        // (틱이 없는 프레임에 눌러도 다음 틱까지 살아남는다)
        public bool JumpPressed;
        public bool AttackPressed;
    }
}