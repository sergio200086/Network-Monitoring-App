using RouteMonitoring.Domain;
using RouteMonitoring.Domain.Interfaces;
using System.Net.NetworkInformation;

namespace RouteMonitoring.Infrastructure.Repositories
{
    public class PingRepository : IPingService
    {
        public async Task<ResponseFormat> SendPingAsync (string ip)
        {
            using Ping pingSender = new();
            string host = ip;
            int timeout = 2000;
            PingReply pingReply = await pingSender.SendPingAsync(host, timeout);

            if (pingReply.Status != IPStatus.Success)
                return null;

            DateTime pingMoment = DateTime.UtcNow;

            string ISODate = pingMoment.ToString("yyyy-MM-ddTHH:mm:ss");

            return new ResponseFormat
            {
                Status = pingReply.Status.ToString(),
                ResponseTimeMs = pingReply.RoundtripTime,
                TimeStamp = ISODate,
                sk = $"PING#{ISODate}"
            };
        }
    }
}
