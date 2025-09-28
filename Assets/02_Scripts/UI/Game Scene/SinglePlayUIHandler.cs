using UnityEngine;

public class SinglePlayUIHandler : MonoBehaviour
{
    private GamePlayManager gamePlayManager;
    private PlayerProfileUIController playerProfileUIController;
    private GamePopupController gamePopupController;
    private TurnIndicatorUIController turnIndicatorUIController;

    #region Unity Life Cycle

    private void Awake()
    {
        gamePlayManager = GamePlayManager.Instance;
        playerProfileUIController = GameSceneUIManager.Instance.playerProfileUIController;
        gamePopupController = GameSceneUIManager.Instance.gamePopupController;
        turnIndicatorUIController = GameSceneUIManager.Instance.turnIndicatorUIController;
    }

    private void OnEnable()
    {
        gamePopupController.gameResultPopup.onRematchClicked.AddListener(HandleRematchRequest);
        gamePopupController.gameResultPopup.onExitClicked.AddListener(() =>
        {
            gamePopupController.GameResultPopupOff();
            SceneController.LoadScene(SceneType.Main, 0.25f);
        });

        if (gamePlayManager != null)
        {
            gamePlayManager.OnGameEnd += OpenEndGamePanelInSinglePlay;
            gamePlayManager.OnGameEnd += playerProfileUIController.UpdateProfileImagesOnResultInSinglePlay;
            gamePlayManager.OnGameRestart += playerProfileUIController.UpdatePlayerProfileInSinglePlay;

            if (gamePlayManager.GameLogicController != null)
            {
                gamePlayManager.GameLogicController.OnPlayerTurnChanged += UpdatePlayerTurnDisplay;
            }
        }
    }


    private void OnDisable()
    {
        gamePopupController.gameResultPopup.onRematchClicked.RemoveAllListeners();
        gamePopupController.gameResultPopup.onExitClicked.RemoveAllListeners();

        if (gamePlayManager != null)
        {
            gamePlayManager.OnGameEnd -= OpenEndGamePanelInSinglePlay;
            gamePlayManager.OnGameEnd -= playerProfileUIController.UpdateProfileImagesOnResultInSinglePlay;
            gamePlayManager.OnGameRestart -= playerProfileUIController.UpdatePlayerProfileInSinglePlay;
            if (gamePlayManager.GameLogicController != null)
            {
                gamePlayManager.GameLogicController.OnPlayerTurnChanged -= UpdatePlayerTurnDisplay;
            }
        }
    }

    #endregion

    #region 이벤트 콜백
    /// <summary>
    /// 재대국 요청 로직
    /// </summary>
    private void HandleRematchRequest()
    {
        if (GameModeManager.Mode != GameMode.SinglePlayer) return;

        gamePlayManager.RestartGame();
        gamePopupController.GameResultPopupOff();
    }

    /// <summary>
    /// 게임 결과창 띄우기
    /// </summary>
    private void OpenEndGamePanelInSinglePlay(GameResult result)
    {
        gamePopupController.OpenEndGamePanelInSinglePlay(result);
    }

    /// <summary>
    /// 턴 표시하기
    /// </summary>
    private void UpdatePlayerTurnDisplay(StoneType stoneType, PlayerType playerType)
    {
        turnIndicatorUIController.UpdatePlayerTurnDisplay(stoneType, playerType);
    }
    #endregion
}