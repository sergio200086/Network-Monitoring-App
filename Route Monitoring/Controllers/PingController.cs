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

        public PingController(IPingRepository pingRepository)
        {
            _pingrepository = pingRepository;
        }

        [HttpPost("check-status/{ip}")]
        //Request ping to any ip direction
        public async Task<IActionResult> PostPing(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return BadRequest("The Ip Address is required");
            try
            {
                using Ping pingSender = new Ping();
                string host = ip;
                int timeout = 2000;
                PingReply pingReply = await pingSender.SendPingAsync(host, timeout);

                if (pingReply.Status != IPStatus.Success)
                    throw new Exception("fake");
                
                ResponseFormat responseFormat = new ResponseFormat();
                responseFormat.DeviceName = "Unknown";
                responseFormat.Status = pingReply.Status.ToString();
                responseFormat.IpAddress = ip;
                responseFormat.ResponseTimeMs = pingReply.RoundtripTime;
                responseFormat.TimeStamp = DateTime.UtcNow;

                var isSaved = await _pingrepository.SavePingAsync(responseFormat);

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
    }
}
