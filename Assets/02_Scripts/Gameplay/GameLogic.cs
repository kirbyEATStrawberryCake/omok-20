using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static Constants;

public enum PlayerType
{
    Player1 = 1, // 로컬 플레이어 1
    Player2 = 2, // 로컬 플레이어 2
    Me,          // 멀티 플레이: 나
    Opponent,    // 멀티 플레이: 상대방
    AI
}

public class GameLogic
{
    private readonly RenjuRule renjuRule = new();
    public StoneType[,] board { get; private set; } // 오목판 배열 (논리적 보드)

    public StoneType currentStone { get; private set; }       // 현재 차례인 돌 타입 (항상 흑돌부터 시작)
    public PlayerType currentTurnPlayer { get; private set; } // 현재 턴인 플레이어 ID
    private PlayerType blackStonePlayer;                      // 흑돌을 가진 플레이어
    private PlayerType whiteStonePlayer;                      // 백돌을 가진 플레이어

    // 플레이어 변경 시 발생하는 이벤트
    public event UnityAction<StoneType, PlayerType> OnPlayerTurnChanged;
    public event UnityAction<PlayerType> WinConditionChecked;
    public event UnityAction<List<Vector2Int>> UpdateForbiddenPositions;

    /// <summary>
    /// 논리적 오목판 초기화
    /// </summary>
    public void InitializeBoard()
    {
        if (board == null)
            board = new StoneType[boardSize, boardSize];
        else
            ClearBoard();
    }

    /// <summary>
    /// 논리적 오목판 정리
    /// </summary>
    private void ClearBoard()
    {
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++) { board[x, y] = StoneType.None; }
        }
    }

    public void AssignPlayers(PlayerType blackPlayer, PlayerType whitePlayer)
    {
        blackStonePlayer = blackPlayer;
        whiteStonePlayer = whitePlayer;
    }

    public void StartFirstTurn()
    {
        currentStone = StoneType.Black;
        currentTurnPlayer = blackStonePlayer;
        OnPlayerTurnChanged?.Invoke(currentStone, currentTurnPlayer);
    }

    public bool CanPlaceStone(int x, int y)
    {
        // 이미 돌이 놓여있는지 검사
        if (board[x, y] != StoneType.None) return false;

        // 렌주룰 검사 (흑돌인 경우만)
        if (currentStone == StoneType.Black)
        {
            if (!renjuRule.IsValidMove(x, y, currentStone, board)) return false;
        }

        return true;
    }

    public void PlaceStone(int x, int y)
    {
        // 논리적 보드에 저장
        board[x, y] = currentStone;
        // 렌주룰에 따른 착수 금지 위치 업데이트
        UpdateForbiddenPositions?.Invoke(renjuRule.GetForbiddenPositions(board));

        // 승리 조건 확인
        for (int dir = 0; dir < 4; dir++)
        {
            int dx = directions[dir, 0];
            int dy = directions[dir, 1];

            int count = 1;
            count += renjuRule.CountConsecutiveStones(x, y, dx, dy, currentStone, board);
            count += renjuRule.CountConsecutiveStones(x, y, -dx, -dy, currentStone, board);

            if (count == 5 || (count >= 6 && currentStone == StoneType.Black))
            {
                WinConditionChecked?.Invoke(currentTurnPlayer);
                return;
            }
        }

        // 턴 넘기기
        SwitchPlayer();
    }

    /// <summary>
    /// 플레이어 턴 교체
    /// </summary>
    public void SwitchPlayer()
    {
        // 돌 색깔 변경
        currentStone = (currentStone == StoneType.Black) ? StoneType.White : StoneType.Black;

        // 현재 턴 플레이어 변경
        currentTurnPlayer = (currentStone == StoneType.Black) ? blackStonePlayer : whiteStonePlayer;
        Debug.Log($"현재 플레이어 {currentTurnPlayer}");

        OnPlayerTurnChanged?.Invoke(currentStone, currentTurnPlayer);
    }
}