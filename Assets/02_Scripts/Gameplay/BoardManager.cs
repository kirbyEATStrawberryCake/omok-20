using UnityEngine;
using System.Collections.Generic;
using System;

public class BoardManager : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite boardSprite;                  // ������ ��������Ʈ
    public Sprite blackStoneSprite;             // �浹 ��������Ʈ
    public Sprite whiteStoneSprite;             // �鵹 ��������Ʈ
    public Sprite forbiddenMarkerSprite;        // ���� ��ġ ��Ŀ ��������Ʈ
    public Sprite selectedMarkerSprite;         // ������ ��ġ ǥ�� ��Ŀ ��������Ʈ
    public Sprite lastMoveMarkerSprite;         // ������ ���� ��ġ ǥ�� ��Ŀ ��������Ʈ
    public Sprite pendingMoveSprite;            // ���� ��� ǥ�� ��������Ʈ

    [Header("Board Settings")]
    public int boardSize = 15;                  // ������ ũ�� (15x15)
    public float cellSize = 0.6f;               // �� ĭ�� ũ��
    public Vector2 boardOffset = Vector2.zero;  // ���� ������

    [Header("Marker Settings")]
    public float markerAlpha = 0.7f;            // ��Ŀ ����
    public Color forbiddenColor = Color.red;    // ���� ��ġ ����
    public Color selectedColor = Color.yellow;   // ���� ��ġ ����
    public Color lastMoveColor = Color.green;   // ������ �� ����
    public Color pendingMoveColor = Color.cyan; // ���� ��� ����

    protected StoneType[,] board;                 // ������ �迭 (���� ����)
    private GameObject[,] stoneObjects;         // �� ������Ʈ �迭
    private GameObject boardObject;             // ���� ��������Ʈ ������Ʈ

    // �Ŵ��� ���� (ȿ������ ����)
    private GameManager gameManager;
    private RenjuRule renjuRule;

    // ��Ŀ ����
    private List<GameObject> forbiddenMarkers;  // ���� ��ġ ��Ŀ��
    private GameObject selectedMarker;          // ���� ���õ� ��ġ ��Ŀ
    private GameObject lastMoveMarker;          // ������ �� ��Ŀ
    private GameObject pendingMoveMarker;       // ���� ��� ��Ŀ

    // ���콺 ��ġ ����
    private Vector2Int hoveredPosition = new Vector2Int(-1, -1);

    // ��ġ�� ���õǾ��� �� �߻��ϴ� �̺�Ʈ
    public event Action<int, int> OnPositionSelected;

    /// <summary>
    /// GameManager ���� ����
    /// </summary>
    public void SetGameManager(GameManager manager)
    {
        gameManager = manager;
        renjuRule = gameManager.renjuRule; // GameManager�� ���� RenjuRule ���� ȹ��
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
    /// ������ �ʱ�ȭ
    /// </summary>
    public void InitializeBoard()
    {
        // �迭 �ʱ�ȭ
        board = new StoneType[boardSize, boardSize];
        stoneObjects = new GameObject[boardSize, boardSize];
        forbiddenMarkers = new List<GameObject>();

        // ���� ������Ʈ�� ����
        ClearBoard();

        // ���� ��������Ʈ ����
        CreateBoardSprite();

        Debug.Log("�������� �ʱ�ȭ�Ǿ����ϴ�.");
    }

    /// <summary>
    /// ���� ��������Ʈ ����
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
        boardRenderer.sortingOrder = -10; // ���� �ڿ� ������

        // ���� ũ�� ����
        if (boardSprite != null)
        {
            float spriteWidth = boardSprite.bounds.size.x;
            float spriteHeight = boardSprite.bounds.size.y;
            float desiredSize = (boardSize - 1) * cellSize;

            boardObject.transform.localScale = new Vector3(
                desiredSize / spriteWidth,
                desiredSize / spriteHeight,
                1f
            );
        }
    }

    /// <summary>
    /// �������� ��� �� ����
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

        // ��Ŀ�� ����
        HideAllMarkers();
    }

    /// <summary>
    /// ���콺 �Է� ó��
    /// </summary>
    private void HandleMouseInput()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector2Int boardPos = WorldToBoardPosition(mouseWorldPos);

        // ���콺 ȣ�� ó��
        if (IsValidPosition(boardPos.x, boardPos.y) && boardPos != hoveredPosition)
        {
            hoveredPosition = boardPos;
            UpdateSelectedMarker(boardPos.x, boardPos.y);
        }

        // Ŭ�� ó��
        if (Input.GetMouseButtonDown(0))
        {
            HandleBoardClick(boardPos.x, boardPos.y);
        }
    }

    /// <summary>
    /// ���� Ŭ�� ó��
    /// </summary>
    private void HandleBoardClick(int x, int y)
    {
        if (gameManager.GetGameState() != GameState.Playing) return;

        if (IsValidPosition(x, y))
        {
            // ��ġ ���� �̺�Ʈ �߻�
            OnPositionSelected?.Invoke(x, y);
        }
    }

    /// <summary>
    /// ������ ��ġ�� ���� ���� �� �ִ��� �˻�
    /// </summary>
    /// <param name="x">x��ǥ</param>
    /// <param name="y">y��ǥ</param>
    /// <param name="stoneType">���� ���� Ÿ��</param>
    /// <returns>���� ���� �� �ִ��� ����</returns>
    public bool CanPlaceStone(int x, int y, StoneType stoneType)
    {
        // ���� �˻�
        if (!IsValidPosition(x, y)) return false;

        // �̹� ���� �����ִ��� �˻�
        if (board[x, y] != StoneType.None) return false;

        // ���ַ� �˻� (�浹�� ��츸)
        if (gameManager.isRenjuModeEnabled && stoneType == StoneType.Black)
        {
            if (!renjuRule.IsValidMove(x, y, stoneType, board))
            {
                Debug.Log("���ַ� �������� �ش� ��ġ�� ���� ���� �� �����ϴ�.");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// ������ ���� ���� �޼���
    /// </summary>
    /// <param name="x">x��ǥ</param>
    /// <param name="y">y��ǥ</param>
    /// <param name="stoneType">���� Ÿ��</param>
    /// <returns>���� ���������� �������� ����</returns>
    public bool PlaceStone(int x, int y, StoneType stoneType)
    {
        if (!CanPlaceStone(x, y, stoneType)) return false;

        // ���� ���忡 �� ���� ����
        board[x, y] = stoneType;

        // �ð��� �� ��������Ʈ ����
        Vector3 worldPos = BoardToWorldPosition(x, y);
        GameObject stoneObj = new GameObject($"Stone_{x}_{y}");
        stoneObj.transform.SetParent(transform);
        stoneObj.transform.position = worldPos;

        SpriteRenderer stoneRenderer = stoneObj.AddComponent<SpriteRenderer>();
        stoneRenderer.sprite = (stoneType == StoneType.Black) ? blackStoneSprite : whiteStoneSprite;
        stoneRenderer.sortingOrder = 1; // ���庸�� �տ� ������

        // �� ������Ʈ ����
        Stone stone = stoneObj.AddComponent<Stone>();
        stone.SetStoneType(stoneType);
        stone.SetPosition(x, y);

        stoneObjects[x, y] = stoneObj;

        // ������ �� ��Ŀ ������Ʈ
        UpdateLastMoveMarker(x, y);

        Debug.Log($"���� �������ϴ�: ({x}, {y}) - {stoneType}");
        return true;
    }

    /// <summary>
    /// ���� ��� ǥ��
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
            // �� ���� �°� ��������Ʈ ����
            SpriteRenderer renderer = pendingMoveMarker.GetComponent<SpriteRenderer>();
            renderer.sprite = (stoneType == StoneType.Black) ? blackStoneSprite : whiteStoneSprite;
        }

        pendingMoveMarker.transform.position = BoardToWorldPosition(x, y);
        pendingMoveMarker.SetActive(true);
    }

    /// <summary>
    /// ���� ��� ǥ�� �����
    /// </summary>
    public void HidePendingMove()
    {
        if (pendingMoveMarker != null)
        {
            pendingMoveMarker.SetActive(false);
        }
    }

    /// <summary>
    /// ���õ� ��ġ ��Ŀ ������Ʈ
    /// </summary>
    private void UpdateSelectedMarker(int x, int y)
    {
        if (gameManager.GetGameState() != GameState.Playing) return;

        // �̹� ���� ���� ��ġ�� ��Ŀ ǥ������ ����
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
    /// ���õ� ��ġ ��Ŀ �����
    /// </summary>
    private void HideSelectedMarker()
    {
        if (selectedMarker != null)
        {
            selectedMarker.SetActive(false);
        }
    }

    /// <summary>
    /// ������ �� ��Ŀ ������Ʈ
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
    /// ������ �� ��Ŀ �����
    /// </summary>
    private void HideLastMoveMarker()
    {
        if (lastMoveMarker != null)
        {
            lastMoveMarker.SetActive(false);
        }
    }

    /// <summary>
    /// ���� ��ġ ��Ŀ�� ������Ʈ
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
    /// ���� ��ġ ��Ŀ ����
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
    /// ���� ��ġ ��Ŀ�� �����
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
    /// ��� ��Ŀ �����
    /// </summary>
    public void HideAllMarkers()
    {
        HideSelectedMarker();
        HideLastMoveMarker();
        HideForbiddenMarkers();
        HidePendingMove();
    }

    /// <summary>
    /// ���� ��ǥ�� ���� ��ǥ�� ��ȯ
    /// </summary>
    private Vector3 BoardToWorldPosition(int x, int y)
    {
        float worldX = (x - (boardSize - 1) / 2.0f) * cellSize + boardOffset.x;
        float worldY = ((boardSize - 1) / 2.0f - y) * cellSize + boardOffset.y; // Y�� ����
        return transform.position + new Vector3(worldX, worldY, 0);
    }

    /// <summary>
    /// ���� ��ǥ�� ���� ��ǥ�� ��ȯ
    /// </summary>
    private Vector2Int WorldToBoardPosition(Vector3 worldPos)
    {
        Vector3 localPos = worldPos - transform.position;
        int x = Mathf.RoundToInt((localPos.x - boardOffset.x) / cellSize + (boardSize - 1) / 2.0f);
        int y = Mathf.RoundToInt((boardSize - 1) / 2.0f - (localPos.y - boardOffset.y) / cellSize); // Y�� ����
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// ��ȿ�� ��ġ���� �˻�
    /// </summary>
    public bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < boardSize && y >= 0 && y < boardSize;
    }

    /// <summary>
    /// ������ ��ġ�� �� Ÿ�� ��ȯ
    /// </summary>
    public StoneType GetStoneAt(int x, int y)
    {
        if (!IsValidPosition(x, y)) return StoneType.None;
        return board[x, y];
    }

    /// <summary>
    /// ���� ���� ���� ��ȯ (���纻)
    /// </summary>
    public StoneType[,] GetBoardState()
    {
        StoneType[,] boardCopy = new StoneType[boardSize, boardSize];
        Array.Copy(board, boardCopy, board.Length);
        return boardCopy;
    }
}