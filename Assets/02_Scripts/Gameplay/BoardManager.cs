using UnityEngine;
using System.Collections.Generic;
using System;

public class BoardManager : MonoBehaviour
{
    [Header("Board Settings")]
    public GameObject stonePrefab;              // 돌 프리팹
    public Transform boardParent;               // 오목판 부모 오브젝트
    public int boardSize = 15;                  // 오목판 크기 (15x15)
    public float cellSize = 1.0f;               // 각 칸의 크기

    private StoneType[,] board;                 // 오목판 배열 (논리적 보드)
    private GameObject[,] stoneObjects;         // 돌 오브젝트 배열 (시각적 보드)
    private Stack<Move> moveHistory;            // 수 기록을 위한 스택

    // 돌이 놓였을 때 발생하는 이벤트
    public event Action<int, int, StoneType> OnStonePlace;

    /// <summary>
    /// 한 수를 나타내는 구조체
    /// </summary>
    [System.Serializable]
    public struct Move
    {
        public int x, y;                        // 돌의 위치
        public StoneType stoneType;             // 돌의 종류
        public GameObject stoneObject;          // 돌 오브젝트 참조

        public Move(int x, int y, StoneType type, GameObject obj)
        {
            this.x = x;
            this.y = y;
            this.stoneType = type;
            this.stoneObject = obj;
        }
    }

    void Start()
    {
        InitializeBoard();
    }

    /// <summary>
    /// 오목판 초기화
    /// </summary>
    public void InitializeBoard()
    {
        // 배열 초기화
        board = new StoneType[boardSize, boardSize];
        stoneObjects = new GameObject[boardSize, boardSize];
        moveHistory = new Stack<Move>();

        // 기존 돌들 제거
        ClearBoard();

        // 마우스 클릭 감지를 위한 콜라이더 설정
        SetupBoardCollider();

        Debug.Log("오목판이 초기화되었습니다.");
    }

