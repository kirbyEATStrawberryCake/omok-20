using System;
using UnityEngine;

public enum StoneType
{
    None = 0, // 빈 공간
    Black = 1, // 흑돌
    White = 2, // 백돌
    Error = 3 // 에러 (이게 뜨면 문제가 있는 상황입니다)
}

public class Stone : MonoBehaviour
{
    [Header("Animation Settings")] public float placementAnimationTime = 0.3f; // 돌이 놓일 때 애니메이션 시간
    public float hoverScaleMultiplier = 1.1f; // 호버시 크기 배율

    private StoneType stoneType; // 이 돌의 타입
    private Vector2Int boardPosition; // 오목판에서의 위치 (x, y)
    private SpriteRenderer spriteRenderer; // 돌의 스프라이트 렌더러
    private Vector3 originalScale; // 원래 크기 저장
    private bool isAnimating = false; // 애니메이션 진행 중 여부

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        // 시작할 때는 크기를 0으로 설정 (애니메이션 효과)
        transform.localScale = Vector3.zero;
    }

    private void OnEnable()
    {
        PlayPlacementAnimation();
    }

    /// <summary>
    /// 돌이 놓일 때의 애니메이션 재생
    /// </summary>
    private void PlayPlacementAnimation()
    {
        if (isAnimating) return;

        StartCoroutine(ScaleAnimation());
    }

    /// <summary>
    /// 크기 변화 애니메이션 코루틴
    /// </summary>
    private System.Collections.IEnumerator ScaleAnimation()
    {
        isAnimating = true;

        float elapsedTime = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = originalScale;

        // 탄성 효과를 위한 오버스케일
        Vector3 overScale = originalScale * 1.2f;

        while (elapsedTime < placementAnimationTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / placementAnimationTime;

            // 탄성 애니메이션 커브 (처음에 크게 나타났다가 원래 크기로)
            if (t < 0.7f)
            {
                float scaleT = t / 0.7f;
                scaleT = 1f - Mathf.Pow(1f - scaleT, 3f); // Ease-out
                transform.localScale = Vector3.Lerp(startScale, overScale, scaleT);
            }
            else
            {
                float scaleT = (t - 0.7f) / 0.3f;
                scaleT = 1f - Mathf.Pow(1f - scaleT, 2f); // Ease-out
                transform.localScale = Vector3.Lerp(overScale, endScale, scaleT);
            }

            yield return null;
        }

        transform.localScale = endScale;
        isAnimating = false;
    }

    /// <summary>
    /// 마우스 오버 시 하이라이트 효과
    /// </summary>
    void OnMouseEnter()
    {
        if (isAnimating) return;

        // 크기를 살짝 키워서 하이라이트 효과
        StartCoroutine(HoverAnimation(originalScale * hoverScaleMultiplier));

        // 투명도 변경으로 하이라이트
        if (spriteRenderer != null)
        {
            Color currentColor = spriteRenderer.color;
            currentColor.a = 0.9f;
            spriteRenderer.color = currentColor;
        }
    }

    /// <summary>
    /// 마우스가 벗어났을 때 원래 상태로 복원
    /// </summary>
    void OnMouseExit()
    {
        if (isAnimating) return;

        // 원래 크기로 복원
        StartCoroutine(HoverAnimation(originalScale));

        // 투명도 원상복구
        if (spriteRenderer != null)
        {
            Color currentColor = spriteRenderer.color;
            currentColor.a = 1.0f;
            spriteRenderer.color = currentColor;
        }
    }

    /// <summary>
    /// 호버 애니메이션 코루틴
    /// </summary>
    private System.Collections.IEnumerator HoverAnimation(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float animationTime = 0.1f;
        float elapsedTime = 0f;

        while (elapsedTime < animationTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationTime;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
    }
}