using System;
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

        // TODO: 포인트를 받아와서 승급까지 남은 포인트를 계산하여 텍스트로 출력하고,
        // TODO: 계산 후 최종 포인트에 따라 게이지 바 조절
        pointsToNextLevel.text = "";
    }

    /// <summary>
    /// 외부에서 OneButtonPanel의 메세지와 버튼 클릭 시 수행할 액션을 설정하는 메소드
    /// </summary>
    /// <param name="result">승리 결과</param>
    /// <param name="onClickExit"></param>
    /// <param name="onClickRematch"></param>
    public void OpenWithButtonEvent(GameResult result, Action onClickExit, Action onClickRematch)
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