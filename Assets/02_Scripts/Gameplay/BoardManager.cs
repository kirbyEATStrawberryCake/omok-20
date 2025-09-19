using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.Multiplayer.Playmode;
using UnityEngine.Events;

public class BoardManager : MonoBehaviour
{
    private GamePlayManager gamePlayManager => GamePlayManager.Instance;
    private RenjuRule renjuRule => gamePlayManager.renjuRule;
    private GameLogic gameLogic => gamePlayManager.gameLogic;
    private GameSceneUIManager uiManager => GameSceneUIManager.Instance;

    [Header("Sprites")]
    [SerializeField] private Sprite blackStoneSprite; // 흑돌 스프라이트

    [SerializeField] private Sprite whiteStoneSprite; // 백돌 스프라이트

    [Header("Board Settings")]
    [SerializeField] private float cellSize = 0.494f; // 각 칸의 크기

    public int boardSize { get; private set; } = 15; // 오목판 크기 (15x15)
    [SerializeField] private Vector2 boardOffset = Vector2.zero; // 보드 오프셋

    [Header("Maker Object")]
    [SerializeField] private GameObject selectedMarker; // 현재 선택된 위치 마커

    [SerializeField] private GameObject lastMoveMarker; // 마지막 수 마커
    [SerializeField] private GameObject pendingMoveStone; // 착수 대기 마커

    [Header("Marker Settings")]
    [SerializeField] private GameObject stonePrefab_White;

    [SerializeField] private GameObject stonePrefab_Black;
    [SerializeField] private Transform stoneParent; // 돌을 생성할 부모 오브젝트
    [Space(10)] [SerializeField] private GameObject forbiddenMarkerPrefab; // 금지 마크 프리팹
    [SerializeField] private Transform forbiddenMarkerParent; // 금지 마크를 생성할 부모 오브젝트

    protected StoneType[,] board; // 오목판 배열 (논리적 보드)
    private GameObject[,] stoneObjects; // 돌 오브젝트 배열

    private Vector2Int pendingMove; // 확정 대기 중인 착수 위치
    private bool hasPendingMove; // 확정 대기 중인 착수가 있는지
    private SpriteRenderer pendingMoveStoneRenderer; // 착수 대기 마커 렌더러

    private List<GameObject> forbiddenMarkers; // 금지 위치 마커들

    // 마우스 위치 추적
    private Camera mainCamera;
    private Vector2Int hoveredPosition = new Vector2Int(-1, -1);

    // 위치가 선택되었을 때 발생하는 이벤트
    public event UnityAction<int, int> OnPlaceStone;

    #region 유니티 이벤트

    private void Awake()
    {
        mainCamera = Camera.main;
        pendingMoveStoneRenderer = pendingMoveStone.GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        gamePlayManager.OnGameStart += InitializeBoard;
        gamePlayManager.OnGameEnd += HideAllMarkers;
    }

    private void OnDisable()
    {
        gamePlayManager.OnGameStart -= InitializeBoard;
        gamePlayManager.OnGameEnd -= HideAllMarkers;
    }

    void Update()
    {
        if (gamePlayManager.currentGameState != GameState.Playing) return;

        HandleMouseInput();
    }

    #endregion

    #region 초기화

    /// <summary>
    /// 오목판 초기화
    /// </summary>
    private void InitializeBoard()
    {
        // 배열 초기화
        board = new StoneType[boardSize, boardSize];
        stoneObjects = new GameObject[boardSize, boardSize];
        forbiddenMarkers = new List<GameObject>();

        pendingMove = Vector2Int.zero;
        hasPendingMove = false;

        // 기존 오브젝트들 제거
        ClearBoard();
    }

