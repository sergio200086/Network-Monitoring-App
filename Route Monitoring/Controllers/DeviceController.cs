using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RouteMonitoring.Domain;
using RouteMonitoring.Domain.Interfaces;

namespace Route_Monitoring.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DeviceController : ControllerBase
    {
        private readonly IPingRepository _pingrepository;
        private readonly IPingService _pingService;

        public DeviceController(IPingRepository pingRepository, IPingService pingservice)
        {
            _pingrepository = pingRepository;
            _pingService = pingservice;
        }

        [HttpGet("all-devices")]
        public async Task<IActionResult> GetAllDevices()
        {
            var allDevices = await _pingrepository.GetAllDevicesAsync();
            if(allDevices.Count == 0)
            {
                return NotFound("No device was found");
            }

            return Ok(allDevices);
        }

        [HttpPost("new-device")]
        public async Task<IActionResult> AddDevice([FromBody] ResponseFormat deviceInfo)
        {
            if (deviceInfo == null)
                return BadRequest("The JSON is empty");
            if (string.IsNullOrEmpty(deviceInfo.DeviceName))
                return BadRequest("The Ip address is required");

            try
            {
                deviceInfo.sk = "METADATA";
                deviceInfo.TimeStamp = DateTime.Now;

                var isTrue = await _pingrepository.SaveItemAsync(deviceInfo);
                if (!isTrue)
                    return StatusCode(500, "No se pudo guardar el equipo en la base de datos.");

                return Ok(deviceInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
            
        }

    }
}
