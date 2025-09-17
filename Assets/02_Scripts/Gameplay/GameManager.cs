using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameState
{
    Playing,    // ���� ���� ��
    GameOver,   // ���� ����
    Paused      // ���� �Ͻ�����
}

public class GameManager : Singleton<GameManager>
{
    [Header("Game Components")]
    public BoardManager boardManager;           // ������ ������ ����
    public PlayerManager playerManager;         // �÷��̾� ������ ����
    public RenjuRule renjuRule;                // ���ַ� ������ ����

    [Header("UI Components")]
    public Image blackStonePlayerTurnImage;     // �浹 �÷��̾� ���� ǥ�� �̹���
    public Image whiteStonePlayerTurnImage;     // �鵹 �÷��̾� ���� ǥ�� �̹���

    public GameObject winPanel;                 // �¸� �г�
    public GameObject losePanel;                // �й� �г�
    public GameObject drawPanel;                // ���º� �г� (���û���

    public Button confirmMoveButton;            // ���� Ȯ�� ��ư
    public Button surrenderButton;              // �׺� ��ư

    // TMP Text �ʵ� �߰�
    [Header("Player Name Input")]
    public TMP_Text player1NameText;            // �÷��̾�1 �̸� TMP Text
    public TMP_Text player2NameText;            // �÷��̾�2 �̸� TMP Text

    [Header("Game Settings")]
    public bool isRenjuModeEnabled = true;      // ���ַ� ���� ����
    public bool showForbiddenPositions = true;  // ���� ��ġ ǥ�� ����

    public GameState currentGameState;         // ���� ���� ����
    private int totalMoves;                     // �� �� ī��Ʈ
    private Vector2Int pendingMove;             // Ȯ�� ��� ���� ���� ��ġ
    private bool hasPendingMove;                // Ȯ�� ��� ���� ������ �ִ���

    protected override void Awake()
    {
        // ������Ʈ �ڵ� ã�� (�ν����Ϳ��� �Ҵ���� ���� ���)
        if (boardManager == null) boardManager = GetComponent<BoardManager>();
        if (playerManager == null) playerManager = GetComponent<PlayerManager>();
        if (renjuRule == null) renjuRule = GetComponent<RenjuRule>();

        // �� �Ŵ����� GameManager ���� ����
        if (boardManager != null) boardManager.SetGameManager(this);
        if (renjuRule != null) renjuRule.SetGameManager(this);
    }

    void Start()
    {
        InitializeGame();
    }

    /// <summary>
    /// ���� �ʱ�ȭ
    /// </summary>
    public void InitializeGame()
    {
        currentGameState = GameState.Playing;
        totalMoves = 0;
        pendingMove = Vector2Int.zero;
        hasPendingMove = false;

        // �� �Ŵ��� �ʱ�ȭ
        boardManager.InitializeBoard();

        // UI���� �÷��̾� �̸��� �о�ͼ� ����
        string player1Name = GetPlayer1NameFromUI();
        string player2Name = GetPlayer2NameFromUI();

        // PlayerManager�� �÷��̾� �̸� ����
        playerManager.SetPlayerNames(player1Name, player2Name);
        playerManager.RandomizePlayerStones(); // �÷��̾ �� ���� ���� �Ҵ�

        renjuRule.Initialize();


        // UI �ʱ�ȭ
        InitializeUI();
        UpdatePlayerTurnDisplay();

        // �̺�Ʈ ���
        boardManager.OnPositionSelected += OnPositionSelected;
        confirmMoveButton.onClick.AddListener(ConfirmMove);
        surrenderButton.onClick.AddListener(Surrender);

        Debug.Log("������ ���۵Ǿ����ϴ�!");
    }

    /// <summary>
    /// UI �ʱ�ȭ (���� ���۽�)
    /// </summary>
    private void InitializeUI()
    {
        // ��� �гε� �����
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (drawPanel != null) drawPanel.SetActive(false);

        // ���� ǥ�� �̹����� �ʱ�ȭ
        if (blackStonePlayerTurnImage != null) blackStonePlayerTurnImage.gameObject.SetActive(false);
        if (whiteStonePlayerTurnImage != null) whiteStonePlayerTurnImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// �÷��̾� ���� ǥ�� ������Ʈ
    /// </summary>
    private void UpdatePlayerTurnDisplay()
    {
        if (playerManager == null) return;

        StoneType currentStone = playerManager.GetCurrentPlayer();

        // �浹/�鵹 ���� ǥ�� �̹��� ������Ʈ
        if (blackStonePlayerTurnImage != null)
        {
            blackStonePlayerTurnImage.gameObject.SetActive(currentStone == StoneType.Black);
        }

        if (whiteStonePlayerTurnImage != null)
        {
            whiteStonePlayerTurnImage.gameObject.SetActive(currentStone == StoneType.White);
        }

        Debug.Log($"���� ǥ�� ������Ʈ: {(currentStone == StoneType.Black ? "�浹" : "�鵹")} ����");
    }


    /// <summary>
    /// ���� ��� UI ǥ��
    /// </summary>
    /// <param name="winner">�¸��� �� Ÿ��</param>
    /// <param name="isCurrentPlayerWinner">���� �÷��̾ �¸��ߴ���</param>
    private void ShowGameResultUI(StoneType winner, bool isCurrentPlayerWinner)
    {
        // ���� ǥ�� �̹����� �����
        if (blackStonePlayerTurnImage != null) blackStonePlayerTurnImage.gameObject.SetActive(false);
        if (whiteStonePlayerTurnImage != null) whiteStonePlayerTurnImage.gameObject.SetActive(false);

        if (winner == StoneType.None) // ���º�
        {
            if (drawPanel != null)
            {
                drawPanel.SetActive(true);
            }
        }
        else // �º� ����
        {
            string winnerName = playerManager.GetPlayerNameByStone(winner);

            if (isCurrentPlayerWinner)
            {
                // �¸� �г� ǥ��
                if (winPanel != null)
                {
                    winPanel.SetActive(true);
                }
            }
            else
            {
                // �й� �г� ǥ��
                if (losePanel != null)
                {
                    losePanel.SetActive(true);
                }
            }
        }
    }

    // UI���� �÷��̾� �̸��� �������� �޼���� �߰�
    private string GetPlayer1NameFromUI()
    {
        if (player1NameText != null && !string.IsNullOrEmpty(player1NameText.text))
        {
            return player1NameText.text;
        }
        return "�÷��̾� 1"; // �⺻��
    }

    private string GetPlayer2NameFromUI()
    {
        if (player2NameText != null && !string.IsNullOrEmpty(player2NameText.text))
        {
            return player2NameText.text;
        }
        return "�÷��̾� 2"; // �⺻��
    }

    /// <summary>
    /// ���忡�� ��ġ�� ���õǾ��� �� ȣ��Ǵ� �̺�Ʈ �ڵ鷯
    /// </summary>
    /// <param name="x">���õ� ��ġ�� x��ǥ</param>
    /// <param name="y">���õ� ��ġ�� y��ǥ</param>
    private void OnPositionSelected(int x, int y)
    {
        if (currentGameState != GameState.Playing) return;

        StoneType currentPlayer = playerManager.GetCurrentPlayer();

        // �ش� ��ġ�� ���� ���� �� �ִ��� �˻�
        if (boardManager.CanPlaceStone(x, y, currentPlayer))
        {
            pendingMove = new Vector2Int(x, y);
            hasPendingMove = true;

            // ���� �� ��ġ ǥ��
            boardManager.ShowPendingMove(x, y, currentPlayer);

            UpdateUI();
            Debug.Log($"���� ���: ({x}, {y}) - {playerManager.GetCurrentPlayerName()}�� {(currentPlayer == StoneType.Black ? "�浹" : "�鵹")}");
        }
        else
        {
            // �߸��� ��ġ ���ý� ��� ���� ����
            ClearPendingMove();
        }
    }

    /// <summary>
    /// ���� Ȯ��
    /// </summary>
    public void ConfirmMove()
    {
        if (!hasPendingMove || currentGameState != GameState.Playing) return;

        StoneType currentPlayer = playerManager.GetCurrentPlayer();

        // ���� �� ����
        if (boardManager.PlaceStone(pendingMove.x, pendingMove.y, currentPlayer))
        {
            totalMoves++;

            // ���� ��ġ ǥ�� ������Ʈ
            if (showForbiddenPositions && isRenjuModeEnabled)
            {
                boardManager.UpdateForbiddenPositions();
            }

            // �¸� ���� �˻�
            if (renjuRule.CheckWinCondition(pendingMove.x, pendingMove.y, currentPlayer))
            {
                EndGame(currentPlayer);
                return;
            }

            // ���º� �˻� (�ٵ����� ������)
            if (totalMoves >= 225) // 15x15 = 225
            {
                EndGame(StoneType.None); // ���º�
                return;
            }

            // ��� ���� ����
            ClearPendingMove();

            // ���� �÷��̾�� �� ����
            playerManager.SwitchPlayer();
            UpdateUI();
        }
    }

    /// <summary>
    /// �׺� ó��
    /// </summary>
    public void Surrender()
    {
        if (currentGameState != GameState.Playing) return;

        StoneType currentPlayer = playerManager.GetCurrentPlayer();
        StoneType winner = playerManager.GetOpponentPlayer();

        string currentPlayerName = playerManager.GetCurrentPlayerName();
        string winnerName = playerManager.GetPlayerNameByStone(winner);

        // �׺��� �÷��̾��� ��밡 �¸�
        EndGame(winner);

        Debug.Log($"{currentPlayerName}�� �׺��߽��ϴ�. {winnerName} �¸�!");
    }

    /// <summary>
    /// ��� ���� ���� ���
    /// </summary>
    private void ClearPendingMove()
    {
        hasPendingMove = false;
        boardManager.HidePendingMove();
        UpdateUI();
    }

    /// <summary>
    /// ���� ���� ó�� (UI ǥ�� ��� ����)
    /// </summary>
    /// <param name="winner">�¸��� �÷��̾��� �� Ÿ��</param>
    private void EndGame(StoneType winner)
    {
        currentGameState = GameState.GameOver;

        // ���� ��� ���
        playerManager.RecordGameResult(winner);

        // ���� �÷��̾ �¸��ߴ��� Ȯ��
        bool isCurrentPlayerWinner = (winner == playerManager.GetCurrentPlayer());

        // ���� ��� UI ǥ��
        ShowGameResultUI(winner, isCurrentPlayerWinner);

        // ��� ���� ���� ���
        ClearPendingMove();

        // ��� ��Ŀ �����
        boardManager.HideAllMarkers();

        string resultMessage = "";
        switch (winner)
        {
            case StoneType.Black:
                resultMessage = $"{playerManager.GetBlackStonePlayerName()} �¸�! (�浹)";
                break;
            case StoneType.White:
                resultMessage = $"{playerManager.GetWhiteStonePlayerName()} �¸�! (�鵹)";
                break;
            case StoneType.None:
                resultMessage = "���º�!";
                break;
        }

        Debug.Log($"���� ����: {resultMessage}");
    }

    /// <summary>
    /// UI ������Ʈ (��ư ���¸� ����)
    /// </summary>
    private void UpdateUI()
    {
        // �÷��̾� ���� ǥ�� ������Ʈ
        UpdatePlayerTurnDisplay();

        // ���� Ȯ�� ��ư�� ��� ���� ������ ���� ���� Ȱ��ȭ
        confirmMoveButton.interactable = hasPendingMove && currentGameState == GameState.Playing;

        // �׺� ��ư�� ���� ���� ���� Ȱ��ȭ
        surrenderButton.interactable = currentGameState == GameState.Playing;
    }

    /// <summary>
    /// ���� ���� ��ȯ
    /// </summary>
    public GameState GetGameState()
    {
        return currentGameState;
    }

    /// <summary>
    /// ���� ��ġ ǥ�� ���
    /// </summary>
    public void ToggleForbiddenPositions()
    {
        showForbiddenPositions = !showForbiddenPositions;
        if (showForbiddenPositions && isRenjuModeEnabled)
        {
            boardManager.UpdateForbiddenPositions();
        }
        else
        {
            boardManager.HideForbiddenMarkers();
        }
    }
    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        throw new NotImplementedException();
    }
}