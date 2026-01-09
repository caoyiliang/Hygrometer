using ProtocolInterface;

namespace Hygrometer
{
    public interface IHygrometer : IProtocol
    {
        Task<Dictionary<string, string>?> Read(string addr, int tryCount = 0, CancellationToken cancelToken = default);
    }
}
