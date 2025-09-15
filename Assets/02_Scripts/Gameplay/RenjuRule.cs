using UnityEngine;

public class RenjuRule : MonoBehaviour
{
    [Header("Renju Rule Settings")]
    public bool enableForbiddenMoves = true;    // 금수 적용 여부
    public bool showForbiddenPositions = false; // 금수 위치 시각적 표시 여부

    private BoardManager boardManager;           // 보드 매니저 참조

    // 8방향 벡터 (상하좌우, 대각선)
    private readonly int[,] directions = {
        {-1, -1}, {-1, 0}, {-1, 1},  // 왼쪽 위, 위, 오른쪽 위
        {0, -1},           {0, 1},    // 왼쪽,     오른쪽
        {1, -1},  {1, 0},  {1, 1}     // 왼쪽 아래, 아래, 오른쪽 아래
    };

    void Start()
    {
        Initialize();
    }

    /// <summary>
    /// 렌주룰 초기화
    /// </summary>
    public void Initialize()
    {
        boardManager = FindObjectOfType<BoardManager>();
        if (boardManager == null)
        {
            Debug.LogError("BoardManager를 찾을 수 없습니다!");
        }

        Debug.Log("렌주룰이 초기화되었습니다.");
    }

    /// <summary>
    /// 유효한 수인지 검사 (렌주룰 적용)
    /// </summary>
    /// <param name="x">놓으려는 위치 x좌표</param>
    /// <param name="y">놓으려는 위치 y좌표</param>
    /// <param name="stoneType">놓으려는 돌의 타입</param>
    /// <param name="board">현재 보드 상태</param>
    /// <returns>유효한 수인지 여부</returns>
    public bool IsValidMove(int x, int y, StoneType stoneType, StoneType[,] board)
    {
        // 백돌은 렌주룰의 영향을 받지 않음
        if (stoneType != StoneType.Black) return true;

        // 금수가 비활성화된 경우 모든 수 허용
        if (!enableForbiddenMoves) return true;

        // 임시로 돌을 놓고 금수 검사
        board[x, y] = stoneType;

        bool isValid = true;

        // 삼삼 금수 검사
        if (IsDoubleThree(x, y, board))
        {
            isValid = false;
            Debug.Log($"삼삼 금수: ({x}, {y})");
        }

        // 사사 금수 검사
        if (isValid && IsDoubleFour(x, y, board))
        {
            isValid = false;
            Debug.Log($"사사 금수: ({x}, {y})");
        }

        // 장목 금수 검사 (6목 이상)
        if (isValid && IsOverline(x, y, board))
        {
            isValid = false;
            Debug.Log($"장목 금수: ({x}, {y})");
        }

        // 임시로 놓은 돌 제거
        board[x, y] = StoneType.None;

        return isValid;
    }

    /// <summary>
    /// 승리 조건 검사 (5목)
    /// </summary>
    /// <param name="x">마지막에 놓은 돌의 x좌표</param>
    /// <param name="y">마지막에 놓은 돌의 y좌표</param>
    /// <param name="stoneType">돌의 타입</param>
    /// <returns>승리했는지 여부</returns>
    public bool CheckWinCondition(int x, int y, StoneType stoneType)
    {
        StoneType[,] board = boardManager.GetBoardState();

        // 4방향(가로, 세로, 대각선 2개)으로 5목 검사
        for (int dir = 0; dir < 4; dir++)
        {
            int dx = directions[dir, 0];
            int dy = directions[dir, 1];

            int count = 1; // 현재 놓은 돌 포함

            // 한 방향으로 연속된 돌 개수 세기
            count += CountConsecutiveStones(x, y, dx, dy, stoneType, board);

            // 반대 방향으로 연속된 돌 개수 세기
            count += CountConsecutiveStones(x, y, -dx, -dy, stoneType, board);

            // 정확히 5개일 때 승리 (6개 이상은 흑돌의 경우 장목으로 패배)
            if (count == 5)
            {
                return true;
            }

            // 흑돌이 6개 이상 만들면 패배 (장목)
            if (count >= 6 && stoneType == StoneType.Black && enableForbiddenMoves)
            {
                Debug.Log("흑돌 장목으로 패배!");
                return false; // 실제로는 상대방 승리로 처리해야 함
            }
        }

        return false;
    }

    /// <summary>
    /// 삼삼 금수 검사
    /// </summary>
    private bool IsDoubleThree(int x, int y, StoneType[,] board)
    {
        int threeCount = 0;

        // 4방향에서 활삼(열린 3목) 검사
        for (int dir = 0; dir < 4; dir++)
        {
            int dx = directions[dir, 0];
            int dy = directions[dir, 1];

            if (IsOpenThree(x, y, dx, dy, board))
            {
                threeCount++;
            }
        }

        return threeCount >= 2; // 2개 이상의 활삼이 있으면 삼삼
    }

    /// <summary>
    /// 사사 금수 검사
    /// </summary>
    private bool IsDoubleFour(int x, int y, StoneType[,] board)
    {
        int fourCount = 0;

        // 4방향에서 4목 검사
        for (int dir = 0; dir < 4; dir++)
        {
            int dx = directions[dir, 0];
            int dy = directions[dir, 1];

            if (IsFour(x, y, dx, dy, board))
            {
                fourCount++;
            }
        }

        return fourCount >= 2; // 2개 이상의 4목이 있으면 사사
    }

