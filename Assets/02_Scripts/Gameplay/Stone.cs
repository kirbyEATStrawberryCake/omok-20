using UnityEngine;

public enum StoneType
{
    None = 0,   // �� ����
    Black = 1,  // �浹
    White = 2,   // �鵹
    Error = 3 // ���� (�̰� �߸� ������ �ִ� ��Ȳ�Դϴ�)
}

public class Stone : MonoBehaviour
{
    [Header("Animation Settings")]
    public float placementAnimationTime = 0.3f;     // ���� ���� �� �ִϸ��̼� �ð�
    public float hoverScaleMultiplier = 1.1f;       // ȣ���� ũ�� ����

    private StoneType stoneType;                    // �� ���� Ÿ��
    private Vector2Int boardPosition;               // �����ǿ����� ��ġ (x, y)
    private SpriteRenderer spriteRenderer;          // ���� ��������Ʈ ������
    private Vector3 originalScale;                  // ���� ũ�� ����
    private bool isAnimating = false;               // �ִϸ��̼� ���� �� ����

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        // ������ ���� ũ�⸦ 0���� ���� (�ִϸ��̼� ȿ��)
        transform.localScale = Vector3.zero;
    }

    /// <summary>
    /// ���� Ÿ�� ����
    /// </summary>
    /// <param name="type">������ ���� Ÿ��</param>
    public void SetStoneType(StoneType type)
    {
        stoneType = type;
        PlayPlacementAnimation();
    }

    /// <summary>
    /// ���� ������ ��ġ ����
    /// </summary>
    /// <param name="x">x��ǥ</param>
    /// <param name="y">y��ǥ</param>
    public void SetPosition(int x, int y)
    {
        boardPosition = new Vector2Int(x, y);
    }

    /// <summary>
    /// ���� ���� ���� �ִϸ��̼� ���
    /// </summary>
    private void PlayPlacementAnimation()
    {
        if (isAnimating) return;

        StartCoroutine(ScaleAnimation());
    }

    /// <summary>
    /// ũ�� ��ȭ �ִϸ��̼� �ڷ�ƾ
    /// </summary>
    private System.Collections.IEnumerator ScaleAnimation()
    {
        isAnimating = true;

        float elapsedTime = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = originalScale;

        // ź�� ȿ���� ���� ����������
        Vector3 overScale = originalScale * 1.2f;

        while (elapsedTime < placementAnimationTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / placementAnimationTime;

            // ź�� �ִϸ��̼� Ŀ�� (ó���� ũ�� ��Ÿ���ٰ� ���� ũ���)
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
    /// �� Ÿ�� ��ȯ
    /// </summary>
    public StoneType GetStoneType()
    {
        return stoneType;
    }

    /// <summary>
    /// �����ǿ����� ��ġ ��ȯ
    /// </summary>
    public Vector2Int GetBoardPosition()
    {
        return boardPosition;
    }

    /// <summary>
    /// ���콺 ���� �� ���̶���Ʈ ȿ��
    /// </summary>
    void OnMouseEnter()
    {
        if (isAnimating) return;

        // ũ�⸦ ��¦ Ű���� ���̶���Ʈ ȿ��
        StartCoroutine(HoverAnimation(originalScale * hoverScaleMultiplier));

        // ���� �������� ���̶���Ʈ
        if (spriteRenderer != null)
        {
            Color currentColor = spriteRenderer.color;
            currentColor.a = 0.9f;
            spriteRenderer.color = currentColor;
        }
    }

    /// <summary>
    /// ���콺�� ����� �� ���� ���·� ����
    /// </summary>
    void OnMouseExit()
    {
        if (isAnimating) return;

        // ���� ũ��� ����
        StartCoroutine(HoverAnimation(originalScale));

        // ���� ���󺹱�
        if (spriteRenderer != null)
        {
            Color currentColor = spriteRenderer.color;
            currentColor.a = 1.0f;
            spriteRenderer.color = currentColor;
        }
    }

    /// <summary>
    /// ȣ�� �ִϸ��̼� �ڷ�ƾ
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
