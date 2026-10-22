using Project02.Player;

namespace Project02.World
{
    // 클라 입력 전송은 여기 한 곳. 틱 루프가 끝난 뒤 프레임당 1회 (ECS_RULES 규칙 4)
    // 별도 전송 큐는 두지 않는다. "보낼 입력"의 정본은 InputHistory + AckTick (규칙 10)
    public static class InputSendSystem
    {
        // 서버가 확인한 다음 틱부터 이번 프레임의 마지막 예측 틱까지 전부 보낸다 (최대 InputBundle.CAPACITY개)
        // 손실 뒤 도착한 첫 번들이 빈 구간을 한 번에 메운다
        public static void Flush(ClientWorld world)
        {
            PlayerOwnerAuthoring owner = world.Local;
            int last = world.Clock.Tick - 1;   // 예측 루프가 끝나면 Tick은 이미 1 올라가 있다
            int first = System.Math.Max(world.AckTick + 1, last - InputBundle.CAPACITY + 1);

            InputBundle bundle = default;
            for (int t = first; t <= last; t++)
            {
                if (!owner.InputHistory.TryGetValue(t, out InputPayload input)) continue;   // 재동기화로 지운 구간
                bundle.Add(input);
            }

            if (bundle.Count == 0) return;
            owner.Network.InputRPC(bundle);
        }
    }
}
