using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public enum GameState
{
    Default,  // 초기 상태
    Playing,  // 게임 진행 중
    GameOver, // 게임 종료
    Paused    // 게임 일시정지
}

[RequireComponent(typeof(BoardInputHandler))]
[RequireComponent(typeof(BoardManager))]
public class GamePlayManager : Singleton<GamePlayManager>
{
    [SerializeField] private GomokuAIManager gomokuAIManager;
    [SerializeField] private GameTimer gameTimer;

    private MultiplayManager multiplayManager;
    private BoardManager boardManager;
    private BoardInputHandler boardInputHandler;
    private readonly GiboManager giboManager = new();
    private readonly GameLogic gameLogic = new();
    public GameLogic GameLogic => gameLogic;

    private bool hasPendingMove;    // 확정 대기 중인 착수가 있는지
    private Vector2Int pendingMove; // 확정 대기 중인 착수 위치

    public GameState currentGameState { get; private set; } = GameState.Default; // 현재 게임 상태

    #region 이벤트

    public event UnityAction OnGameStart;
    public event UnityAction OnGameRestart;
    public event UnityAction OnSurrender;
    public event UnityAction<GameResult> OnGameEnd;

    #endregion

    #region Unity Life Cycle

    protected override void Awake()
    {
        base.Awake();
        multiplayManager = MultiplayManager.Instance;
        boardManager = GetComponent<BoardManager>();
        boardInputHandler = GetComponent<BoardInputHandler>();
#if UNITY_EDITOR
        if (gameObject.scene.name == EditorSceneLoader.StartupSceneName)
        {
            Debug.Log($"<color=cyan>싱글모드 테스트 시작</color>");
            GameModeManager.Mode = GameMode.SinglePlayer;
            OnSceneLoad(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }
#endif
    }

    private void OnEnable()
    {
        if (gameLogic != null)
        {
            gameLogic.WinConditionChecked += EndGame;
            gameLogic.UpdateForbiddenPositions += HandleUpdateForbiddenPosition;
            gameLogic.OnPlayerTurnChanged += HandlePlayerTurnChanged;
        }

        if (boardInputHandler != null)
        {
            boardInputHandler.OnBoardHovered += HandleBoardHover;
            boardInputHandler.OnBoardClicked += HandleBoardClick;
        }

        if (gameTimer != null) { gameTimer.OnTimeUp += HandleTimeUp; }

        if (MultiplayManager.Instance != null && GameModeManager.Mode == GameMode.MultiPlayer)
        {
            MultiplayManager.Instance.MatchCallback += StartGame;
        }
    }

    private void Start()
    {
        currentGameState = GameState.Default;

        if (GameModeManager.Mode == GameMode.SinglePlayer || GameModeManager.Mode == GameMode.AI) { StartGame(); }
        // 멀티플레이는 MultiplayManager에서 담당
    }

    private void Update()
    {
        if (currentGameState != GameState.Playing) return;
        if (GameModeManager.Mode != GameMode.AI) return;

        // 현재 턴이 AI인지 확인
        if (IsCurrentTurnAI())
        {
            if (!isAITurnHandled)
            {
                HandleAITurn();
                isAITurnHandled = true;
            }
        }
        else { isAITurnHandled = false; }
    }


    private void OnDisable()
    {
        if (gameLogic != null)
        {
            gameLogic.WinConditionChecked -= EndGame;
            gameLogic.UpdateForbiddenPositions -= HandleUpdateForbiddenPosition;
            gameLogic.OnPlayerTurnChanged -= HandlePlayerTurnChanged;
        }

        if (boardInputHandler != null)
        {
            boardInputHandler.OnBoardHovered -= HandleBoardHover;
            boardInputHandler.OnBoardClicked -= HandleBoardClick;
        }

        if (gameTimer != null) { gameTimer.OnTimeUp -= HandleTimeUp; }

        if (MultiplayManager.Instance != null && GameModeManager.Mode == GameMode.MultiPlayer)
        {
            MultiplayManager.Instance.MatchCallback -= StartGame;
        }
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode) { }

    #endregion

    #region Public Methods

    /// <summary>
    /// 착수 버튼 클릭시 호출할 메소드
    /// </summary>
    public void OnGoStoneButtonClicked()
    {
        if (!IsMyTurn()) return;
        if (!hasPendingMove) return;

        boardManager.PlaceStoneVisual(pendingMove.x, pendingMove.y, gameLogic.currentStone);
        AddMoveToGibo(pendingMove.x, pendingMove.y);
        gameLogic.PlaceStone(pendingMove.x, pendingMove.y);
        if (GameModeManager.Mode == GameMode.MultiPlayer)
            MultiplayManager.Instance.GoStone(pendingMove.x, pendingMove.y);
        hasPendingMove = false;
        SoundManager.PlaySFX();
    }

    /// <summary>
    /// 멀티플레이 시 상대방이 착수 했을 때 서버에서 호출하는 메소드
    /// </summary>
    public void PlaceOpponentStone(int x, int y)
    {
        boardManager.PlaceStoneVisual(x, y, gameLogic.currentStone);
        AddMoveToGibo(x, y);
        gameLogic.PlaceStone(x, y);
        SoundManager.PlaySFX();
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
            PlayerType winner = (gameLogic.currentTurnPlayer == PlayerType.Player1)
                ? PlayerType.Player2
                : PlayerType.Player1;

            Debug.Log($"{gameLogic.currentTurnPlayer}가 항복했습니다");
            EndGame(winner);
        }
        else if (GameModeManager.Mode == GameMode.MultiPlayer || GameModeManager.Mode == GameMode.AI)
        {
            Debug.Log("항복했습니다");
            OnSurrender?.Invoke();
            EndGame(PlayerType.Opponent);
        }
    }

