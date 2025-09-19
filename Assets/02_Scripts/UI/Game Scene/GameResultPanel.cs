using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameResultPanel : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField] private Button exitButton;

    [SerializeField] private Button rematchButton;

    [Header("승리, 패배 문구")]
    [SerializeField] private Image titleImage;

    [SerializeField] private Sprite[] titleImages; // 0: 승리, 1: 패배, 2: 무승부

    [Header("포인트 관련")]
    [SerializeField] private Sprite pointPanelSprite1to4; // 1 ~ 4급에 사용할 패널

    [SerializeField] private Sprite pointPanelSprite5to9; // 5 ~ 9급에 사용할 패널
    [SerializeField] private Sprite pointPanelSprite10to18; // 10 ~ 18급에 사용할 패널
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private GameObject minusPointPanel;
    [SerializeField] private GameObject plusPointPanel;
    [SerializeField] private Image minusPoint;
    [SerializeField] private Image plusPoint;
    [SerializeField] private TextMeshProUGUI changedPoint;
    [SerializeField] private TextMeshProUGUI pointsToNextLevel;


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
                changedPoint.text = "10 승급 포인트를 잃었습니다.";
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
                changedPoint.text = "10 승급 포인트를 받았습니다.";
                scorePanel.SetActive(true);
                break;
        }
    }

    private void SetPoint(GameResultResponse response)
    {
        if (!scorePanel.activeSelf) return; // 스코어 패널이 활성화 되었을때에만 아래의 로직을 실행

        int grade = response.rank.grade;
        int point = response.rank.points;
        int maxPointsForGrade = response.rank.maxPointsForGrade;
        int pointsForRankUp = maxPointsForGrade - point;

        // 등급별로 포인트 바의 숫자가 다른 스프라이트 사용
        Image scorePanelImage = scorePanel.GetComponent<Image>();
        if (grade >= 10) scorePanelImage.sprite = pointPanelSprite10to18;
        else if (grade >= 5) scorePanelImage.sprite = pointPanelSprite5to9;
        else scorePanelImage.sprite = pointPanelSprite1to4;

        // TODO: 포인트를 받아와서 승급까지 남은 포인트를 계산하여 텍스트로 출력하고,
        // TODO: 계산 후 최종 포인트에 따라 게이지 바 조절
        pointsToNextLevel.text = $"{pointsForRankUp} 게임만 승리하면 승급합니다.";

        minusPointPanel.SetActive(point < 0);
        plusPointPanel.SetActive(point > 0);

        float progress = Mathf.Abs((float)point) / maxPointsForGrade;
        if (point < 0) minusPoint.fillAmount = progress;                
        else if (point > 0) plusPoint.fillAmount = progress;
    }

    /// <summary>
    /// 외부에서 OneButtonPanel의 메세지와 버튼 클릭 시 수행할 액션을 설정하는 메소드
    /// </summary>
    /// <param name="response">게임 결과에 따른 서버 반환값</param>
    /// <param name="result">승리 결과</param>
    /// <param name="onClickExit">종료 버튼을 눌렀을 때 실행할 액션</param>
    /// <param name="onClickRematch">재대국 버튼을 눌렀을 때 실행할 액션</param>
    public void OpenWithButtonEvent(GameResultResponse response, GameResult result, Action onClickExit,
        Action onClickRematch)
    {
        SetMessage(result);
        SetPoint(response);
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