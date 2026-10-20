using UnityEngine;

namespace Project02.Core
{
    public sealed class TickBuffer<T> where T : struct
    {
        public int Capacity => _ticks.Length;

        private readonly T[] _values;
        private readonly int[] _ticks;
        private readonly int _mask;

        public TickBuffer(int size)
        {
            _values = new T[size];
            _ticks = new int[size];
            _mask = size - 1;

            Debug.Assert(size > 0 && (size & (size - 1)) == 0, "TickBuffer 크기는 2의 거듭제곱이어야 함");
            Clear();
        }

        public void Clear()
        {
            for (int i = 0; i < _ticks.Length; i++)
            {
                _ticks[i] = -1;
            }
        }

        public void SetValue(int tick, in T value)
        {
            int index = tick & _mask;
            if (_ticks[index] > tick) return;

            _values[index] = value;
            _ticks[index] = tick;
        }

        public bool TryGetValue(int tick, out T value)
        {
            int index = tick & _mask;
            if (_ticks[index] != tick)
            {
                value = default;
                return false;
            }

            value = _values[index];
            return true;
        }
    }
}