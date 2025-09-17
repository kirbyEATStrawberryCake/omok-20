using UnityEngine;
using TMPro;

public enum PlayerID
{
    Player1 = 1,    // 플레이어 1
    Player2 = 2     // 플레이어 2
}

public class PlayerManager : MonoBehaviour
{
    [Header("Player Settings")]
    private string player1Name = "플레이어 1";
    private string player2Name = "플레이어 2";

    private StoneType currentPlayer;                // 현재 차례인 돌 타입 (항상 흑돌부터 시작)
    private PlayerID blackStonePlayer;              // 흑돌을 가진 플레이어
    private PlayerID whiteStonePlayer;              // 백돌을 가진 플레이어
    private PlayerID currentTurnPlayer;             // 현재 턴인 플레이어 ID
    
    // 게임 기록
    private int player1Wins = 0;                    // 플레이어 1 승수
    private int player2Wins = 0;                    // 플레이어 2 승수
    private int draws = 0;                          // 무승부 횟수
    
    // 플레이어 변경 시 발생하는 이벤트
    public System.Action<StoneType> OnPlayerChanged;
    
    void Start()
    {
        // 초기화는 GameManager에서 호출하므로 여기서는 하지 않음
    }

    /// <summary>
    /// 플레이어별 돌 색깔을 랜덤으로 할당하고 게임 시작
    /// </summary>
    public void RandomizePlayerStones()
    {
        // 랜덤으로 플레이어 1이 흑돌을 가질지 결정
        bool player1GetsBlack = Random.Range(0, 2) == 0;
        
        if (player1GetsBlack)
        {
            blackStonePlayer = PlayerID.Player1;
            whiteStonePlayer = PlayerID.Player2;
            Debug.Log($"{player1Name}이 흑돌(선공)을 가져갑니다!");
        }
        else
        {
            blackStonePlayer = PlayerID.Player2;
            whiteStonePlayer = PlayerID.Player1;
            Debug.Log($"{player2Name}이 흑돌(선공)을 가져갑니다!");
        }
        
        // 오목은 항상 흑돌부터 시작
        currentPlayer = StoneType.Black;
        currentTurnPlayer = blackStonePlayer;
        
        // 이벤트 발생
        OnPlayerChanged?.Invoke(currentPlayer);
    }
    
    /// <summary>
    /// 플레이어 턴 교체 (흑돌 -> 백돌 -> 흑돌 순서)
    /// </summary>
    public void SwitchPlayer()
    {
        // 돌 색깔 변경
        currentPlayer = (currentPlayer == StoneType.Black) ? StoneType.White : StoneType.Black;
        
        // 현재 턴 플레이어 변경
        currentTurnPlayer = (currentPlayer == StoneType.Black) ? blackStonePlayer : whiteStonePlayer;
        
        string currentPlayerName = GetCurrentPlayerName();
        string stoneColorName = (currentPlayer == StoneType.Black) ? "흑돌" : "백돌";
        
        Debug.Log($"턴 교체: {currentPlayerName}의 {stoneColorName} 차례");
        
        // 이벤트 발생
        OnPlayerChanged?.Invoke(currentPlayer);
    }
    
    /// <summary>
    /// 현재 차례인 돌 타입 반환
    /// </summary>
    /// <returns>현재 차례인 돌 타입</returns>
    public StoneType GetCurrentPlayer()
    {
        return currentPlayer;
    }
    
    /// <summary>
    /// 현재 차례인 플레이어의 이름 반환
    /// </summary>
    /// <returns>현재 차례인 플레이어 이름</returns>
    public string GetCurrentPlayerName()
    {
        return GetPlayerName(currentTurnPlayer);
    }
    
    /// <summary>
    /// 특정 돌 색깔을 가진 플레이어의 이름 반환
    /// </summary>
    /// <param name="stoneType">돌 타입</param>
    /// <returns>해당 돌을 가진 플레이어 이름</returns>
    public string GetPlayerNameByStone(StoneType stoneType)
    {
        if (stoneType == StoneType.Black)
        {
            return GetPlayerName(blackStonePlayer);
        }
        else if (stoneType == StoneType.White)
        {
            return GetPlayerName(whiteStonePlayer);
        }
        
        return "알 수 없음";
    }
    
    /// <summary>
    /// 플레이어 ID로 플레이어 이름 반환
    /// </summary>
    /// <param name="playerID">플레이어 ID</param>
    /// <returns>플레이어 이름</returns>
    private string GetPlayerName(PlayerID playerID)
    {
        return (playerID == PlayerID.Player1) ? player1Name : player2Name;
    }
    
    /// <summary>
    /// 흑돌을 가진 플레이어 이름 반환
    /// </summary>
    /// <returns>흑돌 플레이어 이름</returns>
    public string GetBlackStonePlayerName()
    {
        return GetPlayerName(blackStonePlayer);
    }
    
    /// <summary>
    /// 백돌을 가진 플레이어 이름 반환
    /// </summary>
    /// <returns>백돌 플레이어 이름</returns>
    public string GetWhiteStonePlayerName()
    {
        return GetPlayerName(whiteStonePlayer);
    }
    
    /// <summary>
    /// 현재 턴인 플레이어 ID 반환
    /// </summary>
    /// <returns>현재 턴 플레이어 ID</returns>
    public PlayerID GetCurrentTurnPlayerID()
    {
        return currentTurnPlayer;
    }
    
    /// <summary>
    /// 상대방 돌 타입 반환
    /// </summary>
    /// <returns>상대방의 돌 타입</returns>
    public StoneType GetOpponentPlayer()
    {
        return (currentPlayer == StoneType.Black) ? StoneType.White : StoneType.Black;
    }
    
    /// <summary>
    /// 상대방 플레이어 이름 반환
    /// </summary>
    /// <returns>상대방 플레이어 이름</returns>
    public string GetOpponentPlayerName()
    {
        PlayerID opponentID = (currentTurnPlayer == PlayerID.Player1) ? PlayerID.Player2 : PlayerID.Player1;
        return GetPlayerName(opponentID);
    }
    
    /// <summary>
    /// 게임 결과 기록
    /// </summary>
    /// <param name="winner">승리한 돌 타입 (무승부시 StoneType.None)</param>
    public void RecordGameResult(StoneType winner)
    {
        switch (winner)
        {
            case StoneType.Black:
                {
                    string winnerName = GetBlackStonePlayerName();
                    if (blackStonePlayer == PlayerID.Player1)
                        player1Wins++;
                    else
                        player2Wins++;
                    Debug.Log($"{winnerName} 승리! (흑돌로 승리)");
                }
                break;
                
            case StoneType.White:
                {
                    string winnerName = GetWhiteStonePlayerName();
                    if (whiteStonePlayer == PlayerID.Player1)
                        player1Wins++;
                    else
                        player2Wins++;
                    Debug.Log($"{winnerName} 승리! (백돌로 승리)");
                }
                break;
                
            case StoneType.None:
                draws++;
                Debug.Log("무승부!");
                break;
        }
        
        Debug.Log($"현재 전적 - {player1Name}: {player1Wins}승, {player2Name}: {player2Wins}승, 무승부: {draws}회");
    }
    
    /// <summary>
    /// 승패 기록 반환
    /// </summary>
    /// <returns>각 플레이어의 승수와 무승부 횟수</returns>
    public (int player1Wins, int player2Wins, int draws) GetGameRecord()
    {
        return (player1Wins, player2Wins, draws);
    }
    
    /// <summary>
    /// 게임 기록 초기화
    /// </summary>
    public void ResetGameRecord()
    {
        player1Wins = 0;
        player2Wins = 0;
        draws = 0;
        Debug.Log("게임 기록이 초기화되었습니다.");
    }

    /// <summary>
    /// 플레이어 이름 설정 (UI에서 읽어온 이름으로 설정)
    /// </summary>
    /// <param name="player1">플레이어 1 이름</param>
    /// <param name="player2">플레이어 2 이름</param>
    public void SetPlayerNames(string player1, string player2)
    {
        // 빈 문자열이나 null 체크하여 기본값 설정
        player1Name = string.IsNullOrEmpty(player1) ? "플레이어 1" : player1;
        player2Name = string.IsNullOrEmpty(player2) ? "플레이어 2" : player2;

        Debug.Log($"플레이어 설정: {player1Name} vs {player2Name}");
    }

    /// <summary>
    /// 특정 플레이어가 흑돌을 가지고 있는지 확인
    /// </summary>
    /// <param name="playerID">확인할 플레이어 ID</param>
    /// <returns>해당 플레이어가 흑돌을 가지고 있는지</returns>
    public bool IsPlayerBlack(PlayerID playerID)
    {
        return blackStonePlayer == playerID;
    }
    
    /// <summary>
    /// 특정 플레이어가 백돌을 가지고 있는지 확인
    /// </summary>
    /// <param name="playerID">확인할 플레이어 ID</param>
    /// <returns>해당 플레이어가 백돌을 가지고 있는지</returns>
    public bool IsPlayerWhite(PlayerID playerID)
    {
        return whiteStonePlayer == playerID;
    }
}