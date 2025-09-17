using System;
using System.Collections.Generic;
using _02_Scripts.AI;
using UnityEngine;

public class GomokuAI
{
    private readonly BoardManager_AI board;
    private readonly StoneType myStone;
    private readonly StoneType enemyStone;
    
    private int searchDepth = 3;

    public int boardSize = 15;

    [SerializeField] private int collectRadius = 2; // 다음 수를 둘 빈칸과 돌이 있는 칸의 거리

    // 4방향 벡터를 담는 리스트
    private readonly Vector2Int[] directions =
    {
        new Vector2Int(1, 0),   // 가로
        new Vector2Int(0, 1),   // 세로
        new Vector2Int(1, 1),   // 대각 ↘
        new Vector2Int(1, -1)   // 대각 ↗
    };

    public GomokuAI(BoardManager_AI board, StoneType myStone)
    {
        this.board = board;
        this.myStone = myStone;
        enemyStone = (myStone == StoneType.Black) ? StoneType.White : StoneType.Black;
    }

    /// <summary>
    /// 다음 수를 선택하는 메서드. 게임플레이 구현되면 AI턴일때 AIState에서 이 함수를 호출한다.
    /// 가장 최적의 첫 수를 선택하는 역할을 한다.
    /// </summary>
    public Vector2Int GetNextMove()
    {
        // 1. 즉시 승리 수 체크
        Vector2Int? winMove = FindWinningMove(myStone);
        if (winMove.HasValue)
            return winMove.Value;

        // 2. 상대방 승리 차단
        Vector2Int? blockMove = FindWinningMove(enemyStone);
        if (blockMove.HasValue)
            return blockMove.Value;

        // 3. 후보 생성
        List<Vector2Int> candidateMoves = GetCandidateMoves(collectRadius);

        // 4. 보드가 완전히 비어있으면 중앙 반환
        if (candidateMoves.Count == 0)
            return new Vector2Int(boardSize / 2, boardSize / 2);

        // 5. 각 후보 평가
        int bestScore = int.MinValue;
        Vector2Int bestMove = new Vector2Int(-1, -1);
        
        foreach (var move in candidateMoves)
        {
            board.PlaceStone_Logical(move.x, move.y, myStone); // 미리 둬보기
            
            // Minimax 함수를 호출하여 이 수가 가져올 최종 점수를 계산
            // 최상위 레벨(루트 노드)에서의 탐색. 여기서의 alpha와 beta는 초기값
            int score = Minimax(board, searchDepth, int.MinValue, int.MaxValue, false);
            
            board.PlaceStone_Logical(move.x, move.y, StoneType.None); // 수 되돌리기

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }
        return bestMove;
    }

