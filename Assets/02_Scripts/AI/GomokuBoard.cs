using System;
using UnityEngine;

public enum Stone
{
    Empty = 0,
    Black = 1,
    White = 2
}

public class GomokuBoard
{
    public const int SIZE = 15;
    private readonly Stone[,] _board = new Stone[SIZE, SIZE];

    public bool IsInside(int x, int y)
    {
        return x >= 0 && x < SIZE && y >= 0 && y < SIZE;
    }

    public Stone GetStone(int x, int y)
    {
        if (!IsInside(x, y))
            throw new ArgumentOutOfRangeException($"({x},{y}) is outside board.");
        return _board[x, y];
    }

    public bool IsEmpty(int x, int y)
    {
        return IsInside(x, y) && _board[x, y] == Stone.Empty;
    }

    public void PlaceStone(int x, int y, Stone stone)
    {
        if (!IsInside(x, y))
            throw new ArgumentOutOfRangeException($"({x},{y}) is outside board.");
        _board[x, y] = stone;
    }
}