public enum GameMode
{
    SinglePlayer,
    MultiPlayer,
    AI
}

public static class GameModeManager
{
    public static GameMode Mode { get; set; }
}