using System;
using System.Collections.Generic;
using _02_Scripts.AI;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class GomokuAI
{
    private readonly BoardManager board;
    private readonly StoneType myStone;
    private readonly StoneType enemyStone;

    private float defensiveMultiplier = 1.2f;
    
    private int searchDepth = 2;

    public int boardSize = 15;

    [SerializeField] private int collectRadius = 1; // 다음 수를 둘 빈칸과 돌이 있는 칸의 거리
    
    private HashSet<Vector2Int> candidateMoves = new HashSet<Vector2Int>(); // 후보 착수 리스트
    
    // 4방향 벡터를 담는 리스트
    private readonly Vector2Int[] directions =
    {
        new Vector2Int(1, 0),   // 가로
        new Vector2Int(0, 1),   // 세로
        new Vector2Int(1, 1),   // 대각 ↘
        new Vector2Int(1, -1)   // 대각 ↗
    };
    
    // [디버그용] 착수 후보 시각화를 위한 변수들
    public GameObject debugTextPrefab; // 점수 표시용 텍스트 프리팹 (TMP)
    public GameObject DebugSelectPrefab; // 후보 위치 표시용 구체 프리팹
    private List<GameObject> debugObjects = new List<GameObject>(); // 생성된 디버그 오브젝트 리스트

    
    public GomokuAI(BoardManager board, StoneType myStone, GameObject debugTextPrefab, GameObject debugSelectPrefab)
    {
        this.board = board;
        this.myStone = myStone;
        enemyStone = (myStone == StoneType.Black) ? StoneType.White : StoneType.Black;
        this.debugTextPrefab = debugTextPrefab;
        this.DebugSelectPrefab = debugSelectPrefab;
        //board.OnStonePlaced += OnStonePlaced;
    }

    /// <summary>
    /// 다음 수를 선택하는 메서드. 게임플레이 구현되면 AI턴일때 AIState에서 이 함수를 호출한다.
    /// 가장 최적의 첫 수를 선택하는 역할을 한다.
    /// </summary>
    public Vector2Int GetNextMove()
    {
        // AI 턴 시작 시 기존 디버그 시각화 삭제
        ClearDebugVisuals();
        
        // 1. 즉시 승리 수 체크
        Vector2Int? winMove = FindWinningMove(myStone);
        if (winMove.HasValue)
        {
            Debug.Log("AI: 즉시 승리수를 찾았습니다!");
            
            // 선택된 수에 대한 시각화
            Vector3 pos = board.BoardToWorldPosition(winMove.Value.x, winMove.Value.y);
            GameObject winSphere = GameObject.Instantiate(DebugSelectPrefab, pos, Quaternion.identity);
            winSphere.GetComponent<Renderer>().material.color = Color.green; // 승리수는 초록색으로 표시
            debugObjects.Add(winSphere);
            
            return winMove.Value;
        }
        
        Debug.Log("AI: 즉시 승리수가 없습니다.");

        // 2. 상대방 승리 차단
        Vector2Int? blockMove = FindWinningMove(enemyStone);
        if (blockMove.HasValue)
        {
            Debug.Log("AI: 상대방의 즉시 승리수를 방어합니다.");
            
            // 선택된 수에 대한 시각화
            Vector3 pos = board.BoardToWorldPosition(blockMove.Value.x, blockMove.Value.y);
            GameObject blockSphere = GameObject.Instantiate(DebugSelectPrefab, pos, Quaternion.identity);
            blockSphere.GetComponent<Renderer>().material.color = Color.red; // 방어수는 빨간색으로 표시
            debugObjects.Add(blockSphere);
            
            return blockMove.Value;
        }

        Debug.Log("AI: 상대방의 즉시 승리수가 없습니다.");
        
        // 3. 후보 생성
        candidateMoves = GetCandidateMoves(collectRadius);
        Debug.Log($"AI: 착수 후보 위치 {candidateMoves.Count}개를 생성합니다.");
        
        // 후보 위치 시각화
        CreateDebugVisuals(candidateMoves);
        
        // 4. 보드가 완전히 비어있으면 중앙 반환
        if (candidateMoves.Count == 0)
        {
            Debug.Log("AI: 보드가 완전히 비어있어 중앙을 반환합니다.");
            return new Vector2Int(boardSize / 2, boardSize / 2);
        }
        

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

            // 이 부분에서 각 후보 수의 점수를 화면에 표시
            CreateScoreText(move, score);
            
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
            
            Debug.Log($"AI: 탐색중인 수: ({move.x}, {move.y}), 예측 점수: {score}");
        }
        
        // 최종 선택된 수에 대한 시각화
        Vector3 bestMovePos = board.BoardToWorldPosition(bestMove.x, bestMove.y);
        GameObject bestMoveSphere = GameObject.Instantiate(DebugSelectPrefab, bestMovePos, Quaternion.identity);
        bestMoveSphere.GetComponent<Renderer>().material.color = Color.yellow; // 최적의 수는 노란색으로 표시
        debugObjects.Add(bestMoveSphere);
        
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
    private HashSet<Vector2Int> GetCandidateMoves(int collectRadius)
    {
        // 매번 생성하는 대신, 유지되는 리스트를 사용
        // (알파베타 가지치기 효과 극대화를 위해 여기서 각 후보의 가치를 간단히 평가하여 정렬할 예정.)
        //return new HashSet<Vector2Int>(candidateMoves);
        
        HashSet<Vector2Int> hashSet = new();
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                if (!board.IsEmpty(x, y)) continue;
                if (HasNeighborWithinRadius(x, y, collectRadius))
                    hashSet.Add(new Vector2Int(x, y));
            }
        }
        return hashSet;
    }
    
