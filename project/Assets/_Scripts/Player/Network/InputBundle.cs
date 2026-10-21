// 저장 방식: FixedList4096Bytes<InputPayload> (값 타입, 관리 힙 할당 없음). NGO 2.13.1 RPC 인자로 한 번만 복사되고, 처리기가 payload로 풀어 수신함에 담는다 (번들 자체는 쌓지 않음).
//   InputPayload가 메모리 44바이트라 FixedList512Bytes는 11개뿐 → CAPACITY(16)를 담는 가장 작은 크기가 4096 (Collections에 1024는 없음).
//   메모리는 번들당 약 4KB지만 전송은 Count + payload Count개만 직접 쓴다 (가변 길이, 최대 약 660바이트).
//   NativeArray 인자는 Allocator.Temp라 RPC 처리기 밖(ServerPlayerSystem)에서 쓸 수 없고, 관리 배열은 수신마다 할당하므로 고르지 않았다.
using Unity.Collections;
using Unity.Netcode;

namespace Project02.Player
{
    public struct InputBundle : INetworkSerializable
    {
        // 입력 윈도우 상한 16틱(320ms). 메시지 크기는 메시지가 정하고 전송 쪽(PredictSystem)이 따른다
        // InputPayload 직렬화 41바이트(Tick 4 + ViewTick 4 + Move 8 + Run/Jump 2 + Yaw/Pitch 8 + Attack/Pressed/Aim 3 + AimPoint 12)
        // × 16 + Count 1 ≈ 660바이트 → SnapshotSystem.MAX_MESSAGE(1100) 아래, 비분할 전송 한도 안
        public const int CAPACITY = 16;

        private FixedList4096Bytes<InputPayload> _inputs;

        public readonly int Count => _inputs.Length;
        public readonly InputPayload this[int index] => _inputs[index];

        public void Add(in InputPayload payload)
        {
            if (_inputs.Length >= CAPACITY) return;
            _inputs.Add(payload);
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            byte count = (byte)_inputs.Length;
            serializer.SerializeValue(ref count);

            if (serializer.IsReader)
            {
                _inputs.Clear();
                if (count > CAPACITY) return;   // 깨진 패킷: 아무것도 읽지 않는다

                for (int i = 0; i < count; i++)
                {
                    InputPayload payload = default;
                    serializer.SerializeValue(ref payload);
                    _inputs.Add(payload);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    InputPayload payload = _inputs[i];
                    serializer.SerializeValue(ref payload);
                }
            }
        }
    }
}