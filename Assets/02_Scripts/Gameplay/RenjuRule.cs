using System;
using UnityEngine;

public class RenjuRule : MonoBehaviour
{
    [Header("Renju Rule Settings")]
    public bool enableForbiddenMoves = true;    // �ݼ� ���� ����
    public bool showForbiddenPositions = false; // �ݼ� ��ġ �ð��� ǥ�� ����

    // �Ŵ��� ���� (ȿ������ ����)
    private GamePlayManager gamePlayManager;
    private BoardManager boardManager;

    // 8���� ���� (�����¿�, �밢��)
    private readonly int[,] directions = {
        {-1, -1}, {-1, 0}, {-1, 1},  // ���� ��, ��, ������ ��
        {0, -1},           {0, 1},    // ����,     ������
        {1, -1},  {1, 0},  {1, 1}     // ���� �Ʒ�, �Ʒ�, ������ �Ʒ�
    };

    private void Start()
    {
        gamePlayManager = GamePlayManager.Instance;
        boardManager = gamePlayManager?.boardManager;
    }

    /// <summary>
    /// ���ַ� �ʱ�ȭ
    /// </summary>
    public void Initialize()
    {
        if (boardManager == null)
        {
            Debug.LogError("BoardManager ������ �������� �ʾҽ��ϴ�!");
        }

        Debug.Log("���ַ��� �ʱ�ȭ�Ǿ����ϴ�.");
    }

    /// <summary>
    /// ��ȿ�� ������ �˻� (���ַ� ����)
    /// </summary>
    /// <param name="x">�������� ��ġ x��ǥ</param>
    /// <param name="y">�������� ��ġ y��ǥ</param>
    /// <param name="stoneType">�������� ���� Ÿ��</param>
    /// <param name="board">���� ���� ����</param>
    /// <returns>��ȿ�� ������ ����</returns>
    public bool IsValidMove(int x, int y, StoneType stoneType, StoneType[,] board)
    {
        // �鵹�� ���ַ��� ������ ���� ����
        if (stoneType != StoneType.Black) return true;

        // �ݼ��� ��Ȱ��ȭ�� ��� ��� �� ���
        if (!enableForbiddenMoves) return true;

        // �ӽ÷� ���� ���� �ݼ� �˻�
        board[x, y] = stoneType;

        bool isValid = true;

        // ��� �ݼ� �˻�
        if (IsDoubleThree(x, y, board))
        {
            isValid = false;
            Debug.Log($"��� �ݼ�: ({x}, {y})");
        }

        // ��� �ݼ� �˻�
        if (isValid && IsDoubleFour(x, y, board))
        {
            isValid = false;
            Debug.Log($"��� �ݼ�: ({x}, {y})");
        }

        // ��� �ݼ� �˻� (6�� �̻�)
        if (isValid && IsOverline(x, y, board))
        {
            isValid = false;
            Debug.Log($"��� �ݼ�: ({x}, {y})");
        }

        // �ӽ÷� ���� �� ����
        board[x, y] = StoneType.None;

        return isValid;
    }

    /// <summary>
    /// ��� �ݼ� �˻�
    /// </summary>
    private bool IsDoubleThree(int x, int y, StoneType[,] board)
    {
        int threeCount = 0;

        // 4���⿡�� Ȱ��(���� 3��) �˻�
        for (int dir = 0; dir < 4; dir++)
        {
            int dx = directions[dir, 0];
            int dy = directions[dir, 1];

            if (IsOpenThree(x, y, dx, dy, board))
            {
                threeCount++;
            }
        }

        return threeCount >= 2; // 2�� �̻��� Ȱ���� ������ ���
    }

    /// <summary>
    /// ��� �ݼ� �˻�
    /// </summary>
    private bool IsDoubleFour(int x, int y, StoneType[,] board)
    {
        int fourCount = 0;

        // 4���⿡�� 4�� �˻�
        for (int dir = 0; dir < 4; dir++)
        {
            int dx = directions[dir, 0];
            int dy = directions[dir, 1];

            if (IsFour(x, y, dx, dy, board))
            {
                fourCount++;
            }
        }

        return fourCount >= 2; // 2�� �̻��� 4���� ������ ���
    }