    #endregion

    #region 게임 흐름 관리

    /// <summary>
    /// 게임 시작
    /// </summary>
    private void StartGame()
    {
        if (currentGameState != GameState.Default) return;

        boardManager.InitializeBoard();
        SelectRandomBlackStonePlayer();
        gameLogic.StartFirstTurn();
        gameLogic.InitializeBoard();
        giboManager.StartNewRecord();
        gameTimer.StartTimer();
        currentGameState = GameState.Playing;
        OnGameStart?.Invoke();
    }

    /// <summary>
    /// 게임 시작 (멀티플레이용)
    /// </summary>
    private void StartGame(MultiplayControllerState state)
    {
        if (state == MultiplayControllerState.MatchFound)
            StartGame();
        else if (state == MultiplayControllerState.MatchFailed)
        {
            if (MultiplayManager.Instance != null) { MultiplayManager.Instance.MatchCallback -= StartGame; }

            // TODO : AI 시작 트리거
            StartGame();
        }
    }

    /// <summary>
    /// 게임 종료
    /// </summary>
    public void EndGame(PlayerType winner)
    {
        if (currentGameState != GameState.Playing) return;

        GameResult result = GameResult.None;

        string message = "게임 종료 ";
        switch (winner)
        {
            case PlayerType.Player1:
                message += "Player1Win";
                result = GameResult.Player1Win;
                break;
            case PlayerType.Player2:
                message += "Player2Win";
                result = GameResult.Player2Win;
                break;
            case PlayerType.Me:
                message += "Victory";
                result = GameResult.Victory;
                break;
            case PlayerType.Opponent:
            case PlayerType.AI:
                message += "Defeat";
                result = GameResult.Defeat;
                break;
        }

        Debug.Log(message);
        currentGameState = GameState.GameOver;
        giboManager.SaveCurrentRecord();
        gameTimer.StopTimer();
        if (result != GameResult.None)
            OnGameEnd?.Invoke(result);
    }

    /// <summary>
    /// 게임 재시작
    /// </summary>
    public void RestartGame()
    {
        if (currentGameState != GameState.GameOver) return;

        currentGameState = GameState.Default;
        OnGameRestart?.Invoke();
        StartGame();
    }

    #endregion

    #region 이벤트 핸들러

    /// <summary>
    /// 보드 위에 마우스가 올라가 있을 때 처리를 담당하는 메소드
    /// </summary>
    private void HandleBoardHover(Vector2Int pos)
    {
        if (currentGameState != GameState.Playing) return;
        if (GameModeManager.Mode == GameMode.MultiPlayer &&
            gameLogic.currentTurnPlayer == PlayerType.Opponent) return;
        if (GameModeManager.Mode == GameMode.AI &&
            gameLogic.currentTurnPlayer == PlayerType.AI) return;

        if (gameLogic.CanPlaceStone(pos.x, pos.y)) { boardManager.UpdateSelectedMarker(pos.x, pos.y); }
    }

    /// <summary>
    /// 마우스를 클릭 했을 때 처리를 담당하는 메소드
    /// </summary>
    private void HandleBoardClick(Vector2Int pos)
    {
        if (!IsMyTurn()) return;

        if (gameLogic.CanPlaceStone(pos.x, pos.y))
        {
            hasPendingMove = true;
            pendingMove = pos;
            boardManager.ShowPendingMove(pos.x, pos.y, gameLogic.currentStone);
        }
    }

    /// <summary>
    /// 금지 마크가 업데이트 되었을때 처리를 담당하는 메소드
    /// </summary>
    private void HandleUpdateForbiddenPosition(List<Vector2Int> pos) { boardManager.UpdateForbiddenMarker(pos); }

