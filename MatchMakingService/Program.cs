using RabbitMQ.Client;
using MatchMakingService.Hubs;
using MatchMakingService.Domain.Interfaces;
using MatchMakingService.BackgroundServices;
using MatchMakingService.Application.Services;
using MatchMakingService.Application.Interfaces;
using MatchMakingService.Infrastructure.Caching;
using MatchMakingService.Infrastructure.Data.Settings;
using MatchMakingService.Infrastructure.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var redisConnection = builder.Configuration.GetConnectionString("RedisConnection") 
                      ?? throw new InvalidOperationException("Redis Connection Not Configured");
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnection;
    options.InstanceName = "MatchMakingService_";
});
builder.Services.AddSignalR().AddStackExchangeRedis(redisConnection);
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection(nameof(MongoDbSettings)));
builder.Services.AddSingleton<ILobbyCache, LobbyCache>();
builder.Services.AddSingleton<ILobbyRepository, MongoDbRepository>();
builder.Services.AddSingleton<ILobbyCodePool, RedisLobbyCodePool>();
builder.Services.AddScoped<ILobbyService, LobbyService>();
builder.Services.AddScoped<ILobbyNotifier, LobbyNotifier>();
builder.Services.AddHostedService<LobbyCodeRecyclingServices>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHub<LobbyHub>("/lobbyhub");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();