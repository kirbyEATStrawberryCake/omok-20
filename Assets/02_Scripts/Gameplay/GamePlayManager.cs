using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public enum GameState
{
    Default, // 초기 상태
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
[RequireComponent(typeof(GameLogicController))]
public class GamePlayManager : Singleton<GamePlayManager>
{
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private GameSceneUIManager uiManager;
    [SerializeField] private MultiplayManager multiplayManager;
    [SerializeField] private GomokuAIDebugger gomokuAIDebugger;
    [SerializeField] private GameTimer gameTimer;
    public BoardManager BoardManager => boardManager;
    public GameSceneUIManager UIManager => uiManager;
    public MultiplayManager MultiplayManager => multiplayManager;
    public GomokuAIDebugger GomokuAIDebugger => gomokuAIDebugger;
    public GameTimer GameTimer => gameTimer;

    public RenjuRule RenjuRule { get; private set; } // 렌주룰 관리자 참조
    public GameLogicController GameLogicController { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private bool isRenjuModeEnabled = true; // 렌주룰 적용 여부

    public bool IsRenjuModeEnabled => isRenjuModeEnabled;
    private bool isAITurnHandled = false; // AI 의 턴이 실행됐는지

    [SerializeField] private bool showForbiddenPositions = true; // 금지 위치 표시 여부
    public bool ShowForbiddenPositions => showForbiddenPositions;

    public GameState currentGameState { get; private set; } = GameState.Default; // 현재 게임 상태

    public event UnityAction OnGameStart;
    public event UnityAction OnGameRestart;
    public event UnityAction OnSurrender;
    public event UnityAction<GameResult> OnGameEnd;

    #region 유니티 이벤트

    protected override void Awake()
    {
        base.Awake();
        GameLogicController = GetComponent<GameLogicController>();
        RenjuRule = GetComponent<RenjuRule>();
#if UNITY_EDITOR
        if (gameObject.scene.name == EditorSceneLoader.StartupSceneName)
        {
            Debug.Log($"<color=cyan>싱글모드 테스트 시작</color>");
            GameModeManager.Mode = GameMode.SinglePlayer;
            OnSceneLoad(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }
#endif
    }

    private void Start()
    {
        if (GameLogicController != null)
        {
            GameLogicController.Initialize(this);
            GameLogicController.WinConditionChecked += EndGame;
            OnGameStart += GameLogicController.RandomizePlayerStones;
            OnGameRestart += GameLogicController.ResetGame;
        }

        if (MultiplayManager.Instance != null && GameModeManager.Mode == GameMode.MultiPlayer)
        {
            MultiplayManager.Instance.MatchCallback += StartGame;
        }

        currentGameState = GameState.Default;

        if (GameModeManager.Mode == GameMode.SinglePlayer || GameModeManager.Mode == GameMode.AI)
        {
            StartGame();
        }
        // 멀티플레이는 MultiplayManager에서 담당
    }

    private void Update()
    {
        if (currentGameState != GameState.Playing) return;

        if (GameModeManager.Mode != GameMode.SinglePlayer && GameModeManager.Mode != GameMode.AI)
            return;

        // 현재 턴이 AI인지 확인
        if (IsCurrentTurnAI())
        {
            if (!isAITurnHandled)
            {
                HandleAITurn();
                isAITurnHandled = true;
            }
        }
        else
        {
            isAITurnHandled = false;
        }
    }

    private bool IsCurrentTurnAI()
    {
        if (GameModeManager.Mode == GameMode.AI)
        {
            return GameLogicController.GetCurrentTurnPlayer() == PlayerType.AI;
        }

        return false;
    }

    private void HandleAITurn()
    {
        if (!IsInvoking("ExecuteAITurn"))
        {
            Invoke("ExecuteAITurn", 1f);
        }
    }

    private void ExecuteAITurn()
    {
        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();

        Vector2Int aiMove = gomokuAIDebugger.GetNextMoveFromAI();
        boardManager.HandleBoardClick(aiMove.x, aiMove.y);
        boardManager.PlaceStone();

        watch.Stop();
        Debug.Log("AI 착수 시간: " + watch.ElapsedMilliseconds + "ms");
    }

    private void OnDisable()
    {
        if (GameLogicController != null)
        {
            GameLogicController.WinConditionChecked -= EndGame;
            OnGameStart += GameLogicController.RandomizePlayerStones;
            OnGameRestart += GameLogicController.ResetGame;
        }

        if (MultiplayManager.Instance != null && GameModeManager.Mode == GameMode.MultiPlayer)
        {
            MultiplayManager.Instance.MatchCallback -= StartGame;
        }
    }

    #endregion

    #region OnSceneLoad

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
    }

    #endregion

    private void StartGame()
    {
        if (currentGameState != GameState.Default) return;

        currentGameState = GameState.Playing;
        OnGameStart?.Invoke();
    }

    private void StartGame(MultiplayControllerState state)
    {
        if (state == MultiplayControllerState.MatchFound)
            StartGame();
        else if (state == MultiplayControllerState.MatchFailed)
            StartAIGame();
    }

    private void StartAIGame()
    {
        if (currentGameState != GameState.Default) return;

        if (MultiplayManager.Instance != null)
        {
            MultiplayManager.Instance.MatchCallback -= StartGame;
        }

        StartGame();
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
            PlayerType currentTurnPlayer = GameLogicController.GetCurrentTurnPlayer();
            GameResult result = (currentTurnPlayer == PlayerType.Player1)
                ? GameResult.Player2Win
                : GameResult.Player1Win;

            Debug.Log($"{currentTurnPlayer}가 항복했습니다");
            EndGame(result);
        }
        else if (GameModeManager.Mode == GameMode.MultiPlayer || GameModeManager.Mode == GameMode.AI)
        {
            Debug.Log("항복했습니다");
            OnSurrender?.Invoke();
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

    public void ResterGame()
    {
        if (currentGameState != GameState.GameOver) return;

        currentGameState = GameState.Default;
        OnGameRestart?.Invoke();
        StartGame();
    }

    /*private void Update()
    {
        // 게임 중이 아니면 무시
        if (currentGameState != GameState.Playing) return;

        // ���� ���� �÷��̾ AI���� Ȯ��
        if (IsCurrentTurnAI()) // AI 차례
        {
            if (!isAITurnHandled)// AI 턴을 처리하지 않았으면
            {
                // AI 턴 처리
                HandleAITurn();
                isAITurnHandled = true; // 체크
            }
        }
        else // 플레이어 차례
        {
            // ���콺 �Է� ó�� (BoardManager�� update ���� ó��)
            isAITurnHandled = false; // AI 턴 처리 체크해제
        }
    }

    /// <summary>
    /// ���� ���� �÷��̾ AI���� Ȯ��
    /// </summary>
    public bool IsCurrentTurnAI()
    {
        // 현재 차례가 AI 인지 체크
        return gameLogic.currentTurnPlayer == PlayerType.AI;
    }

    /// <summary>
    /// AI 착수 딜레이 주기
    /// </summary>
    private void HandleAITurn()
    {
        if (!IsInvoking("ExecuteAITurn"))
        {
            Invoke("ExecuteAITurn", 1f); // 1초의 딜레이로 AI가 생각하는 것 처럼 보이게 하기
        }
    }

    /// <summary>
    /// AI 의 착수를 실행
    /// </summary>
    private void ExecuteAITurn()
    {
        Stopwatch watch = new Stopwatch(); // Stopwatch ��ü ����
        watch.Start(); // ���� ����

        Vector2Int aiMove = gomokuAIDebugger.GetNextMoveFromAI(); // AI�� ������ ��ġ�� ��.

        // AI가 선택한 위치를 클릭.
        boardManager.HandleBoardClick(aiMove.x, aiMove.y);

        // 실제 착수 실행
        boardManager.PlaceStone();

        watch.Stop(); // ���� ����
        Debug.Log("�ڵ� ���� �ð�: " + watch.ElapsedMilliseconds + "ms"); // ��� �ð� ���
    }*/
}