using System;
using System.Diagnostics;
using Unity.VisualScripting;
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
public class GamePlayManager : Singleton<GamePlayManager>
{
    public BoardManager boardManager { get; private set; } // 오목판 관리자 참조
    public GameLogic gameLogic { get; private set; } = new GameLogic(); // 플레이어 관리자 참조
    public RenjuRule renjuRule { get; private set; } // 렌주룰 관리자 참조
    public GameSceneUIManager uiManager => GameSceneUIManager.Instance;
    public MultiplayManager multiplayManager => MultiplayManager.Instance;
    
    public GomokuAIDebugger gomokuAIDebugger { get; private set; } // 오목 AI 디버거(착수 후보, 가중치 시각화) 참조

    [Header("Game Settings")]
    [SerializeField] private bool isRenjuModeEnabled = true; // 렌주룰 적용 여부

    public bool IsRenjuModeEnabled => isRenjuModeEnabled;
    private bool isAITurnHandled = false; // AI 의 턴이 실행됐는지

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
        gomokuAIDebugger = GameObject.FindFirstObjectByType<GomokuAIDebugger>();
        renjuRule = GetComponent<RenjuRule>();
    }

    private void Start()
    {
        // 에디터 테스트용
        // GameModeManager.Mode = GameMode.SinglePlayer;
        OnSceneLoad(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        // 에디터 테스트용
    }

    private void OnEnable()
    {
        gameLogic.Initialize();
        gameLogic.WinConditionChecked += EndGame;
        if (GameModeManager.Mode == GameMode.MultiPlayer)
        {
            multiplayManager.MatchFoundCallback += StartGame;
            multiplayManager.ExitRoomCallback += EndGame;
            multiplayManager.OpponentLeftCallback += EndGame;
        }
    }

    private void OnDisable()
    {
        gameLogic.Cleanup();
        gameLogic.WinConditionChecked -= EndGame;
        if (GameModeManager.Mode == GameMode.MultiPlayer)
        {
            multiplayManager.MatchFoundCallback -= StartGame;
            multiplayManager.ExitRoomCallback -= EndGame;
            multiplayManager.OpponentLeftCallback -= EndGame;
        }
    }

    #endregion

    #region OnSceneLoad

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        // 테스트 때문에 잠시 주석처리했습니다.
        // TODO : 주석처리 한것 해제해야 합니다
        //if(scene.name != "Game_Scene") return;
        

        if (GameModeManager.Mode == GameMode.SinglePlayer)
        {
            StartGame();
        }
        // 멀티플레이는 MultiplayManager에서 담당
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
    
    private void Update()
    {
        // ������ ���� ���� ���� �� ó��
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
        
        // 게임중이 아니면 무시
        if (currentGameState != GameState.Playing) return;
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
    }
}