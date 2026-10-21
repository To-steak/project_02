using System.Collections.Generic;
using Project02.Core;

namespace Project02.Player
{
    public sealed class InputQueue
    {
        public const int CAPACITY = 64;

        public readonly TickBuffer<InputPayload> Buffer = new(CAPACITY);

        public InputPayload LastInput;       // 입력이 비면 복제할 원본
        public int ConsumedTick = -1;
        public int LatestTick = -1;
        public int AckTick = -1;             // 이 틱까지의 진짜 입력을 빠짐없이 받았다 (복제한 틱은 포함하지 않음). StatePayload로 소유자에게 알린다
        public bool Started;
        public bool CarryJump;
        public bool CarryAttack;
    }
}