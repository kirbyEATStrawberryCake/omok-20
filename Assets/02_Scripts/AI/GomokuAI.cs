using System;
using System.Collections.Generic;
using _02_Scripts.AI;
using UnityEngine;

public class GomokuAI
{
    private readonly GomokuBoard board;
    private readonly Stone myStone;
    private readonly Stone enemyStone;
    
    private int searchDepth = 3;

    [SerializeField] private int collectRadius = 2; // 다음 수를 둘 빈칸과 돌이 있는 칸의 거리

    // 4방향 벡터를 담는 리스트
    private readonly Vector2Int[] directions =
    {
        new Vector2Int(1, 0),   // 가로
        new Vector2Int(0, 1),   // 세로
        new Vector2Int(1, 1),   // 대각 ↘
        new Vector2Int(1, -1)   // 대각 ↗
    };

    public GomokuAI(GomokuBoard board, Stone myStone)
    {
        this.board = board;
        this.myStone = myStone;
        enemyStone = (myStone == Stone.Black) ? Stone.White : Stone.Black;
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
            return new Vector2Int(GomokuBoard.SIZE / 2, GomokuBoard.SIZE / 2);

        // 5. 각 후보 평가
        int bestScore = int.MinValue;
        Vector2Int bestMove = new Vector2Int(-1, -1);
        
        foreach (var move in candidateMoves)
        {
            board.PlaceStone(move.x, move.y, myStone);
            
            // Minimax 함수를 호출하여 이 수가 가져올 최종 점수를 계산
            // 최상위 레벨(루트 노드)에서의 탐색. 여기서의 alpha와 beta는 초기값
            int score = Minimax(board, searchDepth, int.MinValue, int.MaxValue, false);
            board.PlaceStone(move.x, move.y, Stone.Empty);

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
    private Vector2Int? FindWinningMove(Stone stone)
    {
        for (int x = 0; x < GomokuBoard.SIZE; x++)
        {
            for (int y = 0; y < GomokuBoard.SIZE; y++)
            {
                if (board.IsEmpty(x, y))
                {
                    board.PlaceStone(x, y, stone);
                    bool isWin = CheckWin(x, y, stone);
                    board.PlaceStone(x, y, Stone.Empty);

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
    private bool CheckWin(int x, int y, Stone stone)
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
    private int CountDirection(int x, int y, int dx, int dy, Stone stone)
    {
        int count = 0;
        int nx = x + dx;
        int ny = y + dy;

        while (IsInside(nx, ny) && board.GetStone(nx, ny) == stone)
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
        for (int x = 0; x < GomokuBoard.SIZE; x++)
        {
            for (int y = 0; y < GomokuBoard.SIZE; y++)
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
        int xmax = Mathf.Min(GomokuBoard.SIZE - 1, x + collectRadius);
        int ymin = Mathf.Max(0, y - collectRadius);
        int ymax = Mathf.Min(GomokuBoard.SIZE - 1, y + collectRadius);

        for (int i = xmin; i <= xmax; i++)
        for (int j = ymin; j <= ymax; j++)
            if (!board.IsEmpty(i, j))
                return true;

        return false;
    }

    /// <summary>
    /// 한 수를 임시로 두고 평가. 금수(흑의 3x3/4x4)를 만들면 매우 낮은 점수 반환.
    /// </summary>
    private int EvaluateMove(int x, int y)
    {
        if (!board.IsEmpty(x, y)) return int.MinValue;

        board.PlaceStone(x, y, myStone);

        if (CheckWin(x, y, myStone))
        {
            board.PlaceStone(x, y, Stone.Empty);
            return GomokuWeights.FOUR_OPEN;
        }

        if (myStone == Stone.Black && IsForbiddenByRenju(x, y))
        {
            board.PlaceStone(x, y, Stone.Empty);
            return int.MinValue;
        }

        int myScore = EvaluatePatternsAt(x, y, myStone);
        int enemyScore = EvaluatePatternsAt(x, y, enemyStone);

        board.PlaceStone(x, y, Stone.Empty);

        return myScore - (int)(enemyScore * 1.1f);
    }

    /// <summary>
    /// 주어진 좌표에서 특정 색의 패턴 점수를 합하는 메서드 (4방향)
    /// </summary>
    private int EvaluatePatternsAt(int x, int y, Stone stone)
    {
        int sum = 0;
        foreach (var dir in directions)
            sum += EvaluateDirection(x, y, dir.x, dir.y, stone);

        return sum;
    }

    /// <summary>
    /// 한 방향의 수에 대한 평가 (기준 칸 포함)
    /// </summary>
    private int EvaluateDirection(int x, int y, int dx, int dy, Stone stone)
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
    bool leftOut = !IsInside(lx, ly);
    bool rightOut = !IsInside(rx, ry);

    Stone leftStone = leftOut ? Stone.Empty : board.GetStone(lx, ly);
    Stone rightStone = rightOut ? Stone.Empty : board.GetStone(rx, ry);

    bool leftIsEmpty = !leftOut && leftStone == Stone.Empty;
    bool rightIsEmpty = !rightOut && rightStone == Stone.Empty;

    bool leftIsFriend = !leftOut && leftStone == stone;
    bool rightIsFriend = !rightOut && rightStone == stone;

    bool leftIsEnemy = !leftOut && leftStone != Stone.Empty && leftStone != stone;
    bool rightIsEnemy = !rightOut && rightStone != Stone.Empty && rightStone != stone;

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

    /// <summary>
    /// 해당 좌표(칸)가 보드 안에 있는지 체크하는 메서드.
    /// </summary>
    private bool IsInside(int x, int y)
    {
        return x >= 0 && x < GomokuBoard.SIZE &&
               y >= 0 && y < GomokuBoard.SIZE;
    }

    
    /// <summary>
    /// 렌즈(금수)룰 단순 판정.
    /// </summary>
    private bool IsForbiddenByRenju(int x, int y)
    {
        int openThreeCount = 0;
        int fourCount = 0;

        foreach (var d in directions)
        {
            int forward = CountDirection(x, y, d.x, d.y, Stone.Black);
            int back = CountDirection(x, y, -d.x, -d.y, Stone.Black);
            int contiguous = 1 + forward + back;

            int fx = x + (forward + 1) * d.x;
            int fy = y + (forward + 1) * d.y;
            int bx = x - (back + 1) * d.x;
            int by = y - (back + 1) * d.y;

            bool forwardEmpty = IsInside(fx, fy) && board.GetStone(fx, fy) == Stone.Empty;
            bool backEmpty = IsInside(bx, by) && board.GetStone(bx, by) == Stone.Empty;
            int openEnds = (forwardEmpty ? 1 : 0) + (backEmpty ? 1 : 0);

            if (contiguous >= 5) return true;
            if (contiguous >= 4 && openEnds >= 1) fourCount++;
            if (contiguous == 3 && openEnds == 2) openThreeCount++;
        }

        return openThreeCount >= 2 || fourCount >= 2;
    }

    private int Minimax(GomokuBoard board, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        // 1. 종료 조건: 게임이 끝났거나, 깊이 제한에 도달했을 때
        if (depth == 0 /* || 게임오버 */)
        {
            // 여기서 전체 보드를 평가하는 함수가 필요하다.
            //return EvaluateBoard(board, maximizingPlayer ? myStone : enemyStone);
        }

        // 2. 후보 수 생성 (현재 보드 상태에서 둘 수 있는 모든 유효한 수를 수집. 빈칸 주변만 탐색)
        List<Vector2Int> candidateMoves = GetCandidateMoves(collectRadius);
        
        // 3. 내 턴 (최대화). 가능한 점수 중 최대를 선택.
        if (maximizingPlayer)
        {
            int maxEval = int.MinValue; // 가장 작은 값으로 초기화

            foreach (var move in candidateMoves) // 현재 상태에서 가능한 모든 다음 수들을 탐색(후보 수)
            {
                board.PlaceStone(move.x, move.y, myStone); // 수를 미리 둬본다.
                int eval = Minimax(board, depth - 1, alpha, beta, false); // 이 수에 대해 탐색을 진행한다. (다음 턴은 상대방에게 넘김)
                board.PlaceStone(move.x, move.y, Stone.Empty); // 수를 되돌린다.

                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);
                if (beta <= alpha) break; // 가지치기
            }
            return maxEval;
        }
        
        // 4. 상대 턴 (최소화)
        else // maximizingPlayer가 false
        {
            int minEval = int.MaxValue;

            foreach (var move in candidateMoves) // 현재 상태에서 가능한 모든 다음 수들을 탐색(후보 수)
            {
                board.PlaceStone(move.x, move.y, enemyStone); // 수를 미리 둬본다.
                int eval = Minimax(board, depth - 1, alpha, beta,true); // maximizingPlayer가 false였으므로, 다음 턴은 상대방.
                board.PlaceStone(move.x, move.y, Stone.Empty); // 수를 되돌린다.
                
                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);
                if (beta <= alpha) break; // 가지치기
            }
            return minEval;
        }
    }
}