    /// <summary>
    /// 오목판 정리
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
                    Destroy(stoneObjects[x, y]);
                    stoneObjects[x, y] = null;
                }
            }
        }

        HideAllMarkers();
    }

    /// <summary>
    /// 마커들 제거
    /// </summary>
    private void HideAllMarkers(GameResult result = GameResult.None)
    {
        hasPendingMove = false;
        selectedMarker?.SetActive(false); // 선택된 위치 마커 숨기기
        lastMoveMarker?.SetActive(false); // 마지막 수 마커 숨기기
        pendingMoveStone?.SetActive(false); // 착수 대기 표시 숨기기
        HideForbiddenMarkers(); // 금지 위치 마커들 숨기기
    }

    /// <summary>
    /// 금지 위치 마커들 숨기기
    /// </summary>
    private void HideForbiddenMarkers()
    {
        foreach (GameObject marker in forbiddenMarkers)
        {
            if (marker != null)
            {
                Destroy(marker);
            }
        }

        forbiddenMarkers.Clear();
    }

    #endregion

    #region 마우스 입력 및 착수 대기

    /// <summary>
    /// 마우스 입력 처리
    /// </summary>
    private void HandleMouseInput()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
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
    /// 선택된 위치 마커 업데이트
    /// </summary>
    private void UpdateSelectedMarker(int x, int y)
    {
        if (gamePlayManager.currentGameState != GameState.Playing) return;

        // 이미 돌이 놓인 위치는 마커 표시하지 않음
        if (board[x, y] != StoneType.None)
        {
            selectedMarker?.SetActive(false);
            return;
        }

        selectedMarker.transform.position = BoardToWorldPosition(x, y);
        selectedMarker?.SetActive(true);
    }

    /// <summary>
    /// 보드 클릭 처리
    /// </summary>
    public void HandleBoardClick(int x, int y)
    {
        if (gamePlayManager.currentGameState != GameState.Playing) return;
        if (!IsValidPosition(x, y)) return;
        if (GamePlayManager.Instance.IsCurrentTurnAI())
        {
            Debug.Log("AI 차례일 때는 플레이어의 마우스 입력을 무시합니다.");
            // 여기에 AI 차례일때는 플레이어의 마우스 입력을 무시하도록 하는 내용이 필요합니다..!
        }

        // 해당 위치에 돌을 놓을 수 있는지 검사
        if (CanPlaceStone(x, y))
        {
            pendingMove = new Vector2Int(x, y);
            hasPendingMove = true;
            ShowPendingMove(x, y); // 예상 돌 위치 표시
        }
        else
        {
            // 잘못된 위치 선택시 대기 상태 해제
            hasPendingMove = false;
            selectedMarker?.SetActive(false);
        }
    }

    /// <summary>
    /// 지정한 위치에 돌을 놓을 수 있는지 검사
    /// </summary>
    /// <param name="x">x좌표</param>
    /// <param name="y">y좌표</param>
    /// <returns>돌을 놓을 수 있는지 여부</returns>
    private bool CanPlaceStone(int x, int y)
    {
        // 범위 검사
        if (!IsValidPosition(x, y)) return false;

        // 이미 돌이 놓여있는지 검사
        if (board[x, y] != StoneType.None) return false;

        // 렌주룰 검사 (흑돌인 경우만)
        if (gamePlayManager.IsRenjuModeEnabled && gameLogic.currentStone == StoneType.Black)
        {
            if (!renjuRule.IsValidMove(x, y, gameLogic.currentStone, board))
            {
                uiManager.OpenOneConfirmButtonPopup("렌주룰 위반으로 해당 위치에 돌을 놓을 수 없습니다.");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 착수 대기 표시
    /// </summary>
    private void ShowPendingMove(int x, int y)
    {
        if (GameModeManager.Mode == GameMode.MultiPlayer && gameLogic.currentTurnPlayer == PlayerType.Opponent) return;

        // 돌 색상에 맞게 스프라이트 변경
        pendingMoveStoneRenderer.sprite =
            (gameLogic.currentStone == StoneType.Black) ? blackStoneSprite : whiteStoneSprite;
        pendingMoveStone.transform.position = BoardToWorldPosition(x, y);
        pendingMoveStone.SetActive(true);
    }

    /// <summary>
    /// 보드 좌표를 월드 좌표로 변환
    /// </summary>
    public Vector3 BoardToWorldPosition(int x, int y)
    {
        float worldX = (x - (boardSize - 1) / 2.0f) * cellSize + boardOffset.x;
        float worldY = ((boardSize - 1) / 2.0f - y) * cellSize + boardOffset.y; // Y축 반전
        return transform.position + new Vector3(worldX, worldY, 0);
    }

    #endregion

    #region 착수

    /// <summary>
    /// 실제로 돌을 놓는 메서드(착수 버튼에서 호출)
    /// </summary>
    public void PlaceStone()
    {
        if (GameModeManager.Mode == GameMode.MultiPlayer && gameLogic.currentTurnPlayer == PlayerType.Opponent) return;
        if (!hasPendingMove) return;

        int x = pendingMove.x;
        int y = pendingMove.y;
        if (!CanPlaceStone(x, y))
        {
            hasPendingMove = false;
            pendingMoveStone?.SetActive(false);
            return;
        }

        // 논리적 보드에 돌 정보 저장
        var stoneType = gameLogic.currentStone;
        board[x, y] = stoneType;

        // 시각적 돌 스프라이트 생성 및 마지막 수 마커 업데이트 
        PlaceStoneVisual(x, y, stoneType);
        
        Debug.Log($"돌이 놓였습니다: ({x}, {y}) - {stoneType}");

        hasPendingMove = false;
        pendingMoveStone?.SetActive(false); // 착수 대기 표시 숨기기

        if (GameModeManager.Mode == GameMode.MultiPlayer)
        {
            gamePlayManager.multiplayManager.GoStone(x, y);
        }

        OnPlaceStone?.Invoke(x, y); // 착수 이벤트 발생
    }

    /// <summary>
    /// 시각적 돌 스프라이트 생성 및 마지막 수 마커 업데이트
    /// </summary>
    public void PlaceStoneVisual(int x, int y, StoneType stoneType)
    {
        // 시각적 돌 스프라이트 생성
        Vector3 worldPos = BoardToWorldPosition(x, y);
        var stoneObj = Instantiate(stoneType == StoneType.Black ? stonePrefab_Black : stonePrefab_White,
            worldPos, Quaternion.identity, stoneParent);
        stoneObj.name = $"Stone_{x}_{y}";
        stoneObjects[x, y] = stoneObj;

        // 마지막 수 마커 업데이트
        UpdateLastMoveMarker(x, y);
        if (gamePlayManager.ShowForbiddenPositions && gamePlayManager.IsRenjuModeEnabled)
            UpdateForbiddenPositions();
    }

    public void PlaceOpponentStone(int x, int y)
    {
        Debug.Log($"상대방 돌 생성: ({x}, {y})");

        if (!CanPlaceStone(x, y)) return;

        // 논리적 보드에 돌 정보 저장
        var opponentStoneType = gameLogic.currentStone;
        board[x, y] = opponentStoneType;

        // 시각적 돌 생성
        Vector3 worldPos = BoardToWorldPosition(x, y);
        var stoneObj = Instantiate(
            opponentStoneType == StoneType.Black ? stonePrefab_Black : stonePrefab_White,
            worldPos, Quaternion.identity, stoneParent);
        stoneObj.name = $"OpponentStone_{x}_{y}";
        stoneObjects[x, y] = stoneObj;

        // 마지막 수 마커 업데이트
        UpdateLastMoveMarker(x, y);
        if (gamePlayManager.ShowForbiddenPositions && gamePlayManager.IsRenjuModeEnabled)
            UpdateForbiddenPositions();

        OnPlaceStone?.Invoke(x, y); // 착수 이벤트 발생
    }

    #endregion

    #region 마지막 수 마커

    /// <summary>
    /// 마지막 수 마커 업데이트
    /// </summary>
    private void UpdateLastMoveMarker(int x, int y)
    {
        lastMoveMarker.transform.position = BoardToWorldPosition(x, y);
        lastMoveMarker.SetActive(true);
    }

    #endregion

    #region 금지 위치 마커

    /// <summary>
    /// 금지 위치 마커들 업데이트
    /// </summary>
    public void UpdateForbiddenPositions()
    {
        HideForbiddenMarkers();

        if (!gamePlayManager.IsRenjuModeEnabled) return;

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
        var marker = Instantiate(forbiddenMarkerPrefab, BoardToWorldPosition(x, y), Quaternion.identity,
            forbiddenMarkerParent);
        marker.name = $"ForbiddenMarker_{x}_{y}";
        forbiddenMarkers.Add(marker);
    }

    #endregion


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
    
    /// <summary>
    /// 해당 위치에 돌이 없는지 확인
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool IsEmpty(int x, int y)
    {
        return IsValidPosition(x, y) && board[x, y] == StoneType.None;
    }
    
    /// <summary>
    /// 논리적 보드에 돌 정보 저장
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="stone"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void PlaceStone_Logical(int x, int y, StoneType stone)
    {
        if (!IsValidPosition(x, y))
            throw new ArgumentOutOfRangeException($"({x},{y}) is outside board.");
        board[x, y] = stone;
    }

    /// <summary>
    /// 보드에 돌을 놓을 공간이 없는지 확인
    /// </summary>
    /// <returns></returns>
    public bool IsBoardFull()
    {
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                if (board[x, y] == StoneType.None)
                {
                    return false;
                }
            }
        }
        return true;
    }
}