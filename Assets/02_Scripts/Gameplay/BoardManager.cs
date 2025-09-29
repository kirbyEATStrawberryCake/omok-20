using UnityEngine;
using System.Collections.Generic;
using static Constants;

/// <summary>
/// 사용자의 입력에 따라 보드 위에 돌, 마크의 시각적인 처리를 관리하는 클래스
/// </summary>
public class BoardManager : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite blackStoneSprite; // 흑돌 스프라이트
    [SerializeField] private Sprite whiteStoneSprite; // 백돌 스프라이트

    [Header("Maker Object")]
    [SerializeField] private GameObject selectedMarker;   // 현재 선택된 위치 마커
    [SerializeField] private GameObject lastMoveMarker;   // 마지막 수 마커
    [SerializeField] private GameObject pendingMoveStone; // 착수 대기 마커

    [Header("Marker Settings")]
    [SerializeField] private GameObject stonePrefab_White;
    [SerializeField] private GameObject stonePrefab_Black;
    [SerializeField] private Transform stoneParent; // 돌을 생성할 부모 오브젝트
    [Space(10)]
    [SerializeField] private GameObject forbiddenMarkerPrefab; // 금지 마크 프리팹
    [SerializeField] private Transform forbiddenMarkerParent;  // 금지 마크를 생성할 부모 오브젝트

    private List<GameObject> stoneObjects = new();     // 돌 오브젝트 배열
    private List<GameObject> forbiddenMarkers = new(); // 금지 위치 마커들

    private SpriteRenderer pendingMoveStoneRenderer; // 착수 대기 마커 렌더러

    #region Unity Life Cycle

    private void Awake() { pendingMoveStoneRenderer = pendingMoveStone.GetComponent<SpriteRenderer>(); }

    #endregion

    #region 초기화

    /// <summary>
    /// 시각적 오목판 초기화
    /// </summary>
    public void InitializeBoard()
    {
        // 기존 오브젝트들 제거
        ClearBoard();
        HideAllMarkers();
        stoneObjects = new List<GameObject>();
        forbiddenMarkers = new List<GameObject>();
    }

    /// <summary>
    /// 시각적 오목판 정리
    /// </summary>
    private void ClearBoard()
    {
        foreach (var stone in stoneObjects)
        {
            if (stone) Destroy(stone);
        }

        stoneObjects.Clear();
    }

    /// <summary>
    /// 마커들 제거
    /// </summary>
    public void HideAllMarkers(GameResult result = GameResult.None)
    {
        selectedMarker?.SetActive(false);   // 선택된 위치 마커 숨기기
        lastMoveMarker?.SetActive(false);   // 마지막 수 마커 숨기기
        pendingMoveStone?.SetActive(false); // 착수 대기 표시 숨기기
        HideForbiddenMarkers();             // 금지 위치 마커들 숨기기
    }

    /// <summary>
    /// 금지 위치 마커들 숨기기
    /// </summary>
    private void HideForbiddenMarkers()
    {
        foreach (GameObject marker in forbiddenMarkers)
        {
            if (marker) { Destroy(marker); }
        }

        forbiddenMarkers.Clear();
    }

    #endregion

    #region 좌표 변환

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

    #region 착수 대기

    /// <summary>
    /// 선택된 위치 마커 업데이트
    /// </summary>
    public void UpdateSelectedMarker(int x, int y)
    {
        selectedMarker.transform.position = BoardToWorldPosition(x, y);
        selectedMarker?.SetActive(true);
    }

    /// <summary>
    /// 착수 대기 표시
    /// </summary>
    public void ShowPendingMove(int x, int y, StoneType stoneType)
    {
        // 돌 색상에 맞게 스프라이트 변경
        pendingMoveStoneRenderer.sprite = (stoneType == StoneType.Black) ? blackStoneSprite : whiteStoneSprite;
        pendingMoveStone.transform.position = BoardToWorldPosition(x, y);
        pendingMoveStone.SetActive(true);
    }

    #endregion

    #region 착수

    /// <summary>
    /// 시각적 돌 스프라이트 생성 및 마지막 수 마커 업데이트
    /// </summary>
    public void PlaceStoneVisual(int x, int y, StoneType stoneType)
    {
        pendingMoveStone?.SetActive(false); // 착수 대기 표시 숨기기

        // 시각적 돌 스프라이트 생성
        Vector3 worldPos = BoardToWorldPosition(x, y);
        var stoneObj = Instantiate(stoneType == StoneType.Black ? stonePrefab_Black : stonePrefab_White,
            worldPos, Quaternion.identity, stoneParent);
        stoneObj.name = $"Stone_{x}_{y}";
        stoneObjects.Add(stoneObj);

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

    #region 금지 위치 마커

    /// <summary>
    /// 금지 위치 마커 생성
    /// </summary>
    public void UpdateForbiddenMarker(List<Vector2Int> forbiddenPositions)
    {
        HideForbiddenMarkers();
        foreach (var pos in forbiddenPositions)
        {
            var marker = Instantiate(forbiddenMarkerPrefab, BoardToWorldPosition(pos.x, pos.y), Quaternion.identity,
                forbiddenMarkerParent);
            marker.name = $"ForbiddenMarker_{pos.x}_{pos.y}";
            forbiddenMarkers.Add(marker);
        }
    }

    #endregion
}