using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RankingUI : MonoBehaviour
{
    [SerializeField] private ScrollViewController scrollViewController;

    private MainSceneUIManager mainSceneUIManager;
    private StatsManager statsManager;


    private void Awake()
    {
        mainSceneUIManager = MainSceneUIManager.Instance;
        statsManager = NetworkManager.Instance.statsManager;
    }

    private void OnEnable()
    {
        statsManager.GetRanking(
            (response) => scrollViewController.Initialize(response.ranking.ToList()),
            (errorType) =>
            {
                mainSceneUIManager.OpenOneButtonPopup("랭킹 정보를 가져올 수 없습니다.",
                    () => { mainSceneUIManager.OpenPanel(); });
            });
    }

    public void OnClickBackButton()
    {
        mainSceneUIManager.OpenTwoButtonPopup("랭킹을 종료하시겠습니까?", () => mainSceneUIManager.OpenPanel());
    }
}