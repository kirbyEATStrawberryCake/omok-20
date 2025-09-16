using UnityEngine;

public enum StoneType
{
    None = 0,   // 빈 공간
    Black = 1,  // 흑돌
    White = 2,   // 백돌
    Error = 3 // 에러 (이게 뜨면 문제가 있는 상황입니다)
}

public class Stone : MonoBehaviour
{
    [Header("Stone Visual Settings")]
    public Material blackStoneMaterial;     // 흑돌 머티리얼
    public Material whiteStoneMaterial;     // 백돌 머티리얼
    public float placementAnimationTime = 0.3f;  // 돌이 놓일 때 애니메이션 시간

    private StoneType stoneType;            // 이 돌의 타입
    private Vector2Int boardPosition;       // 오목판에서의 위치 (x, y)
    private Renderer stoneRenderer;         // 돌의 렌더러 컴포넌트
    private Vector3 originalScale;          // 원래 크기 저장

    void Awake()
    {
        stoneRenderer = GetComponent<Renderer>();
        originalScale = transform.localScale;

        // 시작할 때는 크기를 0으로 설정 (애니메이션 효과)
        transform.localScale = Vector3.zero;
    }

    /// <summary>
    /// 돌의 타입 설정
    /// </summary>
    /// <param name="type">설정할 돌의 타입</param>
    public void SetStoneType(StoneType type)
    {
        stoneType = type;
        UpdateVisual();
        PlayPlacementAnimation();
    }

    /// <summary>
    /// 돌의 오목판 위치 설정
    /// </summary>
    /// <param name="x">x좌표</param>
    /// <param name="y">y좌표</param>
    public void SetPosition(int x, int y)
    {
        boardPosition = new Vector2Int(x, y);
    }

    /// <summary>
    /// 돌의 시각적 표현 업데이트
    /// </summary>
    private void UpdateVisual()
    {
        if (stoneRenderer == null) return;

        switch (stoneType)
        {
            case StoneType.Black:
                stoneRenderer.material = blackStoneMaterial;
                break;
            case StoneType.White:
                stoneRenderer.material = whiteStoneMaterial;
                break;
            default:
                Debug.LogWarning("Unknown stone type!");
                break;
        }
    }

    /// <summary>
    /// 돌이 놓일 때의 애니메이션 재생
    /// </summary>
    private void PlayPlacementAnimation()
    {
        // LeanTween이 없는 경우를 위한 간단한 코루틴 애니메이션
        StartCoroutine(ScaleAnimation());
    }

    /// <summary>
    /// 크기 변화 애니메이션 코루틴
    /// </summary>
    private System.Collections.IEnumerator ScaleAnimation()
    {
        float elapsedTime = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = originalScale;

        while (elapsedTime < placementAnimationTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / placementAnimationTime;

            // Ease-out 애니메이션 커브
            t = 1f - Mathf.Pow(1f - t, 3f);

            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        transform.localScale = endScale;
    }

    /// <summary>
    /// 돌 타입 반환
    /// </summary>
    public StoneType GetStoneType()
    {
        return stoneType;
    }

    /// <summary>
    /// 오목판에서의 위치 반환
    /// </summary>
    public Vector2Int GetBoardPosition()
    {
        return boardPosition;
    }

    /// <summary>
    /// 마우스 오버 시 하이라이트 효과
    /// </summary>
    void OnMouseEnter()
    {
        // 투명도나 크기를 살짝 변경하여 하이라이트 효과
        if (stoneRenderer != null)
        {
            Color currentColor = stoneRenderer.material.color;
            currentColor.a = 0.8f;
            stoneRenderer.material.color = currentColor;
        }
    }

    /// <summary>
    /// 마우스가 벗어났을 때 원래 상태로 복원
    /// </summary>
    void OnMouseExit()
    {
        if (stoneRenderer != null)
        {
            Color currentColor = stoneRenderer.material.color;
            currentColor.a = 1.0f;
            stoneRenderer.material.color = currentColor;
        }
    }
}