    /// <summary>
    /// 주어진 시간이 다 지났을 때 처리를 담당하는 메소드
    /// </summary>
    private void HandleTimeUp() { gameLogic.SwitchPlayer(); }

    /// <summary>
    /// 턴 변경 시 처리를 담당하는 메소드
    /// </summary>
    private void HandlePlayerTurnChanged(StoneType stone, PlayerType player)
    {
        boardManager.HideSelectedMakerAndPendingStone();
        gameTimer.ResetTimer();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// 내 턴인지 확인하는 메소드
    /// </summary>
    private bool IsMyTurn()
    {
        if (currentGameState != GameState.Playing) return false;
        if (GameModeManager.Mode == GameMode.MultiPlayer &&
            gameLogic.currentTurnPlayer == PlayerType.Opponent) return false;
        if (GameModeManager.Mode == GameMode.AI &&
            gameLogic.currentTurnPlayer == PlayerType.AI) return false;

        return true;
    }

    /// <summary>
    /// 시작 시 흑돌 플레이어를 랜덤하게 선택하는 메소드
    /// </summary>
    private void SelectRandomBlackStonePlayer()
    {
        if (GameModeManager.Mode == GameMode.MultiPlayer)
        {
            // 멀티플레이: 서버에서 받은 정보 사용
            bool amIFirstPlayer = multiplayManager.multiplayController.amIFirstPlayer;

            if (amIFirstPlayer)
            {
                gameLogic.AssignPlayers(PlayerType.Me, PlayerType.Opponent); // 내가 흑돌 (선공), 상대가 백돌
                Debug.Log("멀티플레이: 내가 선공 (흑돌)");
            }
            else
            {
                gameLogic.AssignPlayers(PlayerType.Opponent, PlayerType.Me); // 상대가 흑돌 (선공), 내가 백돌
                Debug.Log("멀티플레이: 내가 후공 (백돌)");
            }
        }
        else if (GameModeManager.Mode == GameMode.AI)
        {
            Debug.Log("AI 전환 모드: 플레이어 vs AI");

            if (Random.Range(0, 2) == 0)
            {
                gameLogic.AssignPlayers(PlayerType.Me, PlayerType.AI);
                Debug.Log("AI플레이: Player1이 선공 (흑돌)");
            }
            else
            {
                gameLogic.AssignPlayers(PlayerType.AI, PlayerType.Me);
                Debug.Log("AI플레이: AI가 선공 (흑돌)");
            }
        }
        else
        {
            // 싱글플레이 (로컬): 랜덤 방식
            if (Random.Range(0, 2) == 0)
            {
                gameLogic.AssignPlayers(PlayerType.Player1, PlayerType.Player2);
                Debug.Log("싱글플레이: Player1이 선공 (흑돌)");
            }
            else
            {
                gameLogic.AssignPlayers(PlayerType.Player2, PlayerType.Player1);
                Debug.Log("싱글플레이: Player2가 선공 (흑돌)");
            }
        }

        if (GameModeManager.Mode == GameMode.AI)
        {
            StoneType aiStoneType = (gameLogic.currentTurnPlayer == PlayerType.AI) ? StoneType.Black : StoneType.White;
            gomokuAIManager.InstantiateGomokuAI(aiStoneType);
        }
    }

    #endregion

    #region AI

    private bool isAITurnHandled = false; // AI 의 턴이 실행됐는지

    private bool IsCurrentTurnAI()
    {
        if (GameModeManager.Mode == GameMode.AI) { return gameLogic.currentTurnPlayer == PlayerType.AI; }

        return false;
    }

    private void HandleAITurn()
    {
        if (!IsInvoking("ExecuteAITurn")) { Invoke("ExecuteAITurn", 1f); }
    }

    private void ExecuteAITurn()
    {
        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();

        var aiResult = gomokuAIManager.GetAIResult(gameLogic.board);
        boardManager.PlaceStoneVisual(aiResult.bestMove.x, aiResult.bestMove.y, gameLogic.currentStone);
        AddMoveToGibo(aiResult.bestMove.x, aiResult.bestMove.y);
        gameLogic.PlaceStone(aiResult.bestMove.x, aiResult.bestMove.y);
        SoundManager.PlaySFX();

        watch.Stop();
        Debug.Log("AI 착수 시간: " + watch.ElapsedMilliseconds + "ms");
    }

    #endregion

    #region 기보

    private void AddMoveToGibo(int _x, int _y)
    {
        MoveData move = new MoveData
            { x = _x, y = _y, stoneColor = gameLogic.currentStone == StoneType.Black ? 1 : 2 };
        giboManager.AddMove(move);
    }

    #endregion
}