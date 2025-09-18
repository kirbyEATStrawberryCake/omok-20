public enum GameMode
{
    SinglePlayer,
    MultiPlayer
}

public static class GameModeManager
{
    public static GameMode Mode { get; set; }
}