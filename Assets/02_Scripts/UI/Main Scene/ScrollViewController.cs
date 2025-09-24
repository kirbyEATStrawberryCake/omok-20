using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewController : MonoBehaviour
{
    [SerializeField] private float cellHeight;

    private ScrollRect _scrollRect;
    private RectTransform _viewportRectTransform;

    private List<RankingUser> rankingUsers;
    private readonly LinkedList<RankingCell> visibleCells = new();

    private void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
        _viewportRectTransform = _scrollRect.viewport;
    }

    /// <summary>
    /// 외부에서 데이터를 받아 스크롤 뷰를 초기화하는 메서드
    /// </summary>
    public void Initialize(List<RankingUser> rankingUsers)
    {
        this.rankingUsers = rankingUsers;
        _scrollRect.verticalNormalizedPosition = 1f; // 스크롤을 맨 위로 설정

        // 기존 셀들 풀에 반환
        foreach (var cell in visibleCells)
        {
            ObjectPool.Instance.ReturnObject(cell.gameObject);
        }

        visibleCells.Clear();

        // Content 높이를 새 데이터에 맞게 재설정
        var contentSizeDelta = _scrollRect.content.sizeDelta;
        contentSizeDelta.y = this.rankingUsers.Count * cellHeight;
        _scrollRect.content.sizeDelta = contentSizeDelta;

        // 스크롤 이벤트가 발생하기 전에 초기 화면을 채움
        UpdateVisibleCells();
    }

    /// <summary>
    /// 스크롤 값이 변경될 때마다 호출
    /// </summary>
    public void OnValueChanged(Vector2 value)
    {
        UpdateVisibleCells();
    }

    /// <summary>
    /// 현재 스크롤 위치를 기반으로 보여져야 할 셀들을 계산하고 업데이트
    /// </summary>
    private void UpdateVisibleCells()
    {
        var (startIndex, endIndex) = GetVisibleIndexRange();

        // --- 1. 화면 밖으로 나간 셀을 풀에 반환 ---
        var node = visibleCells.First;
        while (node != null)
        {
            var next = node.Next;
            if (node.Value.Index < startIndex || node.Value.Index > endIndex)
            {
                ObjectPool.Instance.ReturnObject(node.Value.gameObject);
                visibleCells.Remove(node);
            }

            node = next;
        }

        // --- 2. 화면에 새로 보여야 할 셀을 풀에서 가져와 생성 ---
        for (int i = startIndex; i <= endIndex; i++)
        {
            // 이미 보이는 셀인지 확인
            bool isVisible = visibleCells.Any(cell => cell.Index == i);

            if (!isVisible)
            {
                var cellObject = ObjectPool.Instance.GetObject();
                var rankingItem = cellObject.GetComponent<RankingCell>();
                rankingItem.SetData(rankingUsers[i], i);
                rankingItem.transform.localPosition = new Vector3(0, -i * cellHeight, 0);

                // 정렬된 상태를 유지하며 추가
                if (visibleCells.Count == 0 || i > visibleCells.Last.Value.Index)
                {
                    visibleCells.AddLast(rankingItem);
                }
                else
                {
                    var firstNode = visibleCells.First;
                    while (firstNode != null && i > firstNode.Value.Index)
                    {
                        firstNode = firstNode.Next;
                    }

                    if (firstNode != null)
                    {
                        visibleCells.AddBefore(firstNode, rankingItem);
                    }
                    else
                    {
                        visibleCells.AddLast(rankingItem);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 보이는 셀들의 인덱스 계산
    /// </summary>
    private (int startIndex, int endIndex) GetVisibleIndexRange()
    {
        var contentPos = _scrollRect.content.anchoredPosition.y;
        var viewHeight = _viewportRectTransform.rect.height;

        var startIndex = Mathf.FloorToInt(contentPos / cellHeight);
        var visibleCount = Mathf.CeilToInt(viewHeight / cellHeight);

        // 버퍼를 두어 부드러운 스크롤링을 위해 위아래로 1개씩 더 표시
        startIndex = Mathf.Max(0, startIndex - 1);
        var endIndex = startIndex + visibleCount + 1;

        // 전체 아이템 개수를 넘어가지 않도록 조정
        endIndex = Mathf.Min(endIndex, rankingUsers.Count - 1);

        return (startIndex, endIndex);
    }
}