    /// <summary>
    /// 즉시 승리수를 찾는 메서드.
    /// 승리 조건 = 돌 5개 연속. 착수할 때 승리 가능한 좌표를 찾는 메서드.
    /// </summary>
    private Vector2Int? FindWinningMove(StoneType stone)
    {
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                if (board.IsEmpty(x, y))
                {
                    board.PlaceStone_Logical(x, y, stone);
                    bool isWin = CheckWin(x, y, stone);
                    board.PlaceStone_Logical(x, y, StoneType.None);
                
                    if (isWin)
                        return new Vector2Int(x, y);
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 승리 체크 메서드.
    /// </summary>
    private bool CheckWin(int x, int y, StoneType stone)
    {
        foreach (var dir in directions)
        {
            int count = 1;
            count += CountDirection(x, y, dir.x, dir.y, stone);
            count += CountDirection(x, y, -dir.x, -dir.y, stone);

            if (count >= 5)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 특정 위치에서 가로, 세로, 대각선을 검사해서 5목이 되는지 확인하는 메서드
    /// </summary>
    private int CountDirection(int x, int y, int dx, int dy, StoneType stone)
    {
        int count = 0;
        int nx = x + dx;
        int ny = y + dy;

        while (board.IsValidPosition(nx, ny) && board.GetStoneAt(nx, ny) == stone)
        {
            count++;
            nx += dx;
            ny += dy;
        }
        return count;
    }

    /// <summary>
    /// 후보 생성 메서드. 주변 돌이 있는 빈 칸만 수집 (radius로 수집 범위 조절)
    /// </summary>
    private List<Vector2Int> GetCandidateMoves(int collectRadius)
    {
        List<Vector2Int> list = new();
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                if (!board.IsEmpty(x, y)) continue;
                if (HasNeighborWithinRadius(x, y, collectRadius))
                    list.Add(new Vector2Int(x, y));
            }
        }
        return list;
    }

    /// <summary>
    /// 빈칸의 주변(Radius)에 돌이 있는 칸이 있는지 체크
    /// </summary>
    private bool HasNeighborWithinRadius(int x, int y, int collectRadius)
    {
        int xmin = Mathf.Max(0, x - collectRadius);
        int xmax = Mathf.Min(boardSize - 1, x + collectRadius);
        int ymin = Mathf.Max(0, y - collectRadius);
        int ymax = Mathf.Min(boardSize - 1, y + collectRadius);

        for (int i = xmin; i <= xmax; i++)
        for (int j = ymin; j <= ymax; j++)
            if (!board.IsEmpty(i, j))
                return true;

        return false;
    }

    /// <summary>
    /// 보드 전체를 평가하여 현재 게임 상태의 점수를 반환하는 메서드.
    /// 양쪽 플레이어의 패턴을 모두 고려한다.
    /// </summary>
    private int EvaluateBoard(BoardManager_AI board)
    {
        int myTotalScore = 0;
        int enemyTotalScore = 0;

        // 보드 전체를 순회하며 점수 계산
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                if (board.GetStoneAt(x, y) == myStone)
                {
                    // 나의 돌이 만드는 패턴 점수 합산
                    myTotalScore += EvaluatePatternsAt(x, y, myStone);
                }
                else if (board.GetStoneAt(x, y) == enemyStone)
                {
                    // 상대방의 돌이 만드는 패턴 점수 합산
                    enemyTotalScore += EvaluatePatternsAt(x, y, enemyStone);
                }
            }
        }

        // 나의 점수에서 상대방 점수를 빼서 최종 점수 반환
        // 상대방의 점수에 1.1 가중치를 주어 방어에 더 집중하게 만든다
        return myTotalScore - (int)(enemyTotalScore * 1.1f);
    }
    
    /// <summary>
    /// 주어진 좌표에서 특정 색의 패턴 점수를 합하는 메서드 (4방향)
    /// </summary>
    private int EvaluatePatternsAt(int x, int y, StoneType stone)
    {
        int sum = 0;
        foreach (var dir in directions)
            sum += EvaluateDirection(x, y, dir.x, dir.y, stone);
    
        return sum;
    }
    
