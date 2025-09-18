using System;
using UnityEngine;

public class RankingUI : MonoBehaviour
{
    private MainSceneUIManager mainSceneUIManager => MainSceneUIManager.Instance;

    [SerializeField] [Tooltip("랭킹 정보가 들어갈 오브젝트")]
    private GameObject rankingItemPrefab;

    [SerializeField] [Tooltip("랭킹이 표시될 부모 오브젝트 트랜스폼(Content)")]
    private Transform rankingContentObject;

    private void OnEnable()
    {
        mainSceneUIManager.statsManager.GetRanking((response) =>
            {
                foreach (var user in response.ranking)
                {
                    var item = Instantiate(rankingItemPrefab, rankingContentObject);
                    var rankingItem = item.GetComponent<RankingItem>();
                    rankingItem?.SetData(user.rank, user.profileImage, user.grade, user.identity.nickname,
                        user.record.totalWins, user.record.totalLoses);
                }
            },
            (errorType) =>
            {
                mainSceneUIManager.OpenOneButtonPopup("랭킹 정보를 가져올 수 없습니다.",
                    () => { mainSceneUIManager.OpenMainPanel(); });
            });
    }
}