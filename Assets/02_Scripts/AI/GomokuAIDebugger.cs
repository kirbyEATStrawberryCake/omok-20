using System.Collections.Generic;
using UnityEngine;

public class GomokuAIDebugger : MonoBehaviour
{
    // [디버그용] 착수 후보 시각화를 위한 변수들
    public GameObject debugTextPrefab;
    public GameObject debugSelectPrefab;
    private List<GameObject> debugObjects = new List<GameObject>(); // 생성된 디버그 오브젝트 리스트

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
