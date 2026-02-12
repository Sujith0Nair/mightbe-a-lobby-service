namespace MatchMakingService.Application.Constants;

public static class SignalRConstants
{
    // <--- Incoming --->
    public const string CreateLobby = "CreateLobby";
    public const string JoinLobby = "JoinLobby";
    public const string LeaveLobby = "LeaveLobby";

    // <--- Outgoing --->
    public const string OnLobbyCreated = "LobbyCreated";
    public const string OnLobbyCreationFailed = "LobbyCreationFailed";
    public const string OnLobbyJoined = "LobbyJoined";
    public const string OnLobbyJoinFailed = "LobbyJoinFailed";
    public const string OnPlayerJoined = "PlayerJoined";
    public const string OnPlayerLeft = "PlayerLeft";
    public const string OnLobbyLocked = "LobbyLocked";
    public const string OnLobbyNotFound = "LobbyNotFound";
    public const string OnLobbyFull = "LobbyFull";
    public const string PlayerAlreadyInLobby = nameof(PlayerAlreadyInLobby);
}