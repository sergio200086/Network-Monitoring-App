using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RouteMonitoring.Domain;
using RouteMonitoring.Domain.Interfaces;
using RouteMonitoring.Domain.Settings;

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

        public async Task<bool> SavePingAsync(ResponseFormat ping)
        {
            try
            {
                var pingAsJson = JsonConvert.SerializeObject(ping);
                var itemAsDocument = Document.FromJson(pingAsJson);
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
    }
}
