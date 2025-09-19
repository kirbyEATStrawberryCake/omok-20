using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameResultPanel : MonoBehaviour
{
    [SerializeField] private Button exitButton;
    [SerializeField] private Button rematchButton;

    [SerializeField] private Image titleImage;
    [SerializeField] private TextMeshProUGUI changedPoint;
    [SerializeField] private TextMeshProUGUI pointsToNextLevel;

    [SerializeField] private Sprite[] titleImages;  // 0: 승리, 1: 패배, 2: 무승부

    [SerializeField] private GameObject scorePanel;
    [SerializeField] private Image minusPoint;
    [SerializeField] private Image plusPoint;

    private PointsManager pointsManager;

    private void Awake()
    {
        pointsManager = FindFirstObjectByType<PointsManager>(); 
    }

    private void SetButtonEvent(UnityAction onClickExit, UnityAction onClickRematch)
    {
        if (onClickExit == null || onClickRematch == null) return;

        exitButton.onClick.RemoveAllListeners();
        exitButton.onClick.AddListener(onClickExit);

        rematchButton.onClick.RemoveAllListeners();
        rematchButton.onClick.AddListener(onClickRematch);
    }

    private void SetMessage(GameResult result)
    {
        switch (result)
        {
            case GameResult.Victory:
                titleImage.sprite = titleImages[0];
                changedPoint.text = "10 승급 포인트를 받았습니다.";
                scorePanel.SetActive(true);
                break;
            case GameResult.Defeat:
                titleImage.sprite = titleImages[1];
                changedPoint.text = "10 승급 포인트를 받았습니다.";
                scorePanel.SetActive(true);
                break;
            case GameResult.Draw:
                titleImage.sprite = titleImages[2];
                changedPoint.text = "0 승급 포인트를 받았습니다.";
                scorePanel.SetActive(true);
                break;
            case GameResult.Player1Win:
                titleImage.sprite = titleImages[0];
                changedPoint.text = "플레이어1이 이겼습니다.";
                scorePanel.SetActive(false);
                break;
            case GameResult.Player2Win:
                titleImage.sprite = titleImages[0];
                changedPoint.text = "플레이어2가 이겼습니다.";
                scorePanel.SetActive(false);
                break;
            case GameResult.Disconnect:
                titleImage.sprite = titleImages[0];
                changedPoint.text = "상대방이 나갔습니다.\n10 승급 포인트를 받았습니다.";
                scorePanel.SetActive(true);
                break;
        }
        // TODO: 계산 후 최종 포인트에 따라 게이지 바 조절 < 계속 해봤는데 안되네요ㅠ 죄송...
        LoadAndShowPointsToNextLevel();
    }

    private void LoadAndShowPointsToNextLevel()
    {
        if (pointsManager == null)
        {
            Debug.LogError("PointsManager를 찾을 수 없습니다.");
            return;
        }

        pointsManager.GetPoints((getPoints) =>
        {
            int currentPoints = getPoints.points; 
            int nextPromotion = GetNextPromotionPoint(currentPoints);

            int winsNeeded = Mathf.Max(0, nextPromotion - currentPoints);
            if (winsNeeded == 0)
                pointsToNextLevel.text = "승급 조건을 달성했습니다!";
            else
                pointsToNextLevel.text = $"{winsNeeded} 게임 승리 시 승급합니다.";

        }, (fail) =>
        {
            Debug.LogError("포인트 정보를 가져오는 데 실패했습니다.");
            pointsToNextLevel.text = "포인트 정보를 불러올 수 없습니다.";
        });
    }

    private int GetNextPromotionPoint(int currentPoint)
    {
        if (currentPoint < 3) return 3;
        else if (currentPoint < 5) return 5;
        else if (currentPoint < 10) return 10;
        else return currentPoint + 5;
    }

    /// <summary>
    /// 외부에서 OneButtonPanel의 메세지와 버튼 클릭 시 수행할 액션을 설정하는 메소드
    /// </summary>
    /// <param name="result">승리 결과</param>
    /// <param name="onClickExit"></param>
    /// <param name="onClickRematch"></param>
    /// 
    public void OpenWithButtonEvent(GameResult result, System.Action onClickExit, System.Action onClickRematch)
    {
        SetMessage(result);

        SetButtonEvent(() =>
        {
            onClickExit?.Invoke();
            gameObject.SetActive(false);
        }, () =>
        {
            onClickRematch?.Invoke();
            gameObject.SetActive(false);
        });

        gameObject.SetActive(true);
    }
}
