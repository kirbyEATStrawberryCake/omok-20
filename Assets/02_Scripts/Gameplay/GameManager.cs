using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameState
{
    Playing,    // 게임 진행 중
    GameOver,   // 게임 종료
    Paused      // 게임 일시정지
}

public class GameManager : Singleton<GameManager>
{
    [Header("Game Components")]
    public BoardManager boardManager;           // 오목판 관리자 참조
    public PlayerManager playerManager;         // 플레이어 관리자 참조
    public RenjuRule renjuRule;                // 렌주룰 관리자 참조

    [Header("UI Components")]
    public TMP_Text currentPlayerText;              // 현재 플레이어 표시 텍스트
    public TMP_Text gameStatusText;                 // 게임 상태 표시 텍스트
    public Button resetButton;                  // 게임 리셋 버튼
    public Button undoButton;                   // 무르기 버튼

    [Header("Game Settings")]
    public bool isRenjuModeEnabled = true;      // 렌주룰 적용 여부

    public GameState currentGameState;         // 현재 게임 상태
    private int totalMoves;                     // 총 수 카운트

    void Start()
    {
        InitializeGame();
    }

    /// <summary>
    /// 게임 초기화
    /// </summary>
    public void InitializeGame()
    {
        currentGameState = GameState.Playing;
        totalMoves = 0;

        // 각 매니저 초기화
        boardManager.InitializeBoard();
        playerManager.InitializePlayers();
        renjuRule.Initialize();

        // UI 초기화
        UpdateUI();

        // 이벤트 등록
        boardManager.OnStonePlace += OnStonePlaced;
        resetButton.onClick.AddListener(ResetGame);
        undoButton.onClick.AddListener(UndoMove);

        Debug.Log("게임이 시작되었습니다!");
    }

    /// <summary>
    /// 돌이 놓였을 때 호출되는 이벤트 핸들러
    /// </summary>
    /// <param name="x">놓인 돌의 x좌표</param>
    /// <param name="y">놓인 돌의 y좌표</param>
    /// <param name="stoneType">놓인 돌의 타입</param>
    private void OnStonePlaced(int x, int y, StoneType stoneType)
    {
        if (currentGameState != GameState.Playing) return;

        totalMoves++;

        // 승리 조건 검사
        if (renjuRule.CheckWinCondition(x, y, stoneType))
        {
            EndGame(stoneType);
            return;
        }

        // 무승부 검사 (바둑판이 가득참)
        if (totalMoves >= 225) // 15x15 = 225
        {
            EndGame(StoneType.None); // 무승부
            return;
        }

        // 다음 플레이어로 턴 변경
        playerManager.SwitchPlayer();
        UpdateUI();
    }

    /// <summary>
    /// 게임 종료 처리
    /// </summary>
    /// <param name="winner">승리한 플레이어의 돌 타입</param>
    private void EndGame(StoneType winner)
    {
        currentGameState = GameState.GameOver;

        string resultMessage = "";
        switch (winner)
        {
            case StoneType.Black:
                resultMessage = "흑돌 승리!";
                break;
            case StoneType.White:
                resultMessage = "백돌 승리!";
                break;
            case StoneType.None:
                resultMessage = "무승부!";
                break;
        }

        gameStatusText.text = resultMessage;
        Debug.Log($"게임 종료: {resultMessage}");
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        StoneType currentPlayer = playerManager.GetCurrentPlayer();
        currentPlayerText.text = currentPlayer == StoneType.Black ? "흑돌 차례" : "백돌 차례";

        if (currentGameState == GameState.Playing)
        {
            gameStatusText.text = "게임 진행 중";
        }

        // 무르기 버튼은 첫 수가 아닐 때만 활성화
        undoButton.interactable = totalMoves > 0 && currentGameState == GameState.Playing;
    }

    /// <summary>
    /// 게임 리셋
    /// </summary>
    public void ResetGame()
    {
        InitializeGame();
    }

    /// <summary>
    /// 무르기 (마지막 수 되돌리기)
    /// </summary>
    public void UndoMove()
    {
        if (totalMoves > 0 && currentGameState == GameState.Playing)
        {
            boardManager.UndoLastMove();
            playerManager.SwitchPlayer(); // 턴을 이전 플레이어로 되돌림
            totalMoves--;
            UpdateUI();
        }
    }

    /// <summary>
    /// 게임 상태 반환
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