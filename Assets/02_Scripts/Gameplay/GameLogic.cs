using UnityEngine;
using UnityEngine.Events;

public enum PlayerType
{
    Player1 = 1, // 로컬 플레이어 1
    Player2 = 2, // 로컬 플레이어 2
    Me, // 멀티 플레이: 나
    Opponent, // 멀티 플레이: 상대방
    AI
}

public class GameLogic
{
    private readonly int[,] directions =
    {
        { -1, -1 }, { -1, 0 }, { -1, 1 },
        { 0, -1 }, { 0, 1 },
        { 1, -1 }, { 1, 0 }, { 1, 1 }
    };

    private GamePlayManager gamePlayManager;
    private BoardManager boardManager;
    private GomokuAIDebugger gomokuAIDebugger;
    private RenjuRule renjuRule;
    private MultiplayManager multiplayManager;
    private GameTimer gameTimer;
    public StoneType currentStone { get; private set; } // 현재 차례인 돌 타입 (항상 흑돌부터 시작)
    private PlayerType blackStonePlayer; // 흑돌을 가진 플레이어
    private PlayerType whiteStonePlayer; // 백돌을 가진 플레이어
    public PlayerType currentTurnPlayer { get; private set; } // 현재 턴인 플레이어 ID

    // 플레이어 변경 시 발생하는 이벤트
    public event UnityAction<StoneType, PlayerType> OnPlayerTurnChanged;
    public event UnityAction<GameResult> WinConditionChecked;

    // 이벤트 구독을 위한 초기화 메서드
    public void Initialize(GamePlayManager gamePlayManager)
    {
        // GomokuAI에게 StoneType을 반환하기 위해 작성했습니다.
        // 멀티플레이일 때는 실행되지 않고, 싱글플레이일 때만 실행하도록 조건문 처리가 필요합니다.
        // if (GameModeManager.Mode == GameMode.SinglePlayer)
        // {
        //     StoneType aiStone = blackStonePlayer == PlayerType.AI ? StoneType.Black : StoneType.White;
        //     gomokuAIDebugger.InstantiateGomokuAI(boardManager, aiStone);
        // }
        // Debug.Log("boardManager" + boardManager);
        this.gamePlayManager = gamePlayManager;
        this.boardManager = gamePlayManager.BoardManager;
        this.gomokuAIDebugger = gamePlayManager.GomokuAIDebugger;
        this.renjuRule = gamePlayManager.RenjuRule;
        this.gameTimer = gamePlayManager.GameTimer;

        this.multiplayManager = MultiplayManager.Instance;
        if (boardManager != null)
        {
            boardManager.OnPlaceStone += CheckWinCondition;
            boardManager.OnPlaceStone += SwitchPlayer;
        }

        if (gameTimer != null)
        {
            gameTimer.OnTimeUp += ForceSkipTurn;
        }
    }

    // 이벤트 구독 해제를 위한 정리 메서드
    public void Cleanup()
    {
        if (gamePlayManager != null)
        {
            gamePlayManager.OnGameStart -= RandomizePlayerStones;
            gamePlayManager.OnGameRestart -= ResetGame;
        }

        if (boardManager != null)
        {
            boardManager.OnPlaceStone -= CheckWinCondition;
            boardManager.OnPlaceStone -= SwitchPlayer;
        }

        if (gameTimer != null)
        {
            gameTimer.OnTimeUp -= ForceSkipTurn;
        }
    }

