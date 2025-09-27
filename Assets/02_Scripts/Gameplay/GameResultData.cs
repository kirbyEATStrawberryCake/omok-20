public enum GameResult
{
    None,
    Player1Win, // 싱글플레이 전용
    Player2Win, // 싱글플레이 전용
    Victory, // 멀티플레이: 내가 승리
    Defeat, // 멀티플레이: 내가 패배
    Draw,
    Disconnect // 멀티플레이: 상대방 나감
}

public class GameResultData
{
    public GameResult result;
}