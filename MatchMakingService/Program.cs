using MatchMakingService.Hubs;
using MatchMakingService.Domain.Interfaces;
using MatchMakingService.Application.Services;
using MatchMakingService.Application.Interfaces;
using MatchMakingService.Infrastructure.Data.Settings;
using MatchMakingService.Infrastructure.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddSignalR().AddStackExchangeRedis(builder.Configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("Redis Connection Not Configured"));
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection(nameof(MongoDbSettings)));

builder.Services.AddSingleton<ILobbyRepository, MongoDbRepository>();
builder.Services.AddScoped<ILobbyService, LobbyService>();
builder.Services.AddScoped<ILobbyNotifier, LobbyNotifier>();

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