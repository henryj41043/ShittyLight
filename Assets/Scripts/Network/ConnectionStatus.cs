namespace ShittyLight.Lobby.Networking
{
    public enum ConnectStatus
    {
        Undefined,
        Success,
        ServerFull,
        GameInProgress,
        LoggedInAgain,
        UserRequestedDisconnect,
        GenericDisconnect
    }
}
