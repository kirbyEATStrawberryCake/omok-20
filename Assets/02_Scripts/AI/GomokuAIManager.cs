using System.Collections.Generic;
using UnityEngine;

public class GomokuAIManager : MonoBehaviour
{
    // [디버그용] 착수 후보 시각화를 위한 변수들
    public GameObject debugTextPrefab;
    public GameObject debugSelectPrefab;
    private List<GameObject> debugObjects = new List<GameObject>(); // 생성된 디버그 오브젝트 리스트
    [SerializeField] bool isDebug = true;

    private GomokuAI gomokuAI;
    [SerializeField] private BoardManager boardManager; // 디버깅 시각화를 위해 참조

    public void InstantiateGomokuAI(StoneType aiStone)
    {
        // GomokuAI 인스턴스에 프리팹 참조 전달
        gomokuAI = new GomokuAI(aiStone);
    }

    public AIResult GetAIResult(StoneType[,] currentBoardState)
    {
        AIResult result = gomokuAI.GetNextMove(currentBoardState);

        // 디버깅이 켜져있으면, 받은 결과의 모든 후보 점수를 시각화합니다.
        if (isDebug && Application.isPlaying) { ShowDebugInfo(result.candidateMovesWithScores); }

        return result;
    }

    private void ShowDebugInfo(List<CandidateInfo> candidateInfos)
    {
        // 기존 디버그 오브젝트 삭제
        foreach (var obj in debugObjects) { Destroy(obj); }

        debugObjects.Clear();

        if (candidateInfos == null) return;

        foreach (var info in candidateInfos)
        {
            Vector3 worldPos = boardManager.BoardToWorldPosition(info.move.x, info.move.y);

            // 점수 텍스트 생성
            var textObj = Instantiate(debugTextPrefab, worldPos, Quaternion.identity, transform);
            textObj.GetComponent<TextMesh>().text = info.score.ToString();
            debugObjects.Add(textObj);
        }
    }
}