using Microsoft.VisualBasic;
using RouteMonitoring.Domain.Interfaces;

namespace Route_Monitoring.Services
{
    public class MonitoringWorkerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private const int intervalMinutes = 5;

        public MonitoringWorkerService(IServiceProvider serviceProvider, ILogger logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested) 
            {
                _logger.LogInformation("Initializing pings");
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var deviceRepo = scope.ServiceProvider.GetRequiredService<IPingRepository>();
                        var pingService = scope.ServiceProvider.GetRequiredService<IPingService>();

                        var devices = await deviceRepo.GetAllDevicesAsync();

                        foreach (var device in devices)
                        {
                            var pingResponse = await pingService.SendPingAsync(device.IpAddress);

                            if (pingResponse != null)
                            {
                                device.Status = pingResponse.Status.ToString();
                                device.ResponseTimeMs = pingResponse.RoundtripTime;
                                device.TimeStamp = DateTime.UtcNow;
                                device.sk = $"PING#{device.TimeStamp:yyyy-MM-ddTHH:mm:ss}";

                                await deviceRepo.SaveItemAsync(device);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error in the monitoring loop.");
                }
                await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
            }
        }
    }
}
