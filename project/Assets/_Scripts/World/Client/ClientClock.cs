namespace Project02.World
{
    public struct ClientClock
    {
        public int Tick;
        public float Accumulator;
        public float Interval;
        public float SmoothedBuffer;
    }
}