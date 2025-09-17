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
    public Image blackStonePlayerTurnImage;     // 흑돌 플레이어 차례 표시 이미지
    public Image whiteStonePlayerTurnImage;     // 백돌 플레이어 차례 표시 이미지

    public GameObject winPanel;                 // 승리 패널
    public GameObject losePanel;                // 패배 패널
    public GameObject drawPanel;                // 무승부 패널 (선택사항

    public Button confirmMoveButton;            // 착수 확정 버튼
    public Button surrenderButton;              // 항복 버튼

    // TMP Text 필드 추가
    [Header("Player Name Input")]
    public TMP_Text player1NameText;            // 플레이어1 이름 TMP Text
    public TMP_Text player2NameText;            // 플레이어2 이름 TMP Text

    [Header("Game Settings")]
    public bool isRenjuModeEnabled = true;      // 렌주룰 적용 여부
    public bool showForbiddenPositions = true;  // 금지 위치 표시 여부

    public GameState currentGameState;         // 현재 게임 상태
    private int totalMoves;                     // 총 수 카운트
    private Vector2Int pendingMove;             // 확정 대기 중인 착수 위치
    private bool hasPendingMove;                // 확정 대기 중인 착수가 있는지

    protected override void Awake()
    {
        // 컴포넌트 자동 찾기 (인스펙터에서 할당되지 않은 경우)
        if (boardManager == null) boardManager = GetComponent<BoardManager>();
        if (playerManager == null) playerManager = GetComponent<PlayerManager>();
        if (renjuRule == null) renjuRule = GetComponent<RenjuRule>();

        // 각 매니저에 GameManager 참조 전달
        if (boardManager != null) boardManager.SetGameManager(this);
        if (renjuRule != null) renjuRule.SetGameManager(this);
    }

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
        pendingMove = Vector2Int.zero;
        hasPendingMove = false;

        // 각 매니저 초기화
        boardManager.InitializeBoard();

        // UI에서 플레이어 이름을 읽어와서 설정
        string player1Name = GetPlayer1NameFromUI();
        string player2Name = GetPlayer2NameFromUI();

        // PlayerManager에 플레이어 이름 설정
        playerManager.SetPlayerNames(player1Name, player2Name);
        playerManager.RandomizePlayerStones(); // 플레이어별 돌 색깔 랜덤 할당

        renjuRule.Initialize();


        // UI 초기화
        InitializeUI();
        UpdatePlayerTurnDisplay();

        // 이벤트 등록
        boardManager.OnPositionSelected += OnPositionSelected;
        confirmMoveButton.onClick.AddListener(ConfirmMove);
        surrenderButton.onClick.AddListener(Surrender);

        Debug.Log("게임이 시작되었습니다!");
    }

    /// <summary>
    /// UI 초기화 (게임 시작시)
    /// </summary>
    private void InitializeUI()
    {
        // 결과 패널들 숨기기
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (drawPanel != null) drawPanel.SetActive(false);

        // 차례 표시 이미지들 초기화
        if (blackStonePlayerTurnImage != null) blackStonePlayerTurnImage.gameObject.SetActive(false);
        if (whiteStonePlayerTurnImage != null) whiteStonePlayerTurnImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// 플레이어 차례 표시 업데이트
    /// </summary>
    private void UpdatePlayerTurnDisplay()
    {
        if (playerManager == null) return;

        StoneType currentStone = playerManager.GetCurrentPlayer();

        // 흑돌/백돌 차례 표시 이미지 업데이트
        if (blackStonePlayerTurnImage != null)
        {
            blackStonePlayerTurnImage.gameObject.SetActive(currentStone == StoneType.Black);
        }

        if (whiteStonePlayerTurnImage != null)
        {
            whiteStonePlayerTurnImage.gameObject.SetActive(currentStone == StoneType.White);
        }

        Debug.Log($"차례 표시 업데이트: {(currentStone == StoneType.Black ? "흑돌" : "백돌")} 차례");
    }


    /// <summary>
    /// 게임 결과 UI 표시
    /// </summary>
    /// <param name="winner">승리한 돌 타입</param>
    /// <param name="isCurrentPlayerWinner">현재 플레이어가 승리했는지</param>
    private void ShowGameResultUI(StoneType winner, bool isCurrentPlayerWinner)
    {
        // 차례 표시 이미지들 숨기기
        if (blackStonePlayerTurnImage != null) blackStonePlayerTurnImage.gameObject.SetActive(false);
        if (whiteStonePlayerTurnImage != null) whiteStonePlayerTurnImage.gameObject.SetActive(false);

        if (winner == StoneType.None) // 무승부
        {
            if (drawPanel != null)
            {
                drawPanel.SetActive(true);
            }
        }
        else // 승부 결정
        {
            string winnerName = playerManager.GetPlayerNameByStone(winner);

            if (isCurrentPlayerWinner)
            {
                // 승리 패널 표시
                if (winPanel != null)
                {
                    winPanel.SetActive(true);
                }
            }
            else
            {
                // 패배 패널 표시
                if (losePanel != null)
                {
                    losePanel.SetActive(true);
                }
            }
        }
    }

    // UI에서 플레이어 이름을 가져오는 메서드들 추가
    private string GetPlayer1NameFromUI()
    {
        if (player1NameText != null && !string.IsNullOrEmpty(player1NameText.text))
        {
            return player1NameText.text;
        }
        return "플레이어 1"; // 기본값
    }

    private string GetPlayer2NameFromUI()
    {
        if (player2NameText != null && !string.IsNullOrEmpty(player2NameText.text))
        {
            return player2NameText.text;
        }
        return "플레이어 2"; // 기본값
    }

    /// <summary>
    /// 보드에서 위치가 선택되었을 때 호출되는 이벤트 핸들러
    /// </summary>
    /// <param name="x">선택된 위치의 x좌표</param>
    /// <param name="y">선택된 위치의 y좌표</param>
    private void OnPositionSelected(int x, int y)
    {
        if (currentGameState != GameState.Playing) return;

        StoneType currentPlayer = playerManager.GetCurrentPlayer();

        // 해당 위치에 돌을 놓을 수 있는지 검사
        if (boardManager.CanPlaceStone(x, y, currentPlayer))
        {
            pendingMove = new Vector2Int(x, y);
            hasPendingMove = true;

            // 예상 돌 위치 표시
            boardManager.ShowPendingMove(x, y, currentPlayer);

            UpdateUI();
            Debug.Log($"착수 대기: ({x}, {y}) - {playerManager.GetCurrentPlayerName()}의 {(currentPlayer == StoneType.Black ? "흑돌" : "백돌")}");
        }
        else
        {
            // 잘못된 위치 선택시 대기 상태 해제
            ClearPendingMove();
        }
    }

    /// <summary>
    /// 착수 확정
    /// </summary>
    public void ConfirmMove()
    {
        if (!hasPendingMove || currentGameState != GameState.Playing) return;

        StoneType currentPlayer = playerManager.GetCurrentPlayer();

        // 실제 돌 놓기
        if (boardManager.PlaceStone(pendingMove.x, pendingMove.y, currentPlayer))
        {
            totalMoves++;

            // 금지 위치 표시 업데이트
            if (showForbiddenPositions && isRenjuModeEnabled)
            {
                boardManager.UpdateForbiddenPositions();
            }

            // 승리 조건 검사
            if (renjuRule.CheckWinCondition(pendingMove.x, pendingMove.y, currentPlayer))
            {
                EndGame(currentPlayer);
                return;
            }

            // 무승부 검사 (바둑판이 가득참)
            if (totalMoves >= 225) // 15x15 = 225
            {
                EndGame(StoneType.None); // 무승부
                return;
            }

            // 대기 상태 해제
            ClearPendingMove();

            // 다음 플레이어로 턴 변경
            playerManager.SwitchPlayer();
            UpdateUI();
        }
    }

    /// <summary>
    /// 항복 처리
    /// </summary>
    public void Surrender()
    {
        if (currentGameState != GameState.Playing) return;

        StoneType currentPlayer = playerManager.GetCurrentPlayer();
        StoneType winner = playerManager.GetOpponentPlayer();

        string currentPlayerName = playerManager.GetCurrentPlayerName();
        string winnerName = playerManager.GetPlayerNameByStone(winner);

        // 항복한 플레이어의 상대가 승리
        EndGame(winner);

        Debug.Log($"{currentPlayerName}이 항복했습니다. {winnerName} 승리!");
    }

    /// <summary>
    /// 대기 중인 착수 취소
    /// </summary>
    private void ClearPendingMove()
    {
        hasPendingMove = false;
        boardManager.HidePendingMove();
        UpdateUI();
    }

    /// <summary>
    /// 게임 종료 처리 (UI 표시 방식 변경)
    /// </summary>
    /// <param name="winner">승리한 플레이어의 돌 타입</param>
    private void EndGame(StoneType winner)
    {
        currentGameState = GameState.GameOver;

        // 게임 결과 기록
        playerManager.RecordGameResult(winner);

        // 현재 플레이어가 승리했는지 확인
        bool isCurrentPlayerWinner = (winner == playerManager.GetCurrentPlayer());

        // 게임 결과 UI 표시
        ShowGameResultUI(winner, isCurrentPlayerWinner);

        // 대기 중인 착수 취소
        ClearPendingMove();

        // 모든 마커 숨기기
        boardManager.HideAllMarkers();

        string resultMessage = "";
        switch (winner)
        {
            case StoneType.Black:
                resultMessage = $"{playerManager.GetBlackStonePlayerName()} 승리! (흑돌)";
                break;
            case StoneType.White:
                resultMessage = $"{playerManager.GetWhiteStonePlayerName()} 승리! (백돌)";
                break;
            case StoneType.None:
                resultMessage = "무승부!";
                break;
        }

        Debug.Log($"게임 종료: {resultMessage}");
    }

    /// <summary>
    /// UI 업데이트 (버튼 상태만 관리)
    /// </summary>
    private void UpdateUI()
    {
        // 플레이어 차례 표시 업데이트
        UpdatePlayerTurnDisplay();

        // 착수 확정 버튼은 대기 중인 착수가 있을 때만 활성화
        confirmMoveButton.interactable = hasPendingMove && currentGameState == GameState.Playing;

        // 항복 버튼은 게임 중일 때만 활성화
        surrenderButton.interactable = currentGameState == GameState.Playing;
    }

    /// <summary>
    /// 게임 상태 반환
    /// </summary>
    public GameState GetGameState()
    {
        return currentGameState;
    }

    /// <summary>
    /// 금지 위치 표시 토글
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