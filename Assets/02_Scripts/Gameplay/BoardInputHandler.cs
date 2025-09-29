using UnityEngine;
using UnityEngine.Events;
using static Constants;

/// <summary>
/// 보드에 입력되는 값을 처리하는 클래스
/// </summary>
public class BoardInputHandler : MonoBehaviour
{
    private Camera mainCamera;
    private Vector2Int hoveredPosition = new Vector2Int(-1, -1);

    public event UnityAction<Vector2Int> OnBoardHovered;
    public event UnityAction<Vector2Int> OnBoardClicked;

    private void Awake() { mainCamera = Camera.main; }

    void Update() { HandleMouseInput(); }

    /// <summary>
    /// 마우스 입력 처리
    /// </summary>
    private void HandleMouseInput()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        Vector2Int boardPos = WorldToBoardPosition(mouseWorldPos);
        int x = boardPos.x;
        int y = boardPos.y;

        // 마우스 호버 처리
        if (IsValidPosition(x, y) && boardPos != hoveredPosition)
        {
            hoveredPosition = boardPos;
            OnBoardHovered?.Invoke(boardPos);
        }

        // 클릭 처리
        if (Input.GetMouseButtonDown(0))
        {
            if (!IsValidPosition(x, y)) return;

            OnBoardClicked?.Invoke(new Vector2Int(x, y));
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
    private bool IsValidPosition(int x, int y) { return x >= 0 && x < boardSize && y >= 0 && y < boardSize; }
}