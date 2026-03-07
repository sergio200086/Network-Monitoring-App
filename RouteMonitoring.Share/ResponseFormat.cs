using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace RouteMonitoring.Domain
{
    public class ResponseFormat
    {
        public ResponseFormat() => Id = Guid.NewGuid().ToString();

        [JsonPropertyName("pk")]
        public string pk => $"DEVICE#{Id}";

        [JsonPropertyName("sk")]
        public string?  sk { get; set; }

        [JsonPropertyName("Id")]
        public string Id { get; set; }

        [JsonPropertyName("DeviceName")]
        public string? DeviceName { get; set; }

        [JsonPropertyName("IpAddress")]
        public string IpAddress { get; set; }

        [JsonPropertyName("Status")]
        public string? Status { get; set; }

        [JsonPropertyName("ResponseTimeMs")]
        public long ResponseTimeMs { get; set; }

        [JsonPropertyName("TimeStamp")]
        public string? TimeStamp { get; set; }
    }
}
