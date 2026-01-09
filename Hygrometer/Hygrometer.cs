using Communication;
using Communication.Bus.PhysicalPort;
using Communication.Exceptions;
using LogInterface;
using Modbus;
using Modbus.Parameter;
using TopPortLib.Interfaces;
using Utils;

namespace Hygrometer
{
    public class Hygrometer : IHygrometer
    {
        private static readonly ILogger _logger = Logs.LogFactory.GetLogger<Hygrometer>();
        private readonly IModBusMaster _modBusMaster;
        private bool _isConnect = false;
        public bool IsConnect => _isConnect;

        /// <inheritdoc/>
        public event DisconnectEventHandler? OnDisconnect { add => _modBusMaster.OnDisconnect += value; remove => _modBusMaster.OnDisconnect -= value; }
        /// <inheritdoc/>
        public event ConnectEventHandler? OnConnect { add => _modBusMaster.OnConnect += value; remove => _modBusMaster.OnConnect -= value; }

        public Hygrometer(SerialPort serialPort, int defaultTimeout = 5000)
        {
            _modBusMaster = new ModBusMaster(serialPort, ModbusType.RTU, defaultTimeout)
            {
                IsHighByteBefore_Req = true,
                IsHighByteBefore_Rsp = false
            };
            _modBusMaster.OnSentData += CrowPort_OnSentData;
            _modBusMaster.OnReceivedData += CrowPort_OnReceivedData;
            _modBusMaster.OnConnect += CrowPort_OnConnect;
            _modBusMaster.OnDisconnect += CrowPort_OnDisconnect;
        }

        public Hygrometer(ICrowPort crowPort)
        {
            _modBusMaster = new ModBusMaster(crowPort)
            {
                IsHighByteBefore_Req = true,
                IsHighByteBefore_Rsp = false
            };
            _modBusMaster.OnConnect += CrowPort_OnConnect;
            _modBusMaster.OnDisconnect += CrowPort_OnDisconnect;
        }

        private async Task CrowPort_OnDisconnect()
        {
            _isConnect = false;
            await Task.CompletedTask;
        }

        private async Task CrowPort_OnConnect()
        {
            _isConnect = true;
            await Task.CompletedTask;
        }

        private async Task CrowPort_OnReceivedData(byte[] data)
        {
            _logger.Trace($"Hygrometer Rec:<-- {StringByteUtils.BytesToString(data)}");
            await Task.CompletedTask;
        }

        private async Task CrowPort_OnSentData(byte[] data)
        {
            _logger.Trace($"Hygrometer Send:--> {StringByteUtils.BytesToString(data)}");
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task OpenAsync()
        {
            _isConnect = _modBusMaster.IsConnect;
            return _modBusMaster.OpenAsync();
        }

        /// <inheritdoc/>
        public async Task CloseAsync(bool closePhysicalPort)
        {
            if (closePhysicalPort) await _modBusMaster.CloseAsync(closePhysicalPort);
        }

        public async Task<Dictionary<string, string>?> Read(string addr, int tryCount = 0, CancellationToken cancelToken = default)
        {
            if (!_isConnect) throw new NotConnectedException();
            var b = new BlockList();
            b.Add(new PHygrometer());
            Func<Task<PHygrometer>> func = () => _modBusMaster.GetAsync<PHygrometer>(addr, b);
            var rs = await func.ReTry(tryCount, cancelToken);
            return rs == null ? null : new Dictionary<string, string>()
            {
                { "71", rs.湿度.ToString() },
                { "状态", rs.状态 switch
                {
                    0 => "N",
                    1 => "C",
                    2 => "C",
                    3 => "D",
                    7 => "P",
                    _ => "N"
                }},
                { "S05-FullRange", rs.量程.ToString() }
            };
        }
    }
}