    /// <summary>
    /// 오목판의 모든 돌 제거
    /// </summary>
    private void ClearBoard()
    {
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                board[x, y] = StoneType.None;

                if (stoneObjects[x, y] != null)
                {
                    DestroyImmediate(stoneObjects[x, y]);
                    stoneObjects[x, y] = null;
                }
            }
        }
        moveHistory.Clear();
    }

    /// <summary>
    /// 보드 클릭 감지를 위한 콜라이더 설정
    /// </summary>
    private void SetupBoardCollider()
    {
        BoxCollider boardCollider = GetComponent<BoxCollider>();
        if (boardCollider == null)
        {
            boardCollider = gameObject.AddComponent<BoxCollider>();
        }

        // 콜라이더 크기를 보드 크기에 맞춤
        boardCollider.size = new Vector3(boardSize * cellSize, 0.1f, boardSize * cellSize);
    }

    /// <summary>
    /// 마우스 클릭 처리
    /// </summary>
    void OnMouseDown()
    {
        // 게임 매니저에서 게임 상태 확인
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager.GetGameState() != GameState.Playing) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // 클릭한 위치를 보드 좌표로 변환
            Vector3 localPos = transform.InverseTransformPoint(hit.point);
            int x = Mathf.RoundToInt(localPos.x / cellSize + (boardSize - 1) / 2.0f);
            int y = Mathf.RoundToInt(localPos.z / cellSize + (boardSize - 1) / 2.0f);

            // 현재 플레이어의 돌 타입 가져오기
            PlayerManager playerManager = FindObjectOfType<PlayerManager>();
            StoneType currentStone = playerManager.GetCurrentPlayer();

            // 돌 놓기 시도
            TryPlaceStone(x, y, currentStone);
        }
    }

    /// <summary>
    /// 지정한 위치에 돌 놓기 시도
    /// </summary>
    /// <param name="x">x좌표</param>
    /// <param name="y">y좌표</param>
    /// <param name="stoneType">놓을 돌의 타입</param>
    /// <returns>돌이 성공적으로 놓였는지 여부</returns>
    public bool TryPlaceStone(int x, int y, StoneType stoneType)
    {
        // 범위 검사
        if (!IsValidPosition(x, y)) return false;

        // 이미 돌이 놓여있는지 검사
        if (board[x, y] != StoneType.None) return false;

        // 렌주룰 검사 (흑돌인 경우만)
        RenjuRule renjuRule = FindObjectOfType<RenjuRule>();
        GameManager gameManager = FindObjectOfType<GameManager>();

        if (gameManager.isRenjuModeEnabled && stoneType == StoneType.Black)
        {
            if (!renjuRule.IsValidMove(x, y, stoneType, board))
            {
                Debug.Log("렌주룰 위반으로 해당 위치에 돌을 놓을 수 없습니다.");
                return false;
            }
        }

        // 돌 놓기
        PlaceStone(x, y, stoneType);
        return true;
    }

    /// <summary>
    /// 실제로 돌을 놓는 메서드
    /// </summary>
    /// <param name="x">x좌표</param>
    /// <param name="y">y좌표</param>
    /// <param name="stoneType">돌의 타입</param>
    private void PlaceStone(int x, int y, StoneType stoneType)
    {
        // 논리적 보드에 돌 정보 저장
        board[x, y] = stoneType;

        // 시각적 돌 오브젝트 생성
        Vector3 worldPos = GetWorldPosition(x, y);
        GameObject stoneObj = Instantiate(stonePrefab, worldPos, Quaternion.identity, boardParent);

        // 돌 컴포넌트 설정
        Stone stone = stoneObj.GetComponent<Stone>();
        if (stone != null)
        {
            stone.SetStoneType(stoneType);
            stone.SetPosition(x, y);
        }

        stoneObjects[x, y] = stoneObj;

        // 수 기록 저장
        moveHistory.Push(new Move(x, y, stoneType, stoneObj));

        // 이벤트 발생
        OnStonePlace?.Invoke(x, y, stoneType);

        Debug.Log($"돌이 놓였습니다: ({x}, {y}) - {stoneType}");
    }

    /// <summary>
    /// 마지막에 놓은 돌 제거 (무르기)
    /// </summary>
    public void UndoLastMove()
    {
        if (moveHistory.Count > 0)
        {
            Move lastMove = moveHistory.Pop();

            // 논리적 보드에서 제거
            board[lastMove.x, lastMove.y] = StoneType.None;

            // 시각적 오브젝트 제거
            if (lastMove.stoneObject != null)
            {
                DestroyImmediate(lastMove.stoneObject);
            }

            stoneObjects[lastMove.x, lastMove.y] = null;

            Debug.Log($"돌이 제거되었습니다: ({lastMove.x}, {lastMove.y})");
        }
    }

    /// <summary>
    /// 보드 좌표를 월드 좌표로 변환
    /// </summary>
    private Vector3 GetWorldPosition(int x, int y)
    {
        float worldX = (x - (boardSize - 1) / 2.0f) * cellSize;
        float worldZ = (y - (boardSize - 1) / 2.0f) * cellSize;
        return transform.position + new Vector3(worldX, 0, worldZ);
    }

    /// <summary>
    /// 유효한 위치인지 검사
    /// </summary>
    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < boardSize && y >= 0 && y < boardSize;
    }

    /// <summary>
    /// 지정한 위치의 돌 타입 반환
    /// </summary>
    public StoneType GetStoneAt(int x, int y)
    {
        if (!IsValidPosition(x, y)) return StoneType.None;
        return board[x, y];
    }

    /// <summary>
    /// 현재 보드 상태 반환 (복사본)
    /// </summary>
    public StoneType[,] GetBoardState()
    {
        StoneType[,] boardCopy = new StoneType[boardSize, boardSize];
        Array.Copy(board, boardCopy, board.Length);
        return boardCopy;
    }
}
