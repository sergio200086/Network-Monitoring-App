using Amazon.DynamoDBv2;
using RouteMonitoring.Domain.Settings;
using RouteMonitoring.Domain.Interfaces;
using RouteMonitoring.Infrastructure.Repositories;
using Route_Monitoring.Services;


var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.Configure<DatabaseSettings>(config.GetSection(DatabaseSettings.KeyName));
builder.Services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(Amazon.RegionEndpoint.USEast1));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IPingRepository, DynamoDbPingrepository>();
builder.Services.AddScoped<IPingService, PingRepository>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddHostedService<MonitoringWorkerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