    /// <summary>
    /// 장목(6목 이상) 검사
    /// </summary>
    private bool IsOverline(int x, int y, StoneType[,] board)
    {
        // 4방향에서 6개 이상 연속 검사
        for (int dir = 0; dir < 4; dir++)
        {
            int dx = directions[dir, 0];
            int dy = directions[dir, 1];

            int count = 1; // 현재 돌 포함
            count += CountConsecutiveStones(x, y, dx, dy, StoneType.Black, board);
            count += CountConsecutiveStones(x, y, -dx, -dy, StoneType.Black, board);

            if (count >= 6)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 활삼(열린 3목) 검사
    /// </summary>
    private bool IsOpenThree(int x, int y, int dx, int dy, StoneType[,] board)
    {
        int count = 1; // 현재 돌 포함

        // 한 방향으로 연속된 흑돌 세기
        count += CountConsecutiveStones(x, y, dx, dy, StoneType.Black, board);

        // 반대 방향으로 연속된 흑돌 세기
        count += CountConsecutiveStones(x, y, -dx, -dy, StoneType.Black, board);

        // 3개이고 양쪽 끝이 비어있어야 활삼
        if (count == 3)
        {
            // 양쪽 끝이 비어있는지 확인
            int frontX = x + dx * (CountConsecutiveStones(x, y, dx, dy, StoneType.Black, board) + 1);
            int frontY = y + dy * (CountConsecutiveStones(x, y, dx, dy, StoneType.Black, board) + 1);

            int backX = x - dx * (CountConsecutiveStones(x, y, -dx, -dy, StoneType.Black, board) + 1);
            int backY = y - dy * (CountConsecutiveStones(x, y, -dx, -dy, StoneType.Black, board) + 1);

            bool frontEmpty = IsValidPosition(frontX, frontY) && board[frontX, frontY] == StoneType.None;
            bool backEmpty = IsValidPosition(backX, backY) && board[backX, backY] == StoneType.None;

            return frontEmpty && backEmpty;
        }

        return false;
    }

    /// <summary>
    /// 4목 검사
    /// </summary>
    private bool IsFour(int x, int y, int dx, int dy, StoneType[,] board)
    {
        int count = 1; // 현재 돌 포함

        // 한 방향으로 연속된 흑돌 세기
        count += CountConsecutiveStones(x, y, dx, dy, StoneType.Black, board);

        // 반대 방향으로 연속된 흑돌 세기
        count += CountConsecutiveStones(x, y, -dx, -dy, StoneType.Black, board);

        return count == 4;
    }

    /// <summary>
    /// 특정 방향으로 연속된 동일한 돌의 개수를 세는 메서드
    /// </summary>
    /// <param name="startX">시작 x좌표</param>
    /// <param name="startY">시작 y좌표</param>
    /// <param name="dx">x방향 증가량</param>
    /// <param name="dy">y방향 증가량</param>
    /// <param name="stoneType">찾을 돌의 타입</param>
    /// <param name="board">보드 상태</param>
    /// <returns>연속된 돌의 개수</returns>
    private int CountConsecutiveStones(int startX, int startY, int dx, int dy, StoneType stoneType, StoneType[,] board)
    {
        int count = 0;
        int x = startX + dx;
        int y = startY + dy;

        while (IsValidPosition(x, y) && board[x, y] == stoneType)
        {
            count++;
            x += dx;
            y += dy;
        }

        return count;
    }

    /// <summary>
    /// 유효한 보드 위치인지 확인
    /// </summary>
    private bool IsValidPosition(int x, int y)
    {
        return boardManager.IsValidPosition(x, y);
    }

    /// <summary>
    /// 모든 금수 위치 반환 (AI나 힌트 시스템용)
    /// </summary>
    /// <param name="board">현재 보드 상태</param>
    /// <returns>금수 위치 리스트</returns>
    public System.Collections.Generic.List<Vector2Int> GetForbiddenPositions(StoneType[,] board)
    {
        var forbiddenPositions = new System.Collections.Generic.List<Vector2Int>();

        if (!enableForbiddenMoves) return forbiddenPositions;

        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                if (board[x, y] == StoneType.None)
                {
                    if (!IsValidMove(x, y, StoneType.Black, board))
                    {
                        forbiddenPositions.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        return forbiddenPositions;
    }

    /// <summary>
    /// 렌주룰 설정 변경
    /// </summary>
    /// <param name="enableForbidden">금수 규칙 활성화 여부</param>
    public void SetRenjuRuleEnabled(bool enableForbidden)
    {
        enableForbiddenMoves = enableForbidden;
        Debug.Log($"렌주룰 금수: {(enableForbidden ? "활성화" : "비활성화")}");
    }

    /// <summary>
    /// 특정 위치가 승부수인지 확인 (5목을 만들 수 있는 수)
    /// </summary>
    /// <param name="x">검사할 x좌표</param>
    /// <param name="y">검사할 y좌표</param>
    /// <param name="stoneType">돌 타입</param>
    /// <param name="board">보드 상태</param>
    /// <returns>승부수인지 여부</returns>
    public bool IsWinningMove(int x, int y, StoneType stoneType, StoneType[,] board)
    {
        // 임시로 돌을 놓고 승리 조건 검사
        board[x, y] = stoneType;
        bool isWinning = CheckWinCondition(x, y, stoneType);
        board[x, y] = StoneType.None; // 원상복구

        return isWinning;
    }
}
