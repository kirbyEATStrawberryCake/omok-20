using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.Multiplayer.Playmode;
using UnityEngine.Events;

public class GiboBoardManager : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite blackStoneSprite; // 흑돌 스프라이트

    [SerializeField] private Sprite whiteStoneSprite; // 백돌 스프라이트

    [Header("Board Settings")]
    [SerializeField] private float cellSize = 0.494f; // 각 칸의 크기

    public int boardSize { get; private set; } = 15; // 오목판 크기 (15x15)
    [SerializeField] private Vector2 boardOffset = Vector2.zero; // 보드 오프셋

    [Header("Maker Object")]
    [SerializeField] private GameObject lastMoveMarker; // 마지막 수 마커

    [Header("Marker Settings")]
    [SerializeField] private GameObject stonePrefab_White;

    [SerializeField] private GameObject stonePrefab_Black;
    [SerializeField] private Transform stoneParent; // 돌을 생성할 부모 오브젝트
    [Space(10)][SerializeField] private GameObject forbiddenMarkerPrefab; // 금지 마크 프리팹
    [SerializeField] private Transform forbiddenMarkerParent; // 금지 마크를 생성할 부모 오브젝트

    protected StoneType[,] board; // 오목판 배열 (논리적 보드)
    private GameObject[,] stoneObjects; // 돌 오브젝트 배열

    private List<GameObject> forbiddenMarkers; // 금지 위치 마커들

    // 기보용
    private GameRecord currentRecord;
    private int currentMoveIndex = -1;

    // 이벤트
    public event UnityAction<int> OnButtonChanged;
    public event UnityAction<GameRecord> OnProfileImage;


    #region 유니티 이벤트

    private void Awake()
    {
        InitializeBoard();
        CreateTestRecord();
        //LoadSelectedRecord();
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

        // 기존 오브젝트들 제거
        ClearBoard();
    }

    /// <summary>
    /// 오목판 정리
    /// </summary>
    public void ClearBoard()
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
        lastMoveMarker?.SetActive(false); // 마지막 수 마커 숨기기
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
    /// 보드 좌표를 월드 좌표로 변환
    /// </summary>
    private Vector3 BoardToWorldPosition(int x, int y)
    {
        float worldX = (x - (boardSize - 1) / 2.0f) * cellSize + boardOffset.x;
        float worldY = ((boardSize - 1) / 2.0f - y) * cellSize + boardOffset.y; // Y축 반전
        return transform.position + new Vector3(worldX, worldY, 0);
    }

    #endregion

    #region 착수

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

    #region 기보용 - 해당 좌표 돌 제거, 마지막 마커 숨기기
    /// <summary>
    /// 해당 좌표의 돌 제거
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void RemoveStone(int x, int y)
    {
        // 논리적 보드 초기화
        board[x, y] = StoneType.None;

        // 시각적 돌 제거
        if (stoneObjects[x, y] != null)
        {
            Destroy(stoneObjects[x, y]);
            stoneObjects[x, y] = null;
        }

        // 마지막 수 마커 업데이트
        lastMoveMarker.SetActive(false);
    }

    /// <summary>
    /// 기보용 - 마지막 수 마커 숨기기 
    /// </summary>
    public void HideLastMoveMarker()
    {
        if (lastMoveMarker != null)
            lastMoveMarker.SetActive(false);
    }
    #endregion

    private void LoadSelectedRecord()
    {
        string gameId = SelectedGiboGameId.selectedGameId;

        if (string.IsNullOrEmpty(gameId))
        {
            Debug.LogError($"{gameId}");
            return;
        }

        currentRecord = GiboManager.LoadRecord(gameId);
        if (currentRecord == null)
        {
            Debug.LogError($"기보를 찾을 수 없습니다: {gameId}");
            return;
        }

        // 초기화: 보드 비우기
        ClearBoard();
        currentMoveIndex = -1; // 아직 돌이 놓이지 않은 상태
        OnProfileImage?.Invoke(currentRecord);
    }

    private void CreateTestRecord()
    {
        // 테스트용 ID 설정
        SelectedGiboGameId.selectedGameId = "20250919_120000";

        // 테스트용 GameRecord 생성
        currentRecord = new GameRecord
        {
            startTime = "20250919_120000", // 키 값용
            displayTime = "2025-09-19 12:00", // 화면 표시용
            otherPlayerNickname = "홍길동",
            otherRank = 3,
            otherProfileImage = 1,
            moves = new System.Collections.Generic.List<MoveData>()
        };

        currentRecord.moves.Add(new MoveData { turn = 1, stoneColor = 1, x = 7, y = 7 }); // 흑 중앙
        currentRecord.moves.Add(new MoveData { turn = 2, stoneColor = 2, x = 7, y = 8 }); // 백 아래
        currentRecord.moves.Add(new MoveData { turn = 3, stoneColor = 1, x = 8, y = 8 }); // 흑 대각
        currentRecord.moves.Add(new MoveData { turn = 4, stoneColor = 2, x = 6, y = 7 }); // 백 왼쪽
        currentRecord.moves.Add(new MoveData { turn = 5, stoneColor = 1, x = 8, y = 7 }); // 흑 오른쪽
    }

    #region 버튼 이벤트 (처음 / 이전 / 다음 / 끝)

    public void OnClickFirst()
    {
        ClearBoard();
        currentMoveIndex = -1;
        OnButtonChanged?.Invoke(currentMoveIndex);
    }

    public void OnClickPrevious()
    {
        if (currentMoveIndex < 0) return;

        // 마지막 수 제거
        MoveData lastMove = currentRecord.moves[currentMoveIndex];
        RemoveStone(lastMove.x, lastMove.y);
        currentMoveIndex--;
        OnButtonChanged?.Invoke(currentMoveIndex);
    }

    public void OnClickNext()
    {
        if (currentRecord == null || currentMoveIndex >= currentRecord.moves.Count - 1) return;

        currentMoveIndex++;
        MoveData move = currentRecord.moves[currentMoveIndex];
        PlaceStoneVisual(move.x, move.y, move.stoneColor == 1 ? StoneType.Black : StoneType.White);
        if(currentMoveIndex == currentRecord.moves.Count -1)
            OnButtonChanged?.Invoke(-2);
        else
        {
            OnButtonChanged?.Invoke(currentMoveIndex);
        }
    }

    public void OnClickLast()
    {
        if (currentRecord == null) return;

        // 보드 초기화 후 모든 돌 배치
        ClearBoard();
        for (int i = 0; i < currentRecord.moves.Count; i++)
        {
            MoveData move = currentRecord.moves[i];
            PlaceStoneVisual(move.x, move.y, move.stoneColor == 1 ? StoneType.Black : StoneType.White);
        }
        currentMoveIndex = currentRecord.moves.Count - 1;
        OnButtonChanged?.Invoke(-2); // -2 값을 보낼 경우 마지막이라고 인식
    }

    #endregion
}