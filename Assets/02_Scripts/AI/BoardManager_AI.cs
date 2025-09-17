using System;
using UnityEngine;

public class BoardManager_AI : BoardManager
{
    /// <summary>
    /// 해당 위치에 돌이 없는지 확인
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool IsEmpty(int x, int y)
    {
        return IsValidPosition(x, y) && board[x, y] == StoneType.None;
    }
    
    /// <summary>
    /// 논리적 보드에 돌 정보 저장
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="stone"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void PlaceStone_Logical(int x, int y, StoneType stone)
    {
        if (!IsValidPosition(x, y))
            throw new ArgumentOutOfRangeException($"({x},{y}) is outside board.");
        board[x, y] = stone;
    }

    /// <summary>
    /// 보드에 돌을 놓을 공간이 없는지 확인
    /// </summary>
    /// <returns></returns>
    public bool IsBoardFull()
    {
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                if (board[x, y] == StoneType.None)
                {
                    return false;
                }
            }
        }
        return true;
    }
}
