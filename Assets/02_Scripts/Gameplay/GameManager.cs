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
    public Button resetButton;                  // ���� ���� ��ư
    public Button undoButton;                   // ������ ��ư

    [Header("Game Settings")]
    public bool isRenjuModeEnabled = true;      // ���ַ� ���� ����

    public GameState currentGameState;         // ���� ���� ����
    private int totalMoves;                     // �� �� ī��Ʈ

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

        // �� �Ŵ��� �ʱ�ȭ
        boardManager.InitializeBoard();
        playerManager.InitializePlayers();
        renjuRule.Initialize();

        // UI �ʱ�ȭ
        UpdateUI();

        // �̺�Ʈ ���
        boardManager.OnStonePlace += OnStonePlaced;
        resetButton.onClick.AddListener(ResetGame);
        undoButton.onClick.AddListener(UndoMove);

        Debug.Log("������ ���۵Ǿ����ϴ�!");
    }

    /// <summary>
    /// ���� ������ �� ȣ��Ǵ� �̺�Ʈ �ڵ鷯
    /// </summary>
    /// <param name="x">���� ���� x��ǥ</param>
    /// <param name="y">���� ���� y��ǥ</param>
    /// <param name="stoneType">���� ���� Ÿ��</param>
    private void OnStonePlaced(int x, int y, StoneType stoneType)
    {
        if (currentGameState != GameState.Playing) return;

        totalMoves++;

        // �¸� ���� �˻�
        if (renjuRule.CheckWinCondition(x, y, stoneType))
        {
            EndGame(stoneType);
            return;
        }

        // ���º� �˻� (�ٵ����� ������)
        if (totalMoves >= 225) // 15x15 = 225
        {
            EndGame(StoneType.None); // ���º�
            return;
        }

        // ���� �÷��̾�� �� ����
        playerManager.SwitchPlayer();
        UpdateUI();
    }

    /// <summary>
    /// ���� ���� ó��
    /// </summary>
    /// <param name="winner">�¸��� �÷��̾��� �� Ÿ��</param>
    private void EndGame(StoneType winner)
    {
        currentGameState = GameState.GameOver;

        string resultMessage = "";
        switch (winner)
        {
            case StoneType.Black:
                resultMessage = "�浹 �¸�!";
                break;
            case StoneType.White:
                resultMessage = "�鵹 �¸�!";
                break;
            case StoneType.None:
                resultMessage = "���º�!";
                break;
        }

        gameStatusText.text = resultMessage;
        Debug.Log($"���� ����: {resultMessage}");
    }

    /// <summary>
    /// UI ������Ʈ
    /// </summary>
    private void UpdateUI()
    {
        StoneType currentPlayer = playerManager.GetCurrentPlayer();
        currentPlayerText.text = currentPlayer == StoneType.Black ? "�浹 ����" : "�鵹 ����";

        if (currentGameState == GameState.Playing)
        {
            gameStatusText.text = "���� ���� ��";
        }

        // ������ ��ư�� ù ���� �ƴ� ���� Ȱ��ȭ
        undoButton.interactable = totalMoves > 0 && currentGameState == GameState.Playing;
    }

    /// <summary>
    /// ���� ����
    /// </summary>
    public void ResetGame()
    {
        InitializeGame();
    }

    /// <summary>
    /// ������ (������ �� �ǵ�����)
    /// </summary>
    public void UndoMove()
    {
        if (totalMoves > 0 && currentGameState == GameState.Playing)
        {
            boardManager.UndoLastMove();
            playerManager.SwitchPlayer(); // ���� ���� �÷��̾�� �ǵ���
            totalMoves--;
            UpdateUI();
        }
    }

    /// <summary>
    /// ���� ���� ��ȯ
    /// </summary>
    public GameState GetGameState()
    {
        return currentGameState;
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        throw new NotImplementedException();
    }
}