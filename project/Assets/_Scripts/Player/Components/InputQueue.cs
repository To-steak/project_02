using System.Collections.Generic;
using Project02.Core;
using Project02.Player;

namespace Project02.Server
{
    public sealed class InputQueue
    {
        public const int CAPACITY = 64;

        public readonly TickBuffer<InputPayload> Buffer = new(CAPACITY);

        public InputPayload LastInput;       // 입력이 비면 복제할 원본
        public int ConsumedTick = -1;
        public int LatestTick = -1;
        public bool Started;
        public bool CarryJump;
        public bool CarryAttack;
    }
}