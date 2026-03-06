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
        public async Task<IActionResult> PostPing(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return BadRequest("The Ip Address is required");
            try
            {
                ResponseFormat pingResult = await _pingService.SendPingAsync(ip);

                if (pingResult == null)
                {
                    return BadRequest("The ping was not succesfull");
                }

                pingResult.DeviceName = "Manual Check";

                var isSaved = await _pingrepository.SaveItemAsync(pingResult);

                if (!isSaved)
                {
                    return StatusCode(500, "Ping made but not able to saved it into AWS");
                }

                return Ok(pingResult);


            }
            catch (Exception ex) 
            {
                return BadRequest($"Error processing the ping: {ex.Message}");
            }

        }

        

        
    }
}