    /// <summary>
    /// ���(6�� �̻�) �˻�
    /// </summary>
    private bool IsOverline(int x, int y, StoneType[,] board)
    {
        // 4���⿡�� 6�� �̻� ���� �˻�
        for (int dir = 0; dir < 4; dir++)
        {
            int dx = directions[dir, 0];
            int dy = directions[dir, 1];

            int count = 1; // ���� �� ����
            count += CountConsecutiveStones(x, y, dx, dy, StoneType.Black, board);
            count += CountConsecutiveStones(x, y, -dx, -dy, StoneType.Black, board);

            if (count >= 6)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Ȱ��(���� 3��) �˻�
    /// </summary>
    private bool IsOpenThree(int x, int y, int dx, int dy, StoneType[,] board)
    {
        int count = 1; // ���� �� ����

        // �� �������� ���ӵ� �浹 ����
        count += CountConsecutiveStones(x, y, dx, dy, StoneType.Black, board);

        // �ݴ� �������� ���ӵ� �浹 ����
        count += CountConsecutiveStones(x, y, -dx, -dy, StoneType.Black, board);

        // 3���̰� ���� ���� ����־�� Ȱ��
        if (count == 3)
        {
            // ���� ���� ����ִ��� Ȯ��
            int frontX = x + dx * (CountConsecutiveStones(x, y, dx, dy, StoneType.Black, board) + 1);
            int frontY = y + dy * (CountConsecutiveStones(x, y, dx, dy, StoneType.Black, board) + 1);

            int backX = x - dx * (CountConsecutiveStones(x, y, -dx, -dy, StoneType.Black, board) + 1);
            int backY = y - dy * (CountConsecutiveStones(x, y, -dx, -dy, StoneType.Black, board) + 1);

            bool frontEmpty = IsValidPosition(frontX, frontY) && board[frontX, frontY] == StoneType.None;
            bool backEmpty = IsValidPosition(backX, backY) && board[backX, backY] == StoneType.None;

            return frontEmpty && backEmpty;
        }

        return false;
    }

    /// <summary>
    /// 4�� �˻�
    /// </summary>
    private bool IsFour(int x, int y, int dx, int dy, StoneType[,] board)
    {
        int count = 1; // ���� �� ����

        // �� �������� ���ӵ� �浹 ����
        count += CountConsecutiveStones(x, y, dx, dy, StoneType.Black, board);

        // �ݴ� �������� ���ӵ� �浹 ����
        count += CountConsecutiveStones(x, y, -dx, -dy, StoneType.Black, board);

        return count == 4;
    }

    /// <summary>
    /// Ư�� �������� ���ӵ� ������ ���� ������ ���� �޼���
    /// </summary>
    /// <param name="startX">���� x��ǥ</param>
    /// <param name="startY">���� y��ǥ</param>
    /// <param name="dx">x���� ������</param>
    /// <param name="dy">y���� ������</param>
    /// <param name="stoneType">ã�� ���� Ÿ��</param>
    /// <param name="board">���� ����</param>
    /// <returns>���ӵ� ���� ����</returns>
    public int CountConsecutiveStones(int startX, int startY, int dx, int dy, StoneType stoneType, StoneType[,] board)
    {
        int count = 0;
        int x = startX + dx;
        int y = startY + dy;

        while (IsValidPosition(x, y) && board[x, y] == stoneType)
        {
            count++;
            x += dx;
            y += dy;
        }

        return count;
    }

    /// <summary>
    /// ��ȿ�� ���� ��ġ���� Ȯ��
    /// </summary>
    private bool IsValidPosition(int x, int y)
    {
        return boardManager.IsValidPosition(x, y);
    }

    /// <summary>
    /// ��� �ݼ� ��ġ ��ȯ (AI�� ��Ʈ �ý��ۿ�)
    /// </summary>
    /// <param name="board">���� ���� ����</param>
    /// <returns>�ݼ� ��ġ ����Ʈ</returns>
    public System.Collections.Generic.List<Vector2Int> GetForbiddenPositions(StoneType[,] board)
    {
        var forbiddenPositions = new System.Collections.Generic.List<Vector2Int>();

        if (!enableForbiddenMoves) return forbiddenPositions;

        for (int x = 0; x < board.GetLength(0); x++)
        {
            for (int y = 0; y < board.GetLength(1); y++)
            {
                if (board[x, y] == StoneType.None)
                {
                    if (!IsValidMove(x, y, StoneType.Black, board))
                    {
                        forbiddenPositions.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        return forbiddenPositions;
    }

    /// <summary>
    /// ���ַ� ���� ����
    /// </summary>
    /// <param name="enableForbidden">�ݼ� ��Ģ Ȱ��ȭ ����</param>
    public void SetRenjuRuleEnabled(bool enableForbidden)
    {
        enableForbiddenMoves = enableForbidden;
        Debug.Log($"���ַ� �ݼ�: {(enableForbidden ? "Ȱ��ȭ" : "��Ȱ��ȭ")}");
    }
}