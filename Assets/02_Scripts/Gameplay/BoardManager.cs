using UnityEngine;
using System.Collections.Generic;
using System;

public class BoardManager : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite boardSprite;                  // 오목판 스프라이트
    public Sprite blackStoneSprite;             // 흑돌 스프라이트
    public Sprite whiteStoneSprite;             // 백돌 스프라이트
    public Sprite forbiddenMarkerSprite;        // 금지 위치 마커 스프라이트
    public Sprite selectedMarkerSprite;         // 선택한 위치 표시 마커 스프라이트
    public Sprite lastMoveMarkerSprite;         // 마지막 착수 위치 표시 마커 스프라이트
    public Sprite pendingMoveSprite;            // 착수 대기 표시 스프라이트

    [Header("Board Settings")]
    public int boardSize = 15;                  // 오목판 크기 (15x15)
    public float cellSize = 0.494f;               // 각 칸의 크기
    public Vector2 boardOffset = Vector2.zero;  // 보드 오프셋

    [Header("Marker Settings")]
    public float markerAlpha = 0.7f;            // 마커 투명도
    public Color forbiddenColor = Color.red;    // 금지 위치 색상
    public Color selectedColor = Color.yellow;   // 선택 위치 색상
    public Color lastMoveColor = Color.green;   // 마지막 수 색상
    public Color pendingMoveColor = Color.cyan; // 착수 대기 색상

    protected StoneType[,] board;                 // 오목판 배열 (논리적 보드)
    private GameObject[,] stoneObjects;         // 돌 오브젝트 배열
    private GameObject boardObject;             // 보드 스프라이트 오브젝트

    // 매니저 참조 (효율적인 접근)
    private GameManager gameManager;
    private RenjuRule renjuRule;

    // 마커 관리
    private List<GameObject> forbiddenMarkers;  // 금지 위치 마커들
    private GameObject selectedMarker;          // 현재 선택된 위치 마커
    private GameObject lastMoveMarker;          // 마지막 수 마커
    private GameObject pendingMoveMarker;       // 착수 대기 마커

    // 마우스 위치 추적
    private Vector2Int hoveredPosition = new Vector2Int(-1, -1);

    // 위치가 선택되었을 때 발생하는 이벤트
    public event Action<int, int> OnPositionSelected;

    /// <summary>
    /// GameManager 참조 설정
    /// </summary>
    public void SetGameManager(GameManager manager)
    {
        gameManager = manager;
        renjuRule = gameManager.renjuRule; // GameManager를 통해 RenjuRule 참조 획득
    }

    void Start()
    {
        InitializeBoard();
    }

    void Update()
    {
        HandleMouseInput();
    }

    /// <summary>
    /// 오목판 초기화
    /// </summary>
    public void InitializeBoard()
    {
        // 배열 초기화
        board = new StoneType[boardSize, boardSize];
        stoneObjects = new GameObject[boardSize, boardSize];
        forbiddenMarkers = new List<GameObject>();

        // 기존 오브젝트들 제거
        ClearBoard();

        // 보드 스프라이트 생성
        CreateBoardSprite();

        Debug.Log("오목판이 초기화되었습니다.");
    }

    /// <summary>
    /// 보드 스프라이트 생성
    /// </summary>
    private void CreateBoardSprite()
    {
        if (boardObject != null)
        {
            DestroyImmediate(boardObject);
        }

        boardObject = new GameObject("Board");
        boardObject.transform.SetParent(transform);
        boardObject.transform.localPosition = Vector3.zero;

        SpriteRenderer boardRenderer = boardObject.AddComponent<SpriteRenderer>();
        boardRenderer.sprite = boardSprite;
        boardRenderer.sortingOrder = 0; // 가장 뒤에 렌더링

        // 보드 크기 조정
        // 보드 스프라이트는 원본 크기 유지 (격자는 별도로 계산)
        boardObject.transform.localScale = Vector3.one;
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

        // 마커들 제거
        HideAllMarkers();
    }

    /// <summary>
    /// 마우스 입력 처리
    /// </summary>
    private void HandleMouseInput()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector2Int boardPos = WorldToBoardPosition(mouseWorldPos);

        // 마우스 호버 처리
        if (IsValidPosition(boardPos.x, boardPos.y) && boardPos != hoveredPosition)
        {
            hoveredPosition = boardPos;
            UpdateSelectedMarker(boardPos.x, boardPos.y);
        }

        // 클릭 처리
        if (Input.GetMouseButtonDown(0))
        {
            HandleBoardClick(boardPos.x, boardPos.y);
        }
    }

    /// <summary>
    /// 보드 클릭 처리
    /// </summary>
    private void HandleBoardClick(int x, int y)
    {
        if (gameManager.GetGameState() != GameState.Playing) return;

        if (IsValidPosition(x, y))
        {
            // 위치 선택 이벤트 발생
            OnPositionSelected?.Invoke(x, y);
        }
    }

    /// <summary>
    /// 지정한 위치에 돌을 놓을 수 있는지 검사
    /// </summary>
    /// <param name="x">x좌표</param>
    /// <param name="y">y좌표</param>
    /// <param name="stoneType">놓을 돌의 타입</param>
    /// <returns>돌을 놓을 수 있는지 여부</returns>
    public bool CanPlaceStone(int x, int y, StoneType stoneType)
    {
        // 범위 검사
        if (!IsValidPosition(x, y)) return false;

        // 이미 돌이 놓여있는지 검사
        if (board[x, y] != StoneType.None) return false;

        // 렌주룰 검사 (흑돌인 경우만)
        if (gameManager.isRenjuModeEnabled && stoneType == StoneType.Black)
        {
            if (!renjuRule.IsValidMove(x, y, stoneType, board))
            {
                Debug.Log("렌주룰 위반으로 해당 위치에 돌을 놓을 수 없습니다.");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 실제로 돌을 놓는 메서드
    /// </summary>
    /// <param name="x">x좌표</param>
    /// <param name="y">y좌표</param>
    /// <param name="stoneType">돌의 타입</param>
    /// <returns>돌이 성공적으로 놓였는지 여부</returns>
    public bool PlaceStone(int x, int y, StoneType stoneType)
    {
        if (!CanPlaceStone(x, y, stoneType)) return false;

        // 논리적 보드에 돌 정보 저장
        board[x, y] = stoneType;

        // 시각적 돌 스프라이트 생성
        Vector3 worldPos = BoardToWorldPosition(x, y);
        GameObject stoneObj = new GameObject($"Stone_{x}_{y}");
        stoneObj.transform.SetParent(transform);
        stoneObj.transform.position = worldPos;

        SpriteRenderer stoneRenderer = stoneObj.AddComponent<SpriteRenderer>();
        stoneRenderer.sprite = (stoneType == StoneType.Black) ? blackStoneSprite : whiteStoneSprite;
        stoneRenderer.sortingOrder = 1; // 보드보다 앞에 렌더링

        // 돌 컴포넌트 설정
        Stone stone = stoneObj.AddComponent<Stone>();
        stone.SetStoneType(stoneType);
        stone.SetPosition(x, y);

        stoneObjects[x, y] = stoneObj;

        // 마지막 수 마커 업데이트
        UpdateLastMoveMarker(x, y);

        Debug.Log($"돌이 놓였습니다: ({x}, {y}) - {stoneType}");
        return true;
    }

    /// <summary>
    /// 착수 대기 표시
    /// </summary>
    public void ShowPendingMove(int x, int y, StoneType stoneType)
    {
        HidePendingMove();

        if (pendingMoveMarker == null)
        {
            pendingMoveMarker = new GameObject("PendingMoveMarker");
            pendingMoveMarker.transform.SetParent(transform);

            SpriteRenderer renderer = pendingMoveMarker.AddComponent<SpriteRenderer>();
            renderer.sprite = (stoneType == StoneType.Black) ? blackStoneSprite : whiteStoneSprite;
            renderer.sortingOrder = 4;

            Color color = pendingMoveColor;
            color.a = markerAlpha;
            renderer.color = color;
        }
        else
        {
            // 돌 색상에 맞게 스프라이트 변경
            SpriteRenderer renderer = pendingMoveMarker.GetComponent<SpriteRenderer>();
            renderer.sprite = (stoneType == StoneType.Black) ? blackStoneSprite : whiteStoneSprite;
        }

        pendingMoveMarker.transform.position = BoardToWorldPosition(x, y);
        pendingMoveMarker.SetActive(true);
    }

    /// <summary>
    /// 착수 대기 표시 숨기기
    /// </summary>
    public void HidePendingMove()
    {
        if (pendingMoveMarker != null)
        {
            pendingMoveMarker.SetActive(false);
        }
    }

    /// <summary>
    /// 선택된 위치 마커 업데이트
    /// </summary>
    private void UpdateSelectedMarker(int x, int y)
    {
        if (gameManager.GetGameState() != GameState.Playing) return;

        // 이미 돌이 놓인 위치는 마커 표시하지 않음
        if (board[x, y] != StoneType.None)
        {
            HideSelectedMarker();
            return;
        }

        if (selectedMarker == null)
        {
            selectedMarker = new GameObject("SelectedMarker");
            selectedMarker.transform.SetParent(transform);

            SpriteRenderer renderer = selectedMarker.AddComponent<SpriteRenderer>();
            renderer.sprite = selectedMarkerSprite;
            renderer.sortingOrder = 5;

            Color color = selectedColor;
            color.a = markerAlpha;
            renderer.color = color;
        }

        selectedMarker.transform.position = BoardToWorldPosition(x, y);
        selectedMarker.SetActive(true);
    }

    /// <summary>
    /// 선택된 위치 마커 숨기기
    /// </summary>
    private void HideSelectedMarker()
    {
        if (selectedMarker != null)
        {
            selectedMarker.SetActive(false);
        }
    }

    /// <summary>
    /// 마지막 수 마커 업데이트
    /// </summary>
    private void UpdateLastMoveMarker(int x, int y)
    {
        if (lastMoveMarker == null)
        {
            lastMoveMarker = new GameObject("LastMoveMarker");
            lastMoveMarker.transform.SetParent(transform);

            SpriteRenderer renderer = lastMoveMarker.AddComponent<SpriteRenderer>();
            renderer.sprite = lastMoveMarkerSprite;
            renderer.sortingOrder = 2;

            Color color = lastMoveColor;
            color.a = markerAlpha;
            renderer.color = color;
        }

        lastMoveMarker.transform.position = BoardToWorldPosition(x, y);
        lastMoveMarker.SetActive(true);
    }

    /// <summary>
    /// 마지막 수 마커 숨기기
    /// </summary>
    private void HideLastMoveMarker()
    {
        if (lastMoveMarker != null)
        {
            lastMoveMarker.SetActive(false);
        }
    }

    /// <summary>
    /// 금지 위치 마커들 업데이트
    /// </summary>
    public void UpdateForbiddenPositions()
    {
        HideForbiddenMarkers();

        if (renjuRule == null) return;

        var forbiddenPositions = renjuRule.GetForbiddenPositions(board);

        foreach (var pos in forbiddenPositions)
        {
            CreateForbiddenMarker(pos.x, pos.y);
        }
    }

    /// <summary>
    /// 금지 위치 마커 생성
    /// </summary>
    private void CreateForbiddenMarker(int x, int y)
    {
        GameObject marker = new GameObject($"ForbiddenMarker_{x}_{y}");
        marker.transform.SetParent(transform);
        marker.transform.position = BoardToWorldPosition(x, y);

        SpriteRenderer renderer = marker.AddComponent<SpriteRenderer>();
        renderer.sprite = forbiddenMarkerSprite;
        renderer.sortingOrder = 3;

        Color color = forbiddenColor;
        color.a = markerAlpha;
        renderer.color = color;

        forbiddenMarkers.Add(marker);
    }

    /// <summary>
    /// 금지 위치 마커들 숨기기
    /// </summary>
    public void HideForbiddenMarkers()
    {
        foreach (GameObject marker in forbiddenMarkers)
        {
            if (marker != null)
            {
                DestroyImmediate(marker);
            }
        }
        forbiddenMarkers.Clear();
    }

    /// <summary>
    /// 모든 마커 숨기기
    /// </summary>
    public void HideAllMarkers()
    {
        HideSelectedMarker();
        HideLastMoveMarker();
        HideForbiddenMarkers();
        HidePendingMove();
    }

    /// <summary>
    /// 보드 좌표를 월드 좌표로 변환
    /// </summary>
    private Vector3 BoardToWorldPosition(int x, int y)
    {
        float worldX = (x - (boardSize - 1) / 2.0f) * cellSize + boardOffset.x;
        float worldY = ((boardSize - 1) / 2.0f - y) * cellSize + boardOffset.y; // Y축 반전
        return transform.position + new Vector3(worldX, worldY, 0);
    }

    /// <summary>
    /// 월드 좌표를 보드 좌표로 변환
    /// </summary>
    private Vector2Int WorldToBoardPosition(Vector3 worldPos)
    {
        Vector3 localPos = worldPos - transform.position;
        int x = Mathf.RoundToInt((localPos.x - boardOffset.x) / cellSize + (boardSize - 1) / 2.0f);
        int y = Mathf.RoundToInt((boardSize - 1) / 2.0f - (localPos.y - boardOffset.y) / cellSize); // Y축 반전
        return new Vector2Int(x, y);
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