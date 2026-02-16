# MatchmakingService

A high-performance, scalable custom matchmaking service built with ASP.NET Core and SignalR, designed for real-time connection management and lobby orchestration. It utilizes MongoDB for persistent storage and Redis for real-time messaging distribution, distributed caching, and efficient unique code generation.

## Features

*   **Real-time Lobby Management:** Create, join, and manage game lobbies using SignalR's persistent WebSocket connections.
*   **Distributed Scalability:** Seamlessly scale across multiple server instances using a Redis backplane for SignalR message synchronization.
*   **Persistent Lobby State:** Store lobby and player data in MongoDB.
*   **High-Performance Lobby Code Generation:** Utilizes a Redis-backed pre-shuffled queue for atomic, fast, and unique lobby code generation (e.g., `ADJECTIVE-NOUN-NUMBER` format).
*   **Automated Lobby Code Recycling:** Background service for safely recycling unused lobby codes back into the available pool.
*   **Distributed Caching:** Leverages Redis `IDistributedCache` for fast retrieval of lobby data, reducing database load.
*   **Clean Architecture:** Structured with clear separation of concerns (Presentation, Application, Domain, Infrastructure layers) for maintainability and testability.

## Technology Stack

*   **.NET SDK:** .NET 8 (or later)
*   **Web Framework:** ASP.NET Core
*   **Real-time Communication:** SignalR
*   **Database:** MongoDB
*   **Caching & Messaging:** Redis
*   **Logging:** Microsoft.Extensions.Logging

## Architecture Overview

The project is structured following principles inspired by Clean Architecture, ensuring a clear separation of concerns and maintainability.

*   **`MatchmakingService` (Presentation Layer):**
    *   The main ASP.NET Core Web API project.
    *   Handles external client interactions (SignalR Hubs).
    *   Acts as the Composition Root, wiring up dependencies.
    *   Contains `LobbyHub` for client-server real-time communication.
    *   Contains `SignalRClientService` to implement abstract SignalR sending for `LobbyNotifier`.
    *   Hosts `LobbyCodeRecyclingService` as an `IHostedService` for background tasks.
*   **`MatchmakingService.Application` (Application Layer):**
    *   Defines application-specific use cases and orchestrates domain entities.
    *   Contains application service interfaces (`ILobbyService`, `ILobbyNotifier`, `ILobbyCache`, `ILobbyCodePool`, `ISignalRClientService`).
    *   Contains concrete application service implementations (`LobbyService`, `LobbyNotifier`).
*   **`MatchmakingService.Domain` (Domain Layer):**
    *   Encapsulates core business rules, entities (`Lobby`, `Player`), and domain-specific interfaces (`ILobbyRepository`).
    *   Independent of all other layers.
*   **`MatchmakingService.Infrastructure.Data` (Infrastructure Layer - MongoDB):**
    *   Implements `ILobbyRepository` using `MongoDB.Driver` for data persistence.
*   **`MatchmakingService.Infrastructure.Redis` (Infrastructure Layer - Redis Pools):**
    *   Implements `ILobbyCodePool` for managing the Redis-backed code generation and recycling system.
*   **`MatchmakingService.Infrastructure.Caching` (Infrastructure Layer - Redis Cache):**
    *   Implements `ILobbyCache` using `IDistributedCache` for distributed caching of lobby data.

## Setup and Configuration

### Prerequisites

*   **.NET SDK 8.0** or later.
*   **Docker Desktop** (recommended for local MongoDB and Redis setup) or directly installed instances of:
    *   **MongoDB:** Version 4.0 or later.
    *   **Redis:** Version 5.0 or later.

### Local Environment Setup (using Docker)

1.  **Start MongoDB:**
    ```bash
    docker run -d -p 27017:27017 --name matchmaking-mongo mongo
    ```
2.  **Start Redis:**
    ```bash
    docker run -d -p 6379:6379 --name matchmaking-redis redis
    ```

### Configuration (`appsettings.json`)

