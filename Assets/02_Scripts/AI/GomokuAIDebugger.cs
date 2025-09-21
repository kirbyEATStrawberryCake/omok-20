using System.Collections.Generic;
using UnityEngine;

public class GomokuAIDebugger : MonoBehaviour
{
    // [디버그용] 착수 후보 시각화를 위한 변수들
    public GameObject debugTextPrefab;
    public GameObject debugSelectPrefab;
    private List<GameObject> debugObjects = new List<GameObject>(); // 생성된 디버그 오브젝트 리스트
    [SerializeField] bool isDebug = true;

    private GomokuAI gomokuAI;

    private void Start()
    {
        InstantiateGomokuAI();
    }

    public void InstantiateGomokuAI()
    {
        // GomokuAI 인스턴스에 프리팹 참조 전달
        gomokuAI = new GomokuAI(GamePlayManager.Instance.BoardManager,
            GamePlayManager.Instance.GameLogicController.GetCurrentStone() == StoneType.Black
                ? StoneType.White
                : StoneType.Black, debugTextPrefab, debugSelectPrefab, isDebug);
    }

    public void InstantiateGomokuAI(StoneType aiStoneType)
    {
        gomokuAI = new GomokuAI(GamePlayManager.Instance.BoardManager,
            aiStoneType,
            debugTextPrefab, debugSelectPrefab, isDebug);
    }

    // // GomokuAI 클래스의 GetNextMove를 호출하는 메서드
    public Vector2Int GetNextMoveFromAI()
    {
        return gomokuAI.GetNextMove();
    }
}