    /// <summary>
    /// 플레이어별 돌 색깔을 랜덤으로 할당하고 게임 시작
    /// </summary>
    public void RandomizePlayerStones()
    {
        if (GameModeManager.Mode == GameMode.MultiPlayer)
        {
            // 멀티플레이: 서버에서 받은 정보 사용
            bool amIFirstPlayer = multiplayManager.multiplayController.amIFirstPlayer;

            if (amIFirstPlayer)
            {
                blackStonePlayer = PlayerType.Me; // 내가 흑돌 (선공)
                whiteStonePlayer = PlayerType.Opponent; // 상대가 백돌
                Debug.Log("멀티플레이: 내가 선공 (흑돌)");
            }
            else
            {
                blackStonePlayer = PlayerType.Opponent; // 상대가 흑돌 (선공)
                whiteStonePlayer = PlayerType.Me; // 내가 백돌
                Debug.Log("멀티플레이: 내가 후공 (백돌)");
            }
        }
        else if (GameModeManager.Mode == GameMode.AI)
        {
            Debug.Log("AI 전환 모드: 플레이어 vs AI");

            if (Random.Range(0, 2) == 0)
            {
                blackStonePlayer = PlayerType.Me;
                whiteStonePlayer = PlayerType.AI;
                Debug.Log("AI플레이: Player1이 선공 (흑돌)");
            }
            else
            {
                blackStonePlayer = PlayerType.AI;
                whiteStonePlayer = PlayerType.Me;
                Debug.Log("AI플레이: AI가 선공 (흑돌)");
            }
        }
        else
        {
            // 싱글플레이 (로컬): 랜덤 방식
            if (Random.Range(0, 2) == 0)
            {
                blackStonePlayer = PlayerType.Player1;
                whiteStonePlayer = PlayerType.Player2;
                Debug.Log("싱글플레이: Player1이 선공 (흑돌)");
            }
            else
            {
                blackStonePlayer = PlayerType.Player2;
                whiteStonePlayer = PlayerType.Player1;
                Debug.Log("싱글플레이: Player2가 선공 (흑돌)");
            }
        }

        // 오목은 항상 흑돌부터 시작
        currentStone = StoneType.Black;
        currentTurnPlayer = blackStonePlayer;

        // AI 초기화 (플레이어 할당 완료 후)
        if (blackStonePlayer == PlayerType.AI || whiteStonePlayer == PlayerType.AI)
        {
            StoneType aiStoneType = (blackStonePlayer == PlayerType.AI) ? StoneType.Black : StoneType.White;
            gomokuAIDebugger.InstantiateGomokuAI(aiStoneType);
        }


        OnPlayerTurnChanged?.Invoke(currentStone, currentTurnPlayer);
    }

    /// <summary>
    /// 플레이어 턴 교체 (흑돌 -> 백돌 -> 흑돌 순서)
    /// </summary>
    private void SwitchPlayer(int x, int y)
    {
        // 돌 색깔 변경
        currentStone = (currentStone == StoneType.Black) ? StoneType.White : StoneType.Black;

        // 현재 턴 플레이어 변경
        currentTurnPlayer = (currentStone == StoneType.Black) ? blackStonePlayer : whiteStonePlayer;
        Debug.Log($"현재 플레이어 {currentTurnPlayer}");

        OnPlayerTurnChanged?.Invoke(currentStone, currentTurnPlayer); // 이벤트 발생
    }

    private void CheckWinCondition(int x, int y)
    {
        StoneType[,] board = boardManager.GetBoardState();

        for (int dir = 0; dir < 4; dir++)
        {
            int dx = directions[dir, 0];
            int dy = directions[dir, 1];

            int count = 1; // ���� ���� �� ����

            // �� �������� ���ӵ� �� ���� ����
            count += renjuRule.CountConsecutiveStones(x, y, dx, dy, currentStone, board);

            // �ݴ� �������� ���ӵ� �� ���� ����
            count += renjuRule.CountConsecutiveStones(x, y, -dx, -dy, currentStone, board);

            // ��Ȯ�� 5���� �� �¸� (6�� �̻��� �浹�� ��� ������� �й�)
            if (count == 5)
            {
                GameResult result = DetermineWinResult(currentStone);
                WinConditionChecked?.Invoke(result);
                return;
            }

            // �浹�� 6�� �̻� ����� �й� (���)
            if (count >= 6 && currentStone == StoneType.Black && gamePlayManager.IsRenjuModeEnabled)
            {
                // 흑돌 오버라인 체크
                GameResult result = DetermineOverlineResult();
                WinConditionChecked?.Invoke(result);
                return;
            }
        }
    }

    private GameResult DetermineWinResult(StoneType winnerStone)
    {
        PlayerType winnerPlayer = (winnerStone == StoneType.Black) ? blackStonePlayer : whiteStonePlayer;

        if (GameModeManager.Mode == GameMode.MultiPlayer || GameModeManager.Mode == GameMode.AI)
        {
            // 멀티플레이: 내가 이겼는지 확인
            return (winnerPlayer == PlayerType.Me) ? GameResult.Victory : GameResult.Defeat;
        }
        else
        {
            // 싱글플레이: Player1/Player2 구분
            return (winnerPlayer == PlayerType.Player1) ? GameResult.Player1Win : GameResult.Player2Win;
        }
    }

    private GameResult DetermineOverlineResult()
    {
        // 흑돌 오버라인 = 백돌 승리
        if (GameModeManager.Mode == GameMode.MultiPlayer)
        {
            return (whiteStonePlayer == PlayerType.Me) ? GameResult.Victory : GameResult.Defeat;
        }
        else
        {
            return (whiteStonePlayer == PlayerType.Player1) ? GameResult.Player1Win : GameResult.Player2Win;
        }
    }

    public void ResetGame()
    {
        currentStone = StoneType.Black;
        currentTurnPlayer = blackStonePlayer;
    }

    private void ForceSkipTurn()
    {
        SwitchPlayer(-1, -1);
    }
}