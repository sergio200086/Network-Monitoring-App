using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace RouteMonitoring.Domain.Interfaces
{
    public interface IPingService
    {
        Task<PingReply?> SendPingAsync(string ip);
    }
}
