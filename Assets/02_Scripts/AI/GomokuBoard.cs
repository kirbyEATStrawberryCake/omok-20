using System;
using UnityEngine;

// StoneType을 사용
// public enum Stone
// {
//     Empty = 0,
//     Black = 1,
//     White = 2
// }

public class GomokuBoard
{
    // public const int SIZE = 15;
    // private readonly StoneType[,] _board = new StoneType[SIZE, SIZE];

    // BoardManager.IsValidPosition 로 대체
    // public bool IsInside(int x, int y)
    // {
    //     return x >= 0 && x < SIZE && y >= 0 && y < SIZE;
    // }

    // BoardManager.GetStoneAt 로 대체
    // public StoneType GetStone(int x, int y)
    // {
    //     if (!IsInside(x, y))
    //         throw new ArgumentOutOfRangeException($"({x},{y}) is outside board.");
    //     return board[x, y];
    // }

    // 
    // public bool IsEmpty(int x, int y)
    // {
    //     return IsInside(x, y) && _board[x, y] == StoneType.None;
    // }
    
    
    //BoardManager.TryPlaceStone 로 대체
    // public void PlaceStone(int x, int y, StoneType stone)
    // {
    //     if (!IsInside(x, y))
    //         throw new ArgumentOutOfRangeException($"({x},{y}) is outside board.");
    //     _board[x, y] = stone;
    // }
}