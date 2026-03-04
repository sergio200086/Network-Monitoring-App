using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Mvc;
using RouteMonitoring.Domain;
using RouteMonitoring.Domain.Interfaces;



namespace Route_Monitoring.Controllers
{
    [ApiController]
    [Route("app/[controller]")]
    public class PingController : ControllerBase
    {
        private readonly IPingRepository _pingrepository;
        private readonly IPingService _pingService;
        public PingController(IPingRepository pingRepository, IPingService pingservice)
        {
            _pingrepository = pingRepository;
            _pingService = pingservice;
        }

        [HttpPost("check-status/{ip}")]
        //Request ping to any ip direction
        public async Task<IActionResult> PostPing(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return BadRequest("The Ip Address is required");
            try
            {
                var pingResult = await _pingService.SendPingAsync(ip);

                if (pingResult == null)
                {
                    return BadRequest("The ping was not succesfull");
                }

                var responseFormat = new ResponseFormat
                {
                    IpAddress = ip,
                    DeviceName = "Manual Check",
                    Status = pingResult.Status.ToString(),
                    ResponseTimeMs = pingResult.RoundtripTime,
                    TimeStamp = DateTime.UtcNow,
                    Id = "MANUAL-CHECK",
                    sk = $"PING#{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss}"
                };

                var isSaved = await _pingrepository.SaveItemAsync(responseFormat);

                if (!isSaved)
                {
                    return StatusCode(500, "Ping made but not able to saved it into AWS");
                }

                return Ok(responseFormat);


            }
            catch (Exception ex) 
            {
                return BadRequest($"Error processing the ping: {ex.Message}");
            }

        }

        public async Task<IActionResult> GetPing(Guid id)
        {
            try
            {
                var isGotten = await _pingrepository.GetAsync(id);
                if (isGotten!= null)
                {
                    return Ok(isGotten);
                }
                return BadRequest();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
