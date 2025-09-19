using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum GameState
{
    Default,   // 초기 상태
    Playing, // 게임 진행 중
    GameOver, // 게임 종료
    Paused // 게임 일시정지
}

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

[RequireComponent(typeof(RenjuRule))]
public class GamePlayManager : Singleton<GamePlayManager>
{
    public BoardManager boardManager { get; private set; } // 오목판 관리자 참조
    public GameLogic gameLogic { get; private set; } = new GameLogic(); // 플레이어 관리자 참조
    public RenjuRule renjuRule { get; private set; } // 렌주룰 관리자 참조
    public GameSceneUIManager uiManager => GameSceneUIManager.Instance;
    public MultiplayManager multiplayManager => MultiplayManager.Instance;

    [Header("Game Settings")]
    [SerializeField] private bool isRenjuModeEnabled = true; // 렌주룰 적용 여부

    public bool IsRenjuModeEnabled => isRenjuModeEnabled;

    [SerializeField] private bool showForbiddenPositions = true; // 금지 위치 표시 여부
    public bool ShowForbiddenPositions => showForbiddenPositions;

    public GameState currentGameState = GameState.Default; // 현재 게임 상태
    private int totalMoves; // 총 수 카운트

    public event UnityAction OnGameStart;
    public event UnityAction<GameResult> OnGameEnd;

    #region 유니티 이벤트

    protected override void Awake()
    {
        base.Awake();
        boardManager = GameObject.FindFirstObjectByType<BoardManager>();
        renjuRule = GetComponent<RenjuRule>();

        // TODO: 싱글모드 테스트
        // GameModeManager.Mode = GameMode.SinglePlayer;
    }

    private void OnEnable()
    {
        gameLogic.Initialize();
        gameLogic.WinConditionChecked += EndGame;
        multiplayManager.MatchFoundCallback += StartGame;
        multiplayManager.ExitRoomCallback += EndGame;
        multiplayManager.OpponentLeftCallback += EndGame;
    }

    private void OnDisable()
    {
        gameLogic.Cleanup();
        gameLogic.WinConditionChecked -= EndGame;
        multiplayManager.MatchFoundCallback -= StartGame;
        multiplayManager.ExitRoomCallback -= EndGame;
        multiplayManager.OpponentLeftCallback -= EndGame;
    }

    #endregion

    #region OnSceneLoad

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if(scene.name != "Game_Scene") return;
        
        if (GameModeManager.Mode == GameMode.SinglePlayer)
        {
            StartGame();
        }
    }

    #endregion

    private void StartGame()
    {
        if (currentGameState != GameState.Default) return;
        
        currentGameState = GameState.Playing;
        OnGameStart?.Invoke();
    }

    /// <summary>
    /// 항복 처리
    /// </summary>
    public void Surrender()
    {
        if (currentGameState != GameState.Playing) return;

        // 항복한 플레이어의 상대가 승리
        if (GameModeManager.Mode == GameMode.SinglePlayer)
        {
            PlayerType currentTurnPlayer = gameLogic.currentTurnPlayer;
            GameResult result = (currentTurnPlayer == PlayerType.Player1)
                ? GameResult.Player2Win
                : GameResult.Player1Win;

            Debug.Log($"{currentTurnPlayer}가 항복했습니다");
            EndGame(result);
        }
        else if (GameModeManager.Mode == GameMode.MultiPlayer)
        {
            Debug.Log("항복했습니다");
            EndGame(GameResult.Defeat);
        }
    }

    /// <summary>
    /// 게임 종료 처리
    /// </summary>
    private void EndGame(GameResult result)
    {
        if (currentGameState != GameState.Playing) return;

        string message = "게임 종료 ";
        switch (result)
        {
            case GameResult.None:
                message += "None";
                break;
            case GameResult.Player1Win:
                message += "Player1Win";
                break;
            case GameResult.Player2Win:
                message += "Player2Win";
                break;
            case GameResult.Victory:
                message += "Victory";
                break;
            case GameResult.Defeat:
                message += "Defeat";
                break;
            case GameResult.Draw:
                message += "Draw";
                break;
            case GameResult.Disconnect:
                message += "Disconnect";
                break;
        }

        Debug.Log(message);
        currentGameState = GameState.GameOver;
        OnGameEnd?.Invoke(result);
    }
}