    /// <summary>
    /// 한 방향의 수에 대한 평가 (기준 칸 포함)
    /// </summary>
    private int EvaluateDirection(int x, int y, int dx, int dy, StoneType stone)
    {
    // 기준칸을 포함한 연속 개수 계산
    int count = CountDirection(x, y, dx, dy, stone) +
                CountDirection(x, y, -dx, -dy, stone) + 1;
    
    // 양쪽 바로 옆 칸 좌표
    int lx = x - dx;
    int ly = y - dy;
    int rx = x + dx;
    int ry = y + dy;
    
    // 양쪽 옆칸의 상태 판정 (경계, 비어있음, 아군, 적군)
    bool leftOut = !board.IsValidPosition(lx, ly);
    bool rightOut = !board.IsValidPosition(rx, ry);
    
    StoneType leftStone = leftOut ? StoneType.None : board.GetStoneAt(lx, ly);
    StoneType rightStone = rightOut ? StoneType.None : board.GetStoneAt(rx, ry);
    
    bool leftIsEmpty = !leftOut && leftStone == StoneType.None;
    bool rightIsEmpty = !rightOut && rightStone == StoneType.None;
    
    bool leftIsFriend = !leftOut && leftStone == stone;
    bool rightIsFriend = !rightOut && rightStone == stone;
    
    bool leftIsEnemy = !leftOut && leftStone != StoneType.None && leftStone != StoneType.Error && leftStone != stone;
    bool rightIsEnemy = !rightOut && rightStone != StoneType.None && leftStone != StoneType.Error && rightStone != stone;
    
    // 경계 또는 적으로 막힌 경우는 '막힘'으로 처리
    bool leftBlocked = leftOut || leftIsEnemy;
    bool rightBlocked = rightOut || rightIsEnemy;
    
    // 양쪽 모두 막혔으면 '유일 경로(Unique Path)'로 간주
    if (leftBlocked && rightBlocked)
    {
        if (count >= 4) return 0; // 오버라인이거나 특수상황 — 점수 부여를 별도로 조정할 수도 있음
        if (count == 3) return GomokuWeights.THREE_UNIQUE_PATH;
        if (count == 2) return GomokuWeights.TWO_UNIQUE_PATH;
        if (count == 1) return GomokuWeights.ONE_UNIQUE_PATH;
        return 0;
    }
    
    // 4목(또는 그 이상)이고 적어도 한쪽 끝이 열려있다면 오픈포(Four open)
    if (count >= 4 && !(leftBlocked && rightBlocked))
    {
        return GomokuWeights.FOUR_OPEN;
    }
    
    bool anyEnemyAdjacent = leftIsEnemy || rightIsEnemy;
    bool anyFriendAdjacent = leftIsFriend || rightIsFriend;
    
    // 3목 평가
    if (count == 3)
    {
        if (!anyEnemyAdjacent)
        {
            // 적이 인접하지 않은 경우
            if (anyFriendAdjacent) return GomokuWeights.THREE_NO_ENEMY_HAS_FRIEND;
            return GomokuWeights.THREE_NO_ENEMY_NO_FRIEND;
        }
        else
        {
            // 적이 인접한 경우
            if (anyFriendAdjacent) return GomokuWeights.THREE_ENEMY_HAS_FRIEND;
            return GomokuWeights.THREE_ENEMY_NO_FRIEND;
        }
    }
    
    // 2목 평가
    if (count == 2)
    {
        if (!anyEnemyAdjacent)
        {
            if (anyFriendAdjacent) return GomokuWeights.TWO_NO_ENEMY_HAS_FRIEND;
            return GomokuWeights.TWO_NO_ENEMY_NO_FRIEND;
        }
        else
        {
            if (anyFriendAdjacent) return GomokuWeights.TWO_ENEMY_HAS_FRIEND;
            return GomokuWeights.TWO_ENEMY_NO_FRIEND;
        }
    }
    
    // 1목 평가
    if (count == 1)
    {
        if (!anyEnemyAdjacent)
        {
            if (anyFriendAdjacent) return GomokuWeights.ONE_NO_ENEMY_HAS_FRIEND;
            return GomokuWeights.ONE_NO_ENEMY_NO_FRIEND;
        }
        else
        {
            if (anyFriendAdjacent) return GomokuWeights.ONE_ENEMY_HAS_FRIEND;
            return GomokuWeights.ONE_ENEMY_NO_FRIEND;
        }
    }
    
    return 0;
    }
    
    private int Minimax(BoardManager_AI board, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        // 1. 종료 조건: 깊이 제한에 도달했거나, 게임이 끝났거나, 오목판이 다 찼으면 (더이상 둘 공간이 없으면)
        if (depth == 0 || GameManager.Instance.currentGameState == GameState.GameOver || board.IsBoardFull())
        {
            // 전체 보드의 형세를 평가
            return EvaluateBoard(board);
        }

        // 2. 후보 수 생성 (현재 보드 상태에서 둘 수 있는 모든 유효한 수를 수집. 빈칸 주변만 탐색)
        List<Vector2Int> candidateMoves = GetCandidateMoves(collectRadius);
        
        // 3. 내 턴 (최대화). 가능한 점수 중 최대를 선택.
        if (maximizingPlayer)
        {
            int maxEval = int.MinValue; // 가장 작은 값으로 초기화

            foreach (var move in candidateMoves) // 현재 상태에서 가능한 모든 다음 수들을 탐색(후보 수)
            {
                board.PlaceStone_Logical(move.x, move.y, myStone); // 수를 미리 둬본다.
                int eval = Minimax(board, depth - 1, alpha, beta, false); // 이 수에 대해 탐색을 진행한다. (다음 턴은 상대방에게 넘김)
                board.PlaceStone_Logical(move.x, move.y, StoneType.None); // 수를 되돌린다.

                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval); // alpha : 현재 플레이어 입장에서 보장받은 최댓값
                if (beta <= alpha) break; // 가지치기 (알파 ≥ 베타가 되면 그 이후는 볼 필요가 없다)
                //
            }
            return maxEval;
        }
        
