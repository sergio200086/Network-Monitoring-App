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

        /// <summary>
        /// Retrieves pings for a specified device on a given date.
        /// </summary>
        /// <param name="id">The unique identifier of the device.</param>
        /// <param name="date">The date for which to retrieve pings, in a supported date format.</param>
        /// <returns>An HTTP 200 response with the list of pings if found; otherwise, an HTTP 404 response.</returns>
        
        [HttpGet("get-pings-by-date/{date}/{id}")]
        public async Task<IActionResult> GetPingByDate (string id, string date)
        {
            var filteredPings = await _pingrepository.GetPingByDate(id, date);

            if (filteredPings == null || filteredPings.Count == 0)
                return NotFound("No pings found for the specified date and device.");

            return Ok(filteredPings);
        }
    }
}
