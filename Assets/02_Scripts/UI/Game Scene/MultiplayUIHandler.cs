using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class MultiplayUIHandler : MonoBehaviour
{
    private GamePlayManager gamePlayManager;
    private MultiplayManager multiplayManager;
    private GameSceneUIManager gameSceneUIManager;
    private PlayerProfileUIController playerProfileUIController;
    private GamePopupController gamePopupController;
    private TurnIndicatorUIController turnIndicatorUIController;

    #region Unity Life Cycle

    private void Awake()
    {
        gamePlayManager = GamePlayManager.Instance;
        multiplayManager = MultiplayManager.Instance;
        gameSceneUIManager = GameSceneUIManager.Instance;
        playerProfileUIController = GameSceneUIManager.Instance.playerProfileUIController;
        gamePopupController = GameSceneUIManager.Instance.gamePopupController;
        turnIndicatorUIController = GameSceneUIManager.Instance.turnIndicatorUIController;
    }

    private void OnEnable()
    {
        gamePopupController.gameResultPopup.onRematchClicked.AddListener(HandleRematchRequest);
        gamePopupController.gameResultPopup.onExitClicked.AddListener(() =>
        {
            StartCoroutine(SafeExitGame());
            gamePopupController.GameResultPopupOff();
        });

        if (gamePlayManager != null)
        {
            gamePlayManager.OnGameEnd += playerProfileUIController.UpdateProfileImagesOnResultInMultiplay;
            gamePlayManager.OnGameRestart += playerProfileUIController.UpdatePlayerProfileInMultiPlay;
            if (gamePlayManager.GameLogicController != null)
            {
                gamePlayManager.GameLogicController.OnPlayerTurnChanged += UpdatePlayerTurnDisplay;
            }
        }

        if (multiplayManager != null)
        {
            multiplayManager.MatchCallback += HandleMatchUI;
            multiplayManager.MatchResultCallback += OpenEndGamePanelInMultiplay;
            multiplayManager.RematchCallback += HandleRematchUI;
            multiplayManager.ErrorCallback += OnError;
            Debug.Log("[MultiplayUIHandler] Start()에서 멀티플레이 이벤트 구독");
        }
    }

    private void OnDisable()
    {
        gamePopupController.gameResultPopup.onRematchClicked.RemoveAllListeners();
        gamePopupController.gameResultPopup.onExitClicked.RemoveAllListeners();

        if (gamePlayManager != null)
        {
            gamePlayManager.OnGameEnd -= playerProfileUIController.UpdateProfileImagesOnResultInMultiplay;
            gamePlayManager.OnGameRestart -= playerProfileUIController.UpdatePlayerProfileInMultiPlay;
            if (gamePlayManager.GameLogicController != null)
            {
                gamePlayManager.GameLogicController.OnPlayerTurnChanged -= UpdatePlayerTurnDisplay;
            }
        }

        if (multiplayManager != null)
        {
            multiplayManager.MatchCallback -= HandleMatchUI;
            multiplayManager.MatchResultCallback -= OpenEndGamePanelInMultiplay;
            multiplayManager.RematchCallback -= HandleRematchUI;
            multiplayManager.ErrorCallback -= OnError;
            Debug.Log("[MultiplayUIHandler] Start()에서 멀티플레이 이벤트 구독 해제");
        }
    }

    #endregion

    #region 매칭

    /// <summary>
    /// 매칭 UI 핸들러
    /// </summary>
    private void HandleMatchUI(MultiplayControllerState state)
    {
        switch (state)
        {
            case MultiplayControllerState.MatchWaiting:
                gameSceneUIManager.SurrenderButtonEnable(false);
                gamePopupController.OpenOneCancelButtonPopup("매칭 찾는 중...",
                    () => multiplayManager.multiplayController?.CancelMatch(), 2f);
                break;
            case MultiplayControllerState.MatchExpanded:
                Debug.Log("<color=cyan>매칭 범위 확장</color>");
                break;
            case MultiplayControllerState.MatchFailed:
                gamePopupController.CloseButtonPopup();
                gameSceneUIManager.SurrenderButtonEnable(true);
                playerProfileUIController.UpdatePlayerProfileInMultiPlay();
                Debug.Log("<color=magenta>매칭 실패 AI 대전으로 전환합니다.</color>");
                break;
            case MultiplayControllerState.MatchFound:
                gamePopupController.CloseButtonPopup();
                gameSceneUIManager.SurrenderButtonEnable(true);
                playerProfileUIController.UpdatePlayerProfileInMultiPlay();
                break;
            case MultiplayControllerState.MatchCanceled:
                gamePopupController.OpenOneConfirmButtonPopup("매칭이 취소되었습니다.",
                    () => SceneController.LoadScene(SceneType.Main, 0.5f));
                break;
        }
    }

    #endregion

    #region 재대국

    /// <summary>
    /// 재대국 요청 로직
    /// </summary>
    private void HandleRematchRequest()
    {
        if (GameModeManager.Mode == GameMode.MultiPlayer)
            multiplayManager.multiplayController?.RequestRematch();
        else if (GameModeManager.Mode == GameMode.AI)
        {
            float waitTime = Random.Range(1f, 3f);
            StartCoroutine(FakeRematch(waitTime));
        }
    }

    /// <summary>
    /// 재대국 UI를 조절하는 메소드
    /// </summary>
    /// <param name="state">서버에서 받은 상태</param>
    private void HandleRematchUI(MultiplayControllerState state)
    {
        switch (state)
        {
            case MultiplayControllerState.RematchRequested:
                gamePopupController.OpenTwoButtonPopup("상대가 재대국 신청을 하였습니다.\n받으시겠습니까?",
                    "수락", "거절",
                    () => multiplayManager.multiplayController?.AcceptRematch(),
                    () => multiplayManager.multiplayController?.RejectRematch()
                );
                break;
            case MultiplayControllerState.RematchRequestSent:
                gamePopupController.OpenOneCancelButtonPopup("재대국 신청 중입니다...",
                    () => multiplayManager.multiplayController?.CancelRematch(), 2f);
                break;
            case MultiplayControllerState.RematchRejected:
                gamePopupController.CloseButtonPopup();
                gamePopupController.OpenOneConfirmButtonPopup("상대방이 재대국을 거절했습니다.",
                    () => gamePopupController.GameResultPopupDisableRematchButton());
                break;
            case MultiplayControllerState.RematchCanceled:
                gamePopupController.CloseButtonPopup();
                gamePopupController.OpenOneConfirmButtonPopup("상대방이 재대국을 취소했습니다.", null);
                break;
            case MultiplayControllerState.RematchStarted:
                gamePopupController.CloseButtonPopup();
                gamePopupController.GameResultPopupOff();
                gamePlayManager.RestartGame();
                break;
            case MultiplayControllerState.ExitRoom:
                break;
            case MultiplayControllerState.OpponentLeft:
                gamePopupController.CloseButtonPopup();
                gamePopupController.OpenOneConfirmButtonPopup("상대방이 나갔습니다.",
                    () => StartCoroutine(SafeExitGame()));

                break;
        }
    }

    #endregion

    #region 이벤트 콜백

    /// <summary>
    /// 턴 표시하기
    /// </summary>
    private void UpdatePlayerTurnDisplay(StoneType stoneType, PlayerType playerType)
    {
        turnIndicatorUIController.UpdatePlayerTurnDisplay(stoneType, playerType);
    }


    /// <summary>
    /// 게임 결과창 띄우기
    /// </summary>
    private void OpenEndGamePanelInMultiplay(GameResultResponse response, GameResult result)
    {
        gamePopupController.OpenEndGamePanelInMultiplay(response, result);
    }

    /// <summary>
    /// 에러시 호출될 콜백
    /// </summary>
    /// <param name="errorMessage">에러 메세지</param>
    /// <param name="goToMainScene">메인 씬으로 돌아갈지 여부</param>
    private void OnError(string errorMessage, bool goToMainScene)
    {
        gamePopupController.OnError(errorMessage, goToMainScene);
    }

    #endregion


    #region AI

    /// <summary>
    /// 사람이 재대국을 받는 것 처럼 보이게 하기위한 코루틴
    /// </summary>
    private IEnumerator FakeRematch(float waitTime)
    {
        gamePopupController.OpenOneCancelButtonPopup("재대국 신청 중입니다...",
            () => StartCoroutine(SafeExitGame()), 2f);
        yield return new WaitForSeconds(waitTime);
        gamePopupController.CloseButtonPopup();
        gamePopupController.GameResultPopupOff();
        gamePlayManager.RestartGame();
    }

    #endregion

    #region 멀티플레이 방 나가기

    bool isRoomLeft = false;

    private void OnRoomLeftHandler()
    {
        isRoomLeft = true;
    }

    /// <summary>
    /// 안전하게 멀티플레이 방을 나가기 위한 코루틴
    /// </summary>
    private IEnumerator SafeExitGame()
    {
        if (multiplayManager?.multiplayController != null &&
            GameModeManager.Mode == GameMode.MultiPlayer)
        {
            MultiplayManager.Instance.OnRoomLeft += OnRoomLeftHandler;

            multiplayManager.multiplayController.LeaveRoom();

            float timeout = 3f;
            while (!isRoomLeft && timeout > 0)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            MultiplayManager.Instance.OnRoomLeft -= OnRoomLeftHandler;
        }

        SceneController.LoadScene(SceneType.Main, 0.5f);
    }

    #endregion
}