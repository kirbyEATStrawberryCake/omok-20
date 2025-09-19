using UnityEngine;

public class GomokuAIDebugger : MonoBehaviour
{
    public GameObject debugTextPrefab;
    public GameObject debugSelectPrefab;

    private GomokuAI gomokuAI;

    public void InstantiateGomokuAI(BoardManager board, StoneType myStone)
    {
        // GomokuAI 인스턴스에 프리팹 참조 전달
        gomokuAI = new GomokuAI(board, myStone, debugTextPrefab, debugSelectPrefab);
    }
    // // GomokuAI 클래스의 GetNextMove를 호출하는 메서드
    public Vector2Int GetNextMoveFromAI()
    {
        return gomokuAI.GetNextMove();
    }
}
