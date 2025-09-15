using UnityEngine;
using System.Collections.Generic;
using System;

public class BoardManager : MonoBehaviour
{
    [Header("Board Settings")]
    public GameObject stonePrefab;              // �� ������
    public Transform boardParent;               // ������ �θ� ������Ʈ
    public int boardSize = 15;                  // ������ ũ�� (15x15)
    public float cellSize = 1.0f;               // �� ĭ�� ũ��

    private StoneType[,] board;                 // ������ �迭 (���� ����)
    private GameObject[,] stoneObjects;         // �� ������Ʈ �迭 (�ð��� ����)
    private Stack<Move> moveHistory;            // �� ����� ���� ����

    // ���� ������ �� �߻��ϴ� �̺�Ʈ
    public event Action<int, int, StoneType> OnStonePlace;

    /// <summary>
    /// �� ���� ��Ÿ���� ����ü
    /// </summary>
    [System.Serializable]
    public struct Move
    {
        public int x, y;                        // ���� ��ġ
        public StoneType stoneType;             // ���� ����
        public GameObject stoneObject;          // �� ������Ʈ ����

        public Move(int x, int y, StoneType type, GameObject obj)
        {
            this.x = x;
            this.y = y;
            this.stoneType = type;
            this.stoneObject = obj;
        }
    }

    void Start()
    {
        InitializeBoard();
    }

    /// <summary>
    /// ������ �ʱ�ȭ
    /// </summary>
    public void InitializeBoard()
    {
        // �迭 �ʱ�ȭ
        board = new StoneType[boardSize, boardSize];
        stoneObjects = new GameObject[boardSize, boardSize];
        moveHistory = new Stack<Move>();

        // ���� ���� ����
        ClearBoard();

        // ���콺 Ŭ�� ������ ���� �ݶ��̴� ����
        SetupBoardCollider();

        Debug.Log("�������� �ʱ�ȭ�Ǿ����ϴ�.");
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
        moveHistory.Clear();
    }

    /// <summary>
    /// ���� Ŭ�� ������ ���� �ݶ��̴� ����
    /// </summary>
    private void SetupBoardCollider()
    {
        BoxCollider boardCollider = GetComponent<BoxCollider>();
        if (boardCollider == null)
        {
            boardCollider = gameObject.AddComponent<BoxCollider>();
        }

        // �ݶ��̴� ũ�⸦ ���� ũ�⿡ ����
        boardCollider.size = new Vector3(boardSize * cellSize, 0.1f, boardSize * cellSize);
    }

    /// <summary>
    /// ���콺 Ŭ�� ó��
    /// </summary>
    void OnMouseDown()
    {
        // ���� �Ŵ������� ���� ���� Ȯ��
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager.GetGameState() != GameState.Playing) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Ŭ���� ��ġ�� ���� ��ǥ�� ��ȯ
            Vector3 localPos = transform.InverseTransformPoint(hit.point);
            int x = Mathf.RoundToInt(localPos.x / cellSize + (boardSize - 1) / 2.0f);
            int y = Mathf.RoundToInt(localPos.z / cellSize + (boardSize - 1) / 2.0f);

            // ���� �÷��̾��� �� Ÿ�� ��������
            PlayerManager playerManager = FindObjectOfType<PlayerManager>();
            StoneType currentStone = playerManager.GetCurrentPlayer();

            // �� ���� �õ�
            TryPlaceStone(x, y, currentStone);
        }
    }

    /// <summary>
    /// ������ ��ġ�� �� ���� �õ�
    /// </summary>
    /// <param name="x">x��ǥ</param>
    /// <param name="y">y��ǥ</param>
    /// <param name="stoneType">���� ���� Ÿ��</param>
    /// <returns>���� ���������� �������� ����</returns>
    public bool TryPlaceStone(int x, int y, StoneType stoneType)
    {
        // ���� �˻�
        if (!IsValidPosition(x, y)) return false;

        // �̹� ���� �����ִ��� �˻�
        if (board[x, y] != StoneType.None) return false;

        // ���ַ� �˻� (�浹�� ��츸)
        RenjuRule renjuRule = FindObjectOfType<RenjuRule>();
        GameManager gameManager = FindObjectOfType<GameManager>();

        if (gameManager.isRenjuModeEnabled && stoneType == StoneType.Black)
        {
            if (!renjuRule.IsValidMove(x, y, stoneType, board))
            {
                Debug.Log("���ַ� �������� �ش� ��ġ�� ���� ���� �� �����ϴ�.");
                return false;
            }
        }

        // �� ����
        PlaceStone(x, y, stoneType);
        return true;
    }

    /// <summary>
    /// ������ ���� ���� �޼���
    /// </summary>
    /// <param name="x">x��ǥ</param>
    /// <param name="y">y��ǥ</param>
    /// <param name="stoneType">���� Ÿ��</param>
    private void PlaceStone(int x, int y, StoneType stoneType)
    {
        // ���� ���忡 �� ���� ����
        board[x, y] = stoneType;

        // �ð��� �� ������Ʈ ����
        Vector3 worldPos = GetWorldPosition(x, y);
        GameObject stoneObj = Instantiate(stonePrefab, worldPos, Quaternion.identity, boardParent);

        // �� ������Ʈ ����
        Stone stone = stoneObj.GetComponent<Stone>();
        if (stone != null)
        {
            stone.SetStoneType(stoneType);
            stone.SetPosition(x, y);
        }

        stoneObjects[x, y] = stoneObj;

        // �� ��� ����
        moveHistory.Push(new Move(x, y, stoneType, stoneObj));

        // �̺�Ʈ �߻�
        OnStonePlace?.Invoke(x, y, stoneType);

        Debug.Log($"���� �������ϴ�: ({x}, {y}) - {stoneType}");
    }

    /// <summary>
    /// �������� ���� �� ���� (������)
    /// </summary>
    public void UndoLastMove()
    {
        if (moveHistory.Count > 0)
        {
            Move lastMove = moveHistory.Pop();

            // ���� ���忡�� ����
            board[lastMove.x, lastMove.y] = StoneType.None;

            // �ð��� ������Ʈ ����
            if (lastMove.stoneObject != null)
            {
                DestroyImmediate(lastMove.stoneObject);
            }

            stoneObjects[lastMove.x, lastMove.y] = null;

            Debug.Log($"���� ���ŵǾ����ϴ�: ({lastMove.x}, {lastMove.y})");
        }
    }

    /// <summary>
    /// ���� ��ǥ�� ���� ��ǥ�� ��ȯ
    /// </summary>
    private Vector3 GetWorldPosition(int x, int y)
    {
        float worldX = (x - (boardSize - 1) / 2.0f) * cellSize;
        float worldZ = (y - (boardSize - 1) / 2.0f) * cellSize;
        return transform.position + new Vector3(worldX, 0, worldZ);
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
