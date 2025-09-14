using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OmokBoard : MonoBehaviour
{
    public const int BoardSize = 15;
    public const int White = 1;
    public const int Black = 2;

    public int[,] board = new int[BoardSize, BoardSize];
    public int countStone;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //���� ���� ��ǥ ī����
        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                board[i, j] = 0;
            }
        }
    }

    // �ٵ��� ���� Ȯ��
    public bool CheckRange(int x, int y)
    {
        return (x >= 0 && y >= 0 && x < BoardSize && y < BoardSize);
    }

    // �ٵϵ� ���� 
    public bool StoneSet(int x, int y, int value)
    {
        if (CheckRange(x, y))
        {
            board[x, y] = value;
            return true;
        }
        else
        {
            return false;
        }
    }
    // ���� �ϼ� Ȯ��
    public bool CheckWin(int _stone)
    {
        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                int buff = board[i, j];

                if (buff == _stone && buff != 0)
                {
                    countStone = 1;

                    // ������ ���� Ȯ��
                    if (CheckRange(i + 1, j) && board[i + 1, j] == buff)
                    {
                        countStone++;

                        for (int k = 2; k < 5; k++)
                        {
                            if (CheckRange(i + k, j) && board[i + k, j] == buff)
                            {
                                countStone++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (countStone == 5)
                        return true;
                    else
                        countStone = 1;

                    // �Ʒ��� ���� Ȯ��
                    if (CheckRange(i, j + 1) && board[i, j + 1] == buff)
                    {
                        countStone++;

                        for (int k = 2; k < 5; k++)
                        {
                            if (CheckRange(i, j + k) && board[i, j + k] == buff)
                            {
                                countStone++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (countStone == 5)
                        return true;
                    else
                        countStone = 1;

                    // ������ �Ʒ� �밢�� ���� Ȯ��
                    if (CheckRange(i + 1, j + 1) && board[i + 1, j + 1] == buff)
                    {
                        countStone++;

                        for (int k = 2; k < 5; k++)
                        {
                            if (CheckRange(i + k, j + k) && board[i + k, j + k] == buff)
                            {
                                countStone++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (countStone == 5)
                        return true;
                    else
                        countStone = 1;

                    // ������ �� �밢�� ���� Ȯ��
                    if (CheckRange(i + 1, j - 1) && board[i + 1, j - 1] == buff)
                    {
                        countStone++;

                        for (int k = 2; k < 5; k++)
                        {
                            if (CheckRange(i + k, j - k) && board[i + k, j - k] == buff)
                            {
                                countStone++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    if (countStone == 5)
                        return true;
                }
            }
        }
        return false;
    }
}
