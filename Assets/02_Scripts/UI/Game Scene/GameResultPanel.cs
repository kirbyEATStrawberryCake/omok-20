using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum TitleImageType
{
    Victory,
    Defeat,
    Draw
}

[Serializable]
public class TitleImageMapping
{
    public TitleImageType type;
    public Sprite sprite;
}

public class GameResultPanel : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField] private Button exitButton;
    [SerializeField] private Button rematchButton;

    [Header("승리, 패배 문구")]
    [SerializeField] private Image titleImage;
    [SerializeField] private TitleImageMapping[] titleImages;
    private Dictionary<TitleImageType, Sprite> titleDictionary = new Dictionary<TitleImageType, Sprite>();

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

    public UnityEvent onExitClicked;
    public UnityEvent onRematchClicked;

    private bool isInitialized;

    /// <summary>
    /// 외부에서 OneButtonPanel의 메세지와 버튼 클릭 시 수행할 액션을 설정하는 메소드
    /// </summary>
    /// <param name="result">승리 결과</param>
    /// <param name="response">게임 결과에 따른 서버 반환값(멀티플레이 시)</param>
    public void Show(GameResult result, GameResultResponse response = null)
    {
        Initialize();

        gameObject.SetActive(true);
        InitPanel();
        SetMessage(result);
        if (response != null) SetPoint(response);

        rematchButton.interactable = false;
        exitButton.interactable = false;
        StartCoroutine(EnableButtonWithDelay());
    }

    public void DisableRematchButton()
    {
        rematchButton.interactable = false;
    }

    private void Initialize()
    {
        if (isInitialized) return;

        foreach (TitleImageMapping t in titleImages)
        {
            titleDictionary[t.type] = t.sprite;
        }

        exitButton.onClick.RemoveAllListeners();
        exitButton.onClick.AddListener(() => onExitClicked?.Invoke());
        rematchButton.onClick.RemoveAllListeners();
        rematchButton.onClick.AddListener(() =>
        {
            DisableRematchButton();
            onRematchClicked?.Invoke();
        });

        isInitialized = true;
    }

    private void InitPanel()
    {
        rematchButton.interactable = true;
    }

    private void SetMessage(GameResult result)
    {
        switch (result)
        {
            case GameResult.Victory:
                titleImage.sprite = titleDictionary[TitleImageType.Victory];
                changedPoint.text = "10 승급 포인트를 받았습니다.";
                scorePanel.SetActive(true);
                break;
            case GameResult.Defeat:
                titleImage.sprite = titleDictionary[TitleImageType.Defeat];
                changedPoint.text = "10 승급 포인트를 잃었습니다.";
                scorePanel.SetActive(true);
                break;
            case GameResult.Draw:
                titleImage.sprite = titleDictionary[TitleImageType.Draw];
                changedPoint.text = "0 승급 포인트를 받았습니다.";
                scorePanel.SetActive(true);
                break;
            case GameResult.Player1Win:
                titleImage.sprite = titleDictionary[TitleImageType.Victory];
                changedPoint.text = "플레이어1이 이겼습니다.";
                pointsToNextLevel.text = "";
                scorePanel.SetActive(false);
                break;
            case GameResult.Player2Win:
                titleImage.sprite = titleDictionary[TitleImageType.Victory];
                changedPoint.text = "플레이어2가 이겼습니다.";
                pointsToNextLevel.text = "";
                scorePanel.SetActive(false);
                break;
            case GameResult.Disconnect:
                titleImage.sprite = titleDictionary[TitleImageType.Victory];
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

        pointsToNextLevel.text = $"{pointsForRankUp} 게임만 승리하면 승급합니다.";

        minusPointPanel.SetActive(point < 0);
        plusPointPanel.SetActive(point > 0);

        float progress = Mathf.Abs((float)point) / maxPointsForGrade;
        if (point < 0) minusPoint.fillAmount = progress;
        else if (point > 0) plusPoint.fillAmount = progress;
    }


    private IEnumerator EnableButtonWithDelay()
    {
        yield return new WaitForSeconds(0.5f);
        rematchButton.interactable = true;
        exitButton.interactable = true;
    }
}