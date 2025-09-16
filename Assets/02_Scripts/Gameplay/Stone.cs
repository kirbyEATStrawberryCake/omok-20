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
    [Header("Stone Visual Settings")]
    public Material blackStoneMaterial;     // �浹 ��Ƽ����
    public Material whiteStoneMaterial;     // �鵹 ��Ƽ����
    public float placementAnimationTime = 0.3f;  // ���� ���� �� �ִϸ��̼� �ð�

    private StoneType stoneType;            // �� ���� Ÿ��
    private Vector2Int boardPosition;       // �����ǿ����� ��ġ (x, y)
    private Renderer stoneRenderer;         // ���� ������ ������Ʈ
    private Vector3 originalScale;          // ���� ũ�� ����

    void Awake()
    {
        stoneRenderer = GetComponent<Renderer>();
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
        UpdateVisual();
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
    /// ���� �ð��� ǥ�� ������Ʈ
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
    /// ���� ���� ���� �ִϸ��̼� ���
    /// </summary>
    private void PlayPlacementAnimation()
    {
        // LeanTween�� ���� ��츦 ���� ������ �ڷ�ƾ �ִϸ��̼�
        StartCoroutine(ScaleAnimation());
    }

    /// <summary>
    /// ũ�� ��ȭ �ִϸ��̼� �ڷ�ƾ
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

            // Ease-out �ִϸ��̼� Ŀ��
            t = 1f - Mathf.Pow(1f - t, 3f);

            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        transform.localScale = endScale;
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
        // ������ ũ�⸦ ��¦ �����Ͽ� ���̶���Ʈ ȿ��
        if (stoneRenderer != null)
        {
            Color currentColor = stoneRenderer.material.color;
            currentColor.a = 0.8f;
            stoneRenderer.material.color = currentColor;
        }
    }

    /// <summary>
    /// ���콺�� ����� �� ���� ���·� ����
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

