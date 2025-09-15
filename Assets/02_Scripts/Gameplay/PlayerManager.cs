using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Settings")]
    public bool blackPlayerStarts = true;      // 흑돌이 먼저 시작하는지 여부

    private StoneType currentPlayer;            // 현재 플레이어
    private int blackWins = 0;                  // 흑돌 승수
    private int whiteWins = 0;                  // 백돌 승수
    private int draws = 0;                      // 무승부 횟수

    // 플레이어 변경 시 발생하는 이벤트
    public System.Action<StoneType> OnPlayerChanged;

    void Start()
    {
        InitializePlayers();
    }

    /// <summary>
    /// 플레이어 초기화
    /// </summary>
    public void InitializePlayers()
    {
        // 첫 번째 플레이어 설정 (일반적으로 흑돌부터)
        currentPlayer = blackPlayerStarts ? StoneType.Black : StoneType.White;

        Debug.Log($"게임 시작! {GetCurrentPlayerName()}부터 시작합니다.");

        // 이벤트 발생
        OnPlayerChanged?.Invoke(currentPlayer);
    }

    /// <summary>
    /// 플레이어 턴 교체
    /// </summary>
    public void SwitchPlayer()
    {
        currentPlayer = (currentPlayer == StoneType.Black) ? StoneType.White : StoneType.Black;

        Debug.Log($"턴 교체: 현재 플레이어는 {GetCurrentPlayerName()}");

        // 이벤트 발생
        OnPlayerChanged?.Invoke(currentPlayer);
    }

    /// <summary>
    /// 현재 플레이어 반환
    /// </summary>
    /// <returns>현재 플레이어의 돌 타입</returns>
    public StoneType GetCurrentPlayer()
    {
        return currentPlayer;
    }

    /// <summary>
    /// 현재 플레이어 이름 반환
    /// </summary>
    /// <returns>현재 플레이어 이름 (흑돌/백돌)</returns>
    public string GetCurrentPlayerName()
    {
        return currentPlayer == StoneType.Black ? "흑돌" : "백돌";
    }

    /// <summary>
    /// 상대방 플레이어 타입 반환
    /// </summary>
    /// <returns>상대방의 돌 타입</returns>
    public StoneType GetOpponentPlayer()
    {
        return (currentPlayer == StoneType.Black) ? StoneType.White : StoneType.Black;
    }

    /// <summary>
    /// 게임 결과 기록
    /// </summary>
    /// <param name="winner">승리한 플레이어 (무승부시 StoneType.None)</param>
    public void RecordGameResult(StoneType winner)
    {
        switch (winner)
        {
            case StoneType.Black:
                blackWins++;
                Debug.Log($"흑돌 승리! (흑돌: {blackWins}승, 백돌: {whiteWins}승, 무승부: {draws}회)");
                break;
            case StoneType.White:
                whiteWins++;
                Debug.Log($"백돌 승리! (흑돌: {blackWins}승, 백돌: {whiteWins}승, 무승부: {draws}회)");
                break;
            case StoneType.None:
                draws++;
                Debug.Log($"무승부! (흑돌: {blackWins}승, 백돌: {whiteWins}승, 무승부: {draws}회)");
                break;
        }
    }

    /// <summary>
    /// 승패 기록 반환
    /// </summary>
    /// <returns>각 플레이어의 승수와 무승부 횟수</returns>
    public (int blackWins, int whiteWins, int draws) GetGameRecord()
    {
        return (blackWins, whiteWins, draws);
    }

    /// <summary>
    /// 게임 기록 초기화
    /// </summary>
    public void ResetGameRecord()
    {
        blackWins = 0;
        whiteWins = 0;
        draws = 0;
        Debug.Log("게임 기록이 초기화되었습니다.");
    }

    /// <summary>
    /// 첫 번째 플레이어 설정
    /// </summary>
    /// <param name="stoneType">시작할 플레이어의 돌 타입</param>
    public void SetFirstPlayer(StoneType stoneType)
    {
        if (stoneType == StoneType.Black || stoneType == StoneType.White)
        {
            currentPlayer = stoneType;
            OnPlayerChanged?.Invoke(currentPlayer);
        }
    }
}