        // 4. 상대 턴 (최소화)
        else // maximizingPlayer가 false
        {
            int minEval = int.MaxValue;

            foreach (var move in candidateMoves) // 현재 상태에서 가능한 모든 다음 수들을 탐색(후보 수)
            {
                board.PlaceStone_Logical(move.x, move.y, enemyStone); // 수를 미리 둬본다.
                int eval = Minimax(board, depth - 1, alpha, beta,true); // maximizingPlayer가 false였으므로, 다음 턴은 상대방.
                board.PlaceStone_Logical(move.x, move.y, StoneType.None); // 수를 되돌린다.
                
                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval); // beta : 상대 플레이어입장에서 보장받은 최솟값
                if (beta <= alpha) break; // 가지치기 (알파 ≥ 베타가 되면 그 이후는 볼 필요가 없다)
            }
            return minEval;
        }
    }
}

    // 미사용 메서드


    // /// <summary>
    // /// 한 수를 임시로 두고 평가. 금수(흑의 3x3/4x4)를 만들면 매우 낮은 점수 반환.
    // /// </summary>
    // private int EvaluateMove(int x, int y)
    // {
    //     if (!board.IsEmpty(x, y)) return int.MinValue;
    //
    //     board.TryPlaceStone(x, y, myStone);
    //
    //     if (CheckWin(x, y, myStone))
    //     {
    //         board.TryPlaceStone(x, y, StoneType.None);
    //         return GomokuWeights.FOUR_OPEN;
    //     }
    //
    //     if (myStone == StoneType.Black /*&& IsForbiddenByRenju(x, y)*/) // 검은돌이고, 렌주룰을 위반하는 경우 매우 낮은 점수 반환
    //     {
    //         board.TryPlaceStone(x, y, StoneType.None);
    //         return int.MinValue;
    //     }
    //
    //     int myScore = EvaluatePatternsAt(x, y, myStone);
    //     int enemyScore = EvaluatePatternsAt(x, y, enemyStone);
    //
    //     board.TryPlaceStone(x, y, StoneType.None);
    //
    //     return myScore - (int)(enemyScore * 1.1f);
    // }
    //


// /// <summary>
// /// 렌즈(금수)룰 단순 판정.
// /// </summary>
// private bool IsForbiddenByRenju(int x, int y)
// {
//     int openThreeCount = 0;
//     int fourCount = 0;
//
//     foreach (var d in directions)
//     {
//         int forward = CountDirection(x, y, d.x, d.y, StoneType.Black);
//         int back = CountDirection(x, y, -d.x, -d.y, StoneType.Black);
//         int contiguous = 1 + forward + back;
//
//         int fx = x + (forward + 1) * d.x;
//         int fy = y + (forward + 1) * d.y;
//         int bx = x - (back + 1) * d.x;
//         int by = y - (back + 1) * d.y;
//
//         bool forwardEmpty = board.IsValidPosition(fx, fy) && board.GetStoneAt(fx, fy) == StoneType.None;
//         bool backEmpty = board.IsValidPosition(bx, by) && board.GetStoneAt(bx, by) == StoneType.None;
//         int openEnds = (forwardEmpty ? 1 : 0) + (backEmpty ? 1 : 0); // 열려있는 위치 갯수 (0이면 다막힘, 1이면 한쪽만 열려있음, 2면 양쪽 다 열려있음)
//
//         if (contiguous >= 5) return true;
//         if (contiguous >= 4 && openEnds >= 1) fourCount++;
//         if (contiguous == 3 && openEnds == 2) openThreeCount++;
//     }
//
//     return openThreeCount >= 2 || fourCount >= 2;
// }

