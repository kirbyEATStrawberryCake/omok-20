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
    public TMP_Text currentPlayerText;              // ���� �÷��̾� ǥ�� �ؽ�Ʈ
    public TMP_Text gameStatusText;                 // ���� ���� ǥ�� �ؽ�Ʈ
    public Button confirmMoveButton;            // ���� Ȯ�� ��ư
    public Button surrenderButton;              // �׺� ��ư

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
        playerManager.RandomizePlayerStones(); // �÷��̾ �� ���� ���� �Ҵ�
        renjuRule.Initialize();

        // UI �ʱ�ȭ
        UpdateUI();

        // �̺�Ʈ ���
        boardManager.OnPositionSelected += OnPositionSelected;
        confirmMoveButton.onClick.AddListener(ConfirmMove);
        surrenderButton.onClick.AddListener(Surrender);

        Debug.Log("������ ���۵Ǿ����ϴ�!");
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
    /// ���� ���� ó��
    /// </summary>
    /// <param name="winner">�¸��� �÷��̾��� �� Ÿ��</param>
    private void EndGame(StoneType winner)
    {
        currentGameState = GameState.GameOver;

        // ���� ��� ���
        playerManager.RecordGameResult(winner);

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

        gameStatusText.text = resultMessage;

        // ��� ���� ���� ���
        ClearPendingMove();

        // ��� ��Ŀ �����
        boardManager.HideAllMarkers();

        Debug.Log($"���� ����: {resultMessage}");
    }

    /// <summary>
    /// UI ������Ʈ
    /// </summary>
    private void UpdateUI()
    {
        StoneType currentStone = playerManager.GetCurrentPlayer();
        string currentPlayerName = playerManager.GetCurrentPlayerName();
        string stoneColor = (currentStone == StoneType.Black) ? "�浹" : "�鵹";

        currentPlayerText.text = $"{currentPlayerName}�� {stoneColor} ����";

        if (currentGameState == GameState.Playing)
        {
            gameStatusText.text = hasPendingMove ? "������ Ȯ���ϼ���" : "���� ���� ��";
        }

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