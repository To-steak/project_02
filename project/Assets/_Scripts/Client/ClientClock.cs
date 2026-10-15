namespace Project02.Client
{
    public struct ClientClock
    {
        public int Tick;
        public float Accumulator;
        public float Interval;
        public float SmoothedBuffer;
    }
}