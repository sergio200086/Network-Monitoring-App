using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RouteMonitoring.Domain;
using RouteMonitoring.Domain.Interfaces;
using RouteMonitoring.Domain.Settings;
using Amazon.Runtime.Internal.Util;

namespace RouteMonitoring.Infrastructure.Repositories
{
    public class DynamoDbPingrepository : IPingRepository
    {
        private readonly IAmazonDynamoDB _dynamoDB;
        private readonly IOptions<DatabaseSettings> _databaseSettings;

        public DynamoDbPingrepository(IAmazonDynamoDB dynamoDb, IOptions<DatabaseSettings> databaseSettings)
        {
            _dynamoDB = dynamoDb;
            _databaseSettings = databaseSettings;
        }

        public async Task<bool> SaveItemAsync(ResponseFormat item)
        {
            try
            {
                var itemAsJson = JsonConvert.SerializeObject(item);
                var itemAsDocument = Document.FromJson(itemAsJson);
                var itemAsAttributeMap = itemAsDocument.ToAttributeMap();

                var request = new PutItemRequest
                {
                    TableName = _databaseSettings.Value.TableName.ToString(),
                    Item = itemAsAttributeMap
                };

                var response = await _dynamoDB.PutItemAsync(request);

                return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<ResponseFormat?> GetAsync(Guid id)
        {
            var request = new GetItemRequest
            {
                TableName = _databaseSettings.Value.TableName.ToString(),
                //Key is pk+sk
                Key = new Dictionary<string, AttributeValue>
                { 
                    {"pk", new AttributeValue {S = "DEVICE#" + id.ToString()} },
                    {"sk", new AttributeValue { S = "METADATA" } },

                }
            };

            GetItemResponse response = await _dynamoDB.GetItemAsync(request);
            if (response.Item == null)
                return null;

            if (response.Item.Count == 0)
                return null;

            Document itemAsDocument = Document.FromAttributeMap(response.Item);
            return JsonConvert.DeserializeObject<ResponseFormat>(itemAsDocument.ToJson());
        }

        public async Task<List<ResponseFormat>> GetAllDevicesAsync()
        {
            var devices = new List<ResponseFormat>();
            var request = new ScanRequest
            {
                TableName = _databaseSettings.Value.TableName.ToString(),
                FilterExpression = "sk = :metadataValue",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":metadataValue", new AttributeValue { S = "METADATA" } }
                }
            };

            var response = await _dynamoDB.ScanAsync(request);
            foreach(var item in response.Items)
            {
                devices.Add(new ResponseFormat
                {
                    Id = item["Id"].S,
                    DeviceName = item.TryGetValue("DeviceName", out AttributeValue? value) ? value.S : "Unknown",
                    IpAddress = item["IpAddress"].S,
                });
            }

            return devices;

        }


        /// <summary>
        /// Get the pings under the provided date.
        /// </summary>
        /// <param name="id">The ID of the registered device. </param>
        /// <param name="date">The date and hour of the desired range.</param>
        /// <returns>A List <see cref="ResponseFormat"/> containing the pings filtered.</returns>
        public async Task<List<ResponseFormat>> GetPingByDate(string id, string date)
        {
            var pings = new List<ResponseFormat>();
            var request = new ScanRequest
            {
                TableName = _databaseSettings.Value.TableName.ToString(),
                FilterExpression = "pk = :id AND sk > :date",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    {":id", new AttributeValue {S = "DEVICE#" + id } },
                    {":date", new AttributeValue { S = "PING#" + date } }
                }
            };

            var response = await _dynamoDB.ScanAsync(request);
            foreach (var item in response.Items)
            {
                pings.Add(new ResponseFormat
                {
                    Id = item["Id"].S,
                    sk = item["sk"].S,
                    DeviceName = item.TryGetValue("DeviceName", out AttributeValue? value) ? value.S : "Unknown",
                    IpAddress = item["IpAddress"].S,
                    ResponseTimeMs = item.TryGetValue("ResponseTimeMs", out AttributeValue? responseTimeValue) && long.TryParse(responseTimeValue.N, out long responseTime) ? responseTime : 0,
                    Status = item.TryGetValue("Status", out AttributeValue? statusValue) ? statusValue.S : "Unknown",
                    TimeStamp = item.TryGetValue("TimeStamp", out AttributeValue? TimeStampValue) ? TimeStampValue.S : "Unknown"
                });
            }

            return pings;
        }
    }
}
