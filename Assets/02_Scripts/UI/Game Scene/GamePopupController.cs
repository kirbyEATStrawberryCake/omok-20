using System;
using UnityEngine;
using UnityEngine.Events;

public class GamePopupController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] [Tooltip("게임 씬에서 사용할 확인 버튼 1개짜리 팝업")] private GameObject oneConfirmButtonPopupPanel;
    [SerializeField] [Tooltip("메인 씬에서 사용할 취소 버튼 1개짜리 팝업")] private GameObject oneCancelButtonPopupPanel;
    [SerializeField] [Tooltip("게임 씬에서 사용할 버튼 2개짜리 팝업")] private GameObject twoButtonPopupPanel;
    [SerializeField] [Tooltip("게임 결과창 팝업")] private GameObject gameResultPopupPanel;

    private OneButtonPanel oneConfirmButtonPopup;
    private OneButtonPanel oneCancelButtonPopup;
    private TwoButtonPanel twoButtonPopup;
    public GameResultPanel gameResultPopup { get; private set; }

    #region Unity Life Cycle

    private void Awake()
    {
        if (oneConfirmButtonPopupPanel != null)
            oneConfirmButtonPopup = oneConfirmButtonPopupPanel.GetComponent<OneButtonPanel>();
        if (oneCancelButtonPopupPanel != null)
            oneCancelButtonPopup = oneCancelButtonPopupPanel.GetComponent<OneButtonPanel>();
        if (twoButtonPopupPanel != null)
            twoButtonPopup = twoButtonPopupPanel.GetComponent<TwoButtonPanel>();
        if (gameResultPopupPanel != null)
            gameResultPopup = gameResultPopupPanel.GetComponent<GameResultPanel>();
    }

    #endregion

    #region 팝업 열기

    /// <summary>
    /// 확인 버튼 팝업 열기
    /// </summary>
    /// <param name="message">팝업창에 띄울 메세지</param>
    /// <param name="onConfirm">확인 버튼 클릭 시 실행할 액션</param>
    /// <param name="delay">확인 버튼이 활성화 되기까지 걸리는 시간(기본 0초)</param>
    public void OpenOneConfirmButtonPopup(string message, Action onConfirm, float delay = 0f)
    {
        oneConfirmButtonPopup.Show<OneButtonPanel>(message)
            .OnConfirm(onConfirm, delay);
    }

    /// <summary>
    /// 취소 버튼 팝업 열기
    /// </summary>
    /// <param name="message">팝업창에 띄울 메세지</param>
    /// <param name="onConfirm">취소 버튼 클릭 시 실행할 액션</param>
    /// <param name="delay">취소 버튼이 활성화 되기까지 걸리는 시간(기본 0초)</param>
    public void OpenOneCancelButtonPopup(string message, Action onConfirm, float delay = 0f)
    {
        oneCancelButtonPopup.Show<OneButtonPanel>(message)
            .OnConfirm(onConfirm, delay);
    }

    /// <summary>
    /// 버튼 2개짜리 팝업 열기
    /// </summary>
    /// <param name="message">팝업창에 띄울 메세지</param>
    /// <param name="confirmButtonText">확인 버튼 문구</param>
    /// <param name="cancelButtonText">취소 버튼 문구</param>
    /// <param name="onConfirm">확인 버튼 클릭 시 실행할 액션</param>
    /// <param name="onCancel">취소 버튼 클릭 시 실행할 액션</param>
    public void OpenTwoButtonPopup(string message, string confirmButtonText, string cancelButtonText, Action onConfirm,
        Action onCancel)
    {
        twoButtonPopup.Show<TwoButtonPanel>(message)
            .WithButtonText(confirmButtonText, cancelButtonText)
            .OnButtons(onConfirm, onCancel);
    }

    /// <summary>
    /// 싱글플레이 게임이 끝날 때 팝업 열기
    /// </summary>
    /// <param name="result">게임 결과</param>
    public void OpenEndGamePanelInSinglePlay(GameResult result)
    {
        if (gameResultPopup != null)
        {
            gameResultPopup.Show(result);
        }
    }

    /// <summary>
    /// 멀티플레이 게임이 끝날 때 팝업 열기
    /// </summary>
    /// <param name="response">게임 결과에 따른 서버 반환값</param>
    /// <param name="result">게임 결과</param>
    public void OpenEndGamePanelInMultiplay(GameResultResponse response, GameResult result)
    {
        if (gameResultPopup != null)
        {
            if (response.rank.gradeChanged)
            {
                string message = $"{response.rank.grade} 등급으로 ";
                message += result == GameResult.Victory ? "승급했습니다." : "강등됐습니다.";
                oneConfirmButtonPopup.Show<OneButtonPanel>(message).OnConfirm(null);
            }

            gameResultPopup.Show(result, response);
        }
    }

    #endregion

    #region 팝업 닫기

    /// <summary>
    /// 모든 버튼 팝업을 강제로 닫기
    /// </summary>
    public void CloseButtonPopup()
    {
        oneCancelButtonPopupPanel.SetActive(false);
        oneConfirmButtonPopupPanel.SetActive(false);
        twoButtonPopupPanel.SetActive(false);
    }

    /// <summary>
    /// 게임 결과 팝업창을 비활성화함
    /// </summary>
    public void GameResultPopupOff()
    {
        gameResultPopupPanel.SetActive(false);
    }

    #endregion

    #region 팝업 설정

    /// <summary>
    /// 게임 결과창의 재대국 버튼을 비활성화함
    /// </summary>
    public void GameResultPopupDisableRematchButton()
    {
        gameResultPopup.DisableRematchButton();
    }

    #endregion

    #region 에러 시

    /// <summary>
    /// 에러 발생 시 팝업을 띄워주는 메소드
    /// </summary>
    /// <param name="errorMessage">에러 메세지</param>
    /// <param name="goToMainScene">확인 버튼 클릭 시 메인 씬으로 돌아갈지 여부</param>
    public void OnError(string errorMessage, bool goToMainScene)
    {
        if (goToMainScene)
            oneConfirmButtonPopup.Show<OneButtonPanel>($"오류가 발생했습니다.\n {errorMessage}").OnConfirm(() =>
                SceneController.LoadScene(SceneType.Main, 0.5f)
            );
        else
            oneConfirmButtonPopup.Show<OneButtonPanel>($"오류가 발생했습니다.\n {errorMessage}");
    }

    #endregion
}