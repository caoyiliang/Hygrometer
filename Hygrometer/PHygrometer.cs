namespace Hygrometer
{
    internal class PHygrometer
    {
        [CHProtocol(71)]
        public float 湿度 { get; set; }

        [CHProtocol(99)]
        public float 量程 { get; set; }

        [CHProtocol(101)]
        public ushort 不要 { get; set; }

        [CHProtocol(140)]
        public ushort 状态 { get; set; }
    }
}
