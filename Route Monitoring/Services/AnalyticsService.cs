using Amazon.DynamoDBv2;
using RouteMonitoring.Domain;
using RouteMonitoring.Domain.Interfaces;

namespace Route_Monitoring.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        public AnalyticsResponse CalculateNetworkParams(List<ResponseFormat> pings)
        {
            if (pings == null || pings.Count == 0)
                return new AnalyticsResponse();

            var total = pings.Count;
            var successfulPings = pings.Where(p => p.Status == "Success").ToList();
            var failedPings = total - successfulPings.Count();

            return new AnalyticsResponse
            {
                PingsSuccessful = successfulPings.Count(),
                PingsFailed = failedPings,
                NumberOfPings = total,
                AvgResponseTime = successfulPings.Count() > 0 ?  successfulPings.Average(p => p.ResponseTimeMs) : 0,
                PercentPacketLoss = (double)failedPings / total * 100,
                Jitter = CalculateJitter([.. successfulPings.Select(p => p.ResponseTimeMs)]),

                MyProperty = pings
            };
        }

        public double CalculateJitter(List<long> responseTimes)
        {
            if (responseTimes == null || responseTimes.Count < 2)
                return 0;

            var sum = 0.0;

            for (int i = 1; i <responseTimes.Count; i++)
            { 
                sum += Math.Abs(responseTimes[i] - responseTimes[i - 1]);
            }

             return sum / (responseTimes.Count - 1);
        }
    }
}
