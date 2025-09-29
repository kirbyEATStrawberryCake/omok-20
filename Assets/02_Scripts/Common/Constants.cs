using UnityEngine;

public static class Constants
{
    public static int boardSize = 15;     // 오목판 크기 (15x15)
    public static float cellSize = 0.45f; // 각 칸의 크기
    public static Vector2 boardOffset = new Vector2(0, 1.8f);

    public static int[,] directions =
    {
        { -1, -1 }, { -1, 0 }, { -1, 1 },
        { 0, -1 }, { 0, 1 },
        { 1, -1 }, { 1, 0 }, { 1, 1 }
    };
}