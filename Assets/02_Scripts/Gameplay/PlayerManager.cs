using UnityEngine;
using TMPro;

public enum PlayerID
{
    Player1 = 1,    // �÷��̾� 1
    Player2 = 2     // �÷��̾� 2
}

public class PlayerManager : MonoBehaviour
{
    [Header("Player Settings")]
    private string player1Name = "�÷��̾� 1";
    private string player2Name = "�÷��̾� 2";

    private StoneType currentPlayer;                // ���� ������ �� Ÿ�� (�׻� �浹���� ����)
    private PlayerID blackStonePlayer;              // �浹�� ���� �÷��̾�
    private PlayerID whiteStonePlayer;              // �鵹�� ���� �÷��̾�
    private PlayerID currentTurnPlayer;             // ���� ���� �÷��̾� ID
    
    // ���� ���
    private int player1Wins = 0;                    // �÷��̾� 1 �¼�
    private int player2Wins = 0;                    // �÷��̾� 2 �¼�
    private int draws = 0;                          // ���º� Ƚ��
    
    // �÷��̾� ���� �� �߻��ϴ� �̺�Ʈ
    public System.Action<StoneType> OnPlayerChanged;
    
    void Start()
    {
        // �ʱ�ȭ�� GameManager���� ȣ���ϹǷ� ���⼭�� ���� ����
    }

    /// <summary>
    /// �÷��̾ �� ������ �������� �Ҵ��ϰ� ���� ����
    /// </summary>
    public void RandomizePlayerStones()
    {
        // �������� �÷��̾� 1�� �浹�� ������ ����
        bool player1GetsBlack = Random.Range(0, 2) == 0;
        
        if (player1GetsBlack)
        {
            blackStonePlayer = PlayerID.Player1;
            whiteStonePlayer = PlayerID.Player2;
            Debug.Log($"{player1Name}�� �浹(����)�� �������ϴ�!");
        }
        else
        {
            blackStonePlayer = PlayerID.Player2;
            whiteStonePlayer = PlayerID.Player1;
            Debug.Log($"{player2Name}�� �浹(����)�� �������ϴ�!");
        }
        
        // ������ �׻� �浹���� ����
        currentPlayer = StoneType.Black;
        currentTurnPlayer = blackStonePlayer;
        
        // �̺�Ʈ �߻�
        OnPlayerChanged?.Invoke(currentPlayer);
    }
    
    /// <summary>
    /// �÷��̾� �� ��ü (�浹 -> �鵹 -> �浹 ����)
    /// </summary>
    public void SwitchPlayer()
    {
        // �� ���� ����
        currentPlayer = (currentPlayer == StoneType.Black) ? StoneType.White : StoneType.Black;
        
        // ���� �� �÷��̾� ����
        currentTurnPlayer = (currentPlayer == StoneType.Black) ? blackStonePlayer : whiteStonePlayer;
        
        string currentPlayerName = GetCurrentPlayerName();
        string stoneColorName = (currentPlayer == StoneType.Black) ? "�浹" : "�鵹";
        
        Debug.Log($"�� ��ü: {currentPlayerName}�� {stoneColorName} ����");
        
        // �̺�Ʈ �߻�
        OnPlayerChanged?.Invoke(currentPlayer);
    }
    
    /// <summary>
    /// ���� ������ �� Ÿ�� ��ȯ
    /// </summary>
    /// <returns>���� ������ �� Ÿ��</returns>
    public StoneType GetCurrentPlayer()
    {
        return currentPlayer;
    }
    
    /// <summary>
    /// ���� ������ �÷��̾��� �̸� ��ȯ
    /// </summary>
    /// <returns>���� ������ �÷��̾� �̸�</returns>
    public string GetCurrentPlayerName()
    {
        return GetPlayerName(currentTurnPlayer);
    }
    
    /// <summary>
    /// Ư�� �� ������ ���� �÷��̾��� �̸� ��ȯ
    /// </summary>
    /// <param name="stoneType">�� Ÿ��</param>
    /// <returns>�ش� ���� ���� �÷��̾� �̸�</returns>
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
        
        return "�� �� ����";
    }
    
    /// <summary>
    /// �÷��̾� ID�� �÷��̾� �̸� ��ȯ
    /// </summary>
    /// <param name="playerID">�÷��̾� ID</param>
    /// <returns>�÷��̾� �̸�</returns>
    private string GetPlayerName(PlayerID playerID)
    {
        return (playerID == PlayerID.Player1) ? player1Name : player2Name;
    }
    
    /// <summary>
    /// �浹�� ���� �÷��̾� �̸� ��ȯ
    /// </summary>
    /// <returns>�浹 �÷��̾� �̸�</returns>
    public string GetBlackStonePlayerName()
    {
        return GetPlayerName(blackStonePlayer);
    }
    
    /// <summary>
    /// �鵹�� ���� �÷��̾� �̸� ��ȯ
    /// </summary>
    /// <returns>�鵹 �÷��̾� �̸�</returns>
    public string GetWhiteStonePlayerName()
    {
        return GetPlayerName(whiteStonePlayer);
    }
    
    /// <summary>
    /// ���� ���� �÷��̾� ID ��ȯ
    /// </summary>
    /// <returns>���� �� �÷��̾� ID</returns>
    public PlayerID GetCurrentTurnPlayerID()
    {
        return currentTurnPlayer;
    }
    
    /// <summary>
    /// ���� �� Ÿ�� ��ȯ
    /// </summary>
    /// <returns>������ �� Ÿ��</returns>
    public StoneType GetOpponentPlayer()
    {
        return (currentPlayer == StoneType.Black) ? StoneType.White : StoneType.Black;
    }
    
    /// <summary>
    /// ���� �÷��̾� �̸� ��ȯ
    /// </summary>
    /// <returns>���� �÷��̾� �̸�</returns>
    public string GetOpponentPlayerName()
    {
        PlayerID opponentID = (currentTurnPlayer == PlayerID.Player1) ? PlayerID.Player2 : PlayerID.Player1;
        return GetPlayerName(opponentID);
    }
    
    /// <summary>
    /// ���� ��� ���
    /// </summary>
    /// <param name="winner">�¸��� �� Ÿ�� (���ºν� StoneType.None)</param>
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
                    Debug.Log($"{winnerName} �¸�! (�浹�� �¸�)");
                }
                break;
                
            case StoneType.White:
                {
                    string winnerName = GetWhiteStonePlayerName();
                    if (whiteStonePlayer == PlayerID.Player1)
                        player1Wins++;
                    else
                        player2Wins++;
                    Debug.Log($"{winnerName} �¸�! (�鵹�� �¸�)");
                }
                break;
                
            case StoneType.None:
                draws++;
                Debug.Log("���º�!");
                break;
        }
        
        Debug.Log($"���� ���� - {player1Name}: {player1Wins}��, {player2Name}: {player2Wins}��, ���º�: {draws}ȸ");
    }
    
    /// <summary>
    /// ���� ��� ��ȯ
    /// </summary>
    /// <returns>�� �÷��̾��� �¼��� ���º� Ƚ��</returns>
    public (int player1Wins, int player2Wins, int draws) GetGameRecord()
    {
        return (player1Wins, player2Wins, draws);
    }
    
    /// <summary>
    /// ���� ��� �ʱ�ȭ
    /// </summary>
    public void ResetGameRecord()
    {
        player1Wins = 0;
        player2Wins = 0;
        draws = 0;
        Debug.Log("���� ����� �ʱ�ȭ�Ǿ����ϴ�.");
    }

    /// <summary>
    /// �÷��̾� �̸� ���� (UI���� �о�� �̸����� ����)
    /// </summary>
    /// <param name="player1">�÷��̾� 1 �̸�</param>
    /// <param name="player2">�÷��̾� 2 �̸�</param>
    public void SetPlayerNames(string player1, string player2)
    {
        // �� ���ڿ��̳� null üũ�Ͽ� �⺻�� ����
        player1Name = string.IsNullOrEmpty(player1) ? "�÷��̾� 1" : player1;
        player2Name = string.IsNullOrEmpty(player2) ? "�÷��̾� 2" : player2;

        Debug.Log($"�÷��̾� ����: {player1Name} vs {player2Name}");
    }

    /// <summary>
    /// Ư�� �÷��̾ �浹�� ������ �ִ��� Ȯ��
    /// </summary>
    /// <param name="playerID">Ȯ���� �÷��̾� ID</param>
    /// <returns>�ش� �÷��̾ �浹�� ������ �ִ���</returns>
    public bool IsPlayerBlack(PlayerID playerID)
    {
        return blackStonePlayer == playerID;
    }
    
    /// <summary>
    /// Ư�� �÷��̾ �鵹�� ������ �ִ��� Ȯ��
    /// </summary>
    /// <param name="playerID">Ȯ���� �÷��̾� ID</param>
    /// <returns>�ش� �÷��̾ �鵹�� ������ �ִ���</returns>
    public bool IsPlayerWhite(PlayerID playerID)
    {
        return whiteStonePlayer == playerID;
    }
}