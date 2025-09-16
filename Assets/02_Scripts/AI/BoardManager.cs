using UnityEngine;

public class BoardManager_AI : BoardManager
{
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
    public bool IsEmpty(int x, int y) // 이거 꼭 bool 반환으로 바꿔야함!!!!
    {
        //return IsInside(x, y) && _board[x, y] == StoneType.None;
        return true;
    }
    
    
    //BoardManager.TryPlaceStone 로 대체
    // public void PlaceStone(int x, int y, StoneType stone)
    // {
    //     if (!IsInside(x, y))
    //         throw new ArgumentOutOfRangeException($"({x},{y}) is outside board.");
    //     _board[x, y] = stone;
    // }
}
