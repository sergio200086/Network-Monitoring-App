using RouteMonitoring.Domain;
using RouteMonitoring.Domain.Interfaces;
using System.Net.NetworkInformation;

namespace RouteMonitoring.Infrastructure.Repositories
{
    public class PingRepository : IPingService
    {
        public PingRepository() { }

        public async Task<PingReply?> SendPingAsync (string ip)
        {
            using Ping pingSender = new();
            string host = ip;
            int timeout = 2000;
            PingReply pingReply = await pingSender.SendPingAsync(host, timeout);

            if (pingReply.Status != IPStatus.Success)
                return null;

            return pingReply;
        }
    }
}