/// <summary>
/// [미사용] 돌이 놓일 때마다 후보 리스트를 업데이트하는 메서드
/// </summary>
/// <param name="x"></param>
/// <param name="y"></param>
    public void OnStonePlaced(int x, int y)
    {
        // 1. 돌이 놓인 위치는 더 이상 후보가 아님
        candidateMoves.Remove(new Vector2Int(x, y));

        // 2. 새로 놓인 돌 주변의 빈 칸들을 새로운 후보로 추가
        int radius = collectRadius;
        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                if (i == 0 && j == 0) continue;
            
                int nx = x + i;
                int ny = y + j;

                if (board.IsValidPosition(nx, ny) && board.IsEmpty(nx, ny))
                {
                    candidateMoves.Add(new Vector2Int(nx, ny));
                    Debug.Log("후보 추가 완료!!");
                }
            }
        }
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
    private int EvaluateBoard(BoardManager board)
    {
        int myTotalScore = 0;
        int enemyTotalScore = 0;
        int totalScore = 0;
        
        // 보드 전체를 순회하며 점수 계산
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                // 각 위치를 시작점으로 하여 4방향의 라인을 평가
                totalScore += EvaluateLine(board, x, y, 1, 0);   // 가로 (→)
                totalScore += EvaluateLine(board, x, y, 0, 1);   // 세로 (↓)
                totalScore += EvaluateLine(board, x, y, 1, 1);   // 대각선 (↘)
                totalScore += EvaluateLine(board, x, y, 1, -1);  // 대각선 (↗)
            }
        }
        return totalScore;
    }
    
    
    /// <summary>
    /// 특정 시작점에서 한 방향의 라인(6칸)을 평가하여 점수를 반환하는 메서드. (개선된 버전)
    /// </summary>
    private int EvaluateLine(BoardManager board, int startX, int startY, int dx, int dy)
    {
        int myStones = 0;
        int enemyStones = 0;

        // 6칸짜리 창(Window)을 기준으로 라인을 분석한다.
        for (int i = 0; i < 6; i++)
        {
            int x = startX + i * dx;
            int y = startY + i * dy;

            // 창이 보드 밖으로 벗어나면 유효한 패턴을 형성할 수 없으므로 0점 처리
            if (!board.IsValidPosition(x, y))
            {
                return 0;
            }

            StoneType stone = board.GetStoneAt(x, y);
            if (stone == myStone)
                myStones++;
            else if (stone == enemyStone)
                enemyStones++;
        }

        // 한 라인에 나와 상대의 돌이 모두 있으면 해당 라인은 발전 가능성이 없으므로 0점 처리
        if (myStones > 0 && enemyStones > 0)
        {
            return 0;
        }

        // 6칸 중 빈칸의 개수
        int emptyCells = 6 - (myStones + enemyStones);
        

        if (myStones > 0)
        {
            return GetScoreForPattern(myStones, emptyCells);
        }
        else if (enemyStones > 0)
        {
            return (int)(-GetScoreForPattern(enemyStones, emptyCells) * defensiveMultiplier);
        }

        return 0;
    }
    
    /// <summary>
    /// 6칸 창을 기준으로 나의 돌 패턴 점수를 계산한다.
    /// </summary>
    private int GetScoreForPattern(int stoneCount, int emptyCount)
    {
        // 6칸 창에 돌과 빈칸 외 다른 것이 있으면(상대 돌) 이미 걸러졌어야 함
        if (stoneCount + emptyCount != 6) return 0;

        switch (stoneCount)
        {
            case 5: // 5목: 6칸 중 5개가 내 돌. (ex: OOOOO_)
                return GomokuWeights.FIVE;
            
            case 4: // 4목
                // 빈칸 2개 -> 열린 4 (ex: _OOOO_)
                if (emptyCount == 2) return GomokuWeights.FOUR_OPEN;
                // 빈칸 1개 -> 닫힌 4 (ex: XOOOO_)
                if (emptyCount == 1) return GomokuWeights.FOUR_SIDE_CLOSE; 
                break;
            
            case 3: // 3목
                // 빈칸 3개 -> 열린 3 (ex: _OOO__)
                if (emptyCount == 3) return GomokuWeights.THREE_NO_ENEMY_NO_FRIEND;
                // 빈칸 2개 -> 닫힌 3 (ex: XOOO__)
                if (emptyCount == 2) return GomokuWeights.THREE_UNIQUE_PATH;
                break;
                
            case 2: // 2목
                // 빈칸 4개 -> 열린 2 (ex: __OO__)
                if (emptyCount == 4) return GomokuWeights.TWO_NO_ENEMY_NO_FRIEND;
                // 빈칸 3개 -> 닫힌 2 (ex: X_OO__)
                if (emptyCount == 3) return GomokuWeights.TWO_UNIQUE_PATH;
                break;

            case 1: // 1목
                // 빈칸 5개 -> 열린 1 (ex: ___O__)
                if (emptyCount == 5) return GomokuWeights.ONE_NO_ENEMY_NO_FRIEND;
                break;
        }
        return 0;
    }

    private int Minimax(BoardManager board, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        // 1. 종료 조건: 깊이 제한에 도달했거나, 게임이 끝났거나, 오목판이 다 찼으면 (더이상 둘 공간이 없으면)
        if (depth == 0 || GamePlayManager.Instance.currentGameState == GameState.GameOver || board.IsBoardFull())
        {
            // 전체 보드의 형세를 평가
            return EvaluateBoard(board);
        }

        // 2. 후보 수 생성 (현재 보드 상태에서 둘 수 있는 모든 유효한 수를 수집. 빈칸 주변만 탐색)
        candidateMoves = GetCandidateMoves(collectRadius);
        
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

    #region 디버깅용 메서드
    private void CreateDebugVisuals(HashSet<Vector2Int> moves)
    {
        // 기존 디버그 오브젝트가 있다면 모두 제거
        ClearDebugVisuals();

        // 후보 위치에 구체 생성
        foreach (var move in moves)
        {
            Vector3 pos = board.BoardToWorldPosition(move.x, move.y);
            GameObject sphere = GameObject.Instantiate(DebugSelectPrefab, pos, Quaternion.identity);
            debugObjects.Add(sphere);
        }
    }

    private void CreateScoreText(Vector2Int move, int score)
    {
        // 점수 텍스트 생성
        Vector3 pos = board.BoardToWorldPosition(move.x, move.y);
        GameObject textObject = GameObject.Instantiate(debugTextPrefab, pos, Quaternion.identity);
        TextMeshPro tmp = textObject.GetComponent<TextMeshPro>();
        tmp.text = score.ToString();
        debugObjects.Add(textObject);
    }

    public void ClearDebugVisuals()
    {
        foreach (var obj in debugObjects)
        {
            GameObject.Destroy(obj);
        }
        debugObjects.Clear();
    }
    #endregion
}