Update your `appsettings.json` (or `appsettings.Development.json`) in the `MatchmakingService` project with your MongoDB and Redis connection details.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "MongoDBConnection": "mongodb://localhost:27017",
    "RedisConnection": "localhost:6379"
  },
  "MongoDbSettings": {
    "DatabaseName": "MatchmakingDb",
    "LobbiesCollectionName": "Lobbies"
  }
}
```

### Lobby Code Pool Population (Crucial Step!)

Before running the `MatchmakingService`, you **must** populate the Redis lobby code pool.

1.  **Create Word Files:** Create `adjectives.txt` and `nouns.txt` files (one word per line) in a convenient location.
    *   **`adjectives.txt` example:**
        ```
        FAST
        QUICK
        BRAVE
        SILENT
        ...
        ```
    *   **`nouns.txt` example:**
        ```
        TIGER
        EAGLE
        SHARK
        DRAGON
        ...
        ```
    *   Aim for a large enough combination of words and numbers (e.g., 500 adjectives, 500 nouns, numbers 0-99) to get millions of unique codes.
2.  **Run Population Script:** You need a separate console application or script that:
    *   Reads words from your `adjectives.txt` and `nouns.txt`.
    *   Generates all unique `ADJECTIVE-NOUN-NUMBER` combinations.
    *   Shuffles the list.
    *   Connects to your Redis server (using the `RedisConnection` string).
    *   Uses `_redisDatabase.SetAddAsync("available_lobby_codes", ...)` to push all shuffled codes into the Redis `SET` at database `2` (as configured in `RedisLobbyCodePool`).
    *   This script is run once initially, and then periodically (e.g., when `available_lobby_codes` count gets low) to refill the pool, without requiring service downtime.

### Running the Service

1.  **Restore NuGet Packages:**
    ```bash
    dotnet restore
    ```
2.  **Build the Project:**
    ```bash
    dotnet build
    ```
3.  **Run the Service:**
    ```bash
    dotnet run --project MatchmakingService
    ```
    The service will start, typically listening on `https://localhost:7xxx` (check console output).

## SignalR API Usage

The MatchmakingService exposes a SignalR Hub at `/lobbyhub`. Clients connect to this endpoint and can invoke server methods or subscribe to server events.

### Client-to-Server Methods (Invoked by Client)

Clients can invoke the following methods on the `LobbyHub`:

*   **`CreateLobby(string lobbyName, int maxPlayers, string userName)`:**
    *   Creates a new lobby. The calling client is automatically added.
    *   _Returns:_ Nothing directly, but triggers `LobbyCreated` or `LobbyCreationFailed` event to the caller.
*   **`JoinLobby(string lobbyCode, string userName)`:**
    *   Attempts to join an existing lobby.
    *   _Returns:_ Nothing directly, but triggers `JoinedLobby` or `JoinLobbyFailed` event to the caller, and `PlayerJoined` to others in the lobby.

### Server-to-Client Events (Client Subscribes To)

Clients should subscribe to these events to receive real-time updates:

*   **`LobbyCreated(string lobbyCode, string lobbyName)`:**
    *   _Sent to:_ Caller after successful lobby creation.
*   **`LobbyCreationFailed(string errorMessage)`:**
    *   _Sent to:_ Caller if lobby creation fails.
*   **`JoinedLobby(string lobbyCode, string lobbyName, int currentPlayers, int maxPlayers)`:**
    *   _Sent to:_ Caller after successfully joining a lobby.
*   **`JoinLobbyFailed(string errorMessage)`:**
    *   _Sent to:_ Caller if joining a lobby fails.
*   **`PlayerJoined(List<Player> currentPlayers)`:**
    *   _Sent to:_ All other clients in a lobby when a new player joins. Includes the updated list of all players.
*   **`PlayerLeft(string connectionIdOfPlayerWhoLeft, string userNameOfPlayerWhoLeft, List<Player> currentPlayers)`:**
    *   _Sent to:_ All remaining clients in a lobby when a player leaves or disconnects. Includes details of the player who left and the updated list of remaining players.
*   **`LobbyLocked(string lobbyCode, List<Player> currentPlayers)`:**
    *   _Sent to:_ All clients in a lobby when it reaches its maximum player count. Includes the lobby code and the final list of players.

---

This `README.md` should provide a solid foundation for anyone looking to understand, set up, and develop against your `MatchmakingService`.