using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSceneUIManager : Singleton<GameSceneUIManager>
{
    [Header("UI Components")]
    [SerializeField] private GameObject defaultPlayerTurnImage; // 플레이어 차례 표시 기본 GameObject

    [SerializeField] private GameObject blackStonePlayerTurnImage; // 흑돌 플레이어 차례 표시 GameObject
    [SerializeField] private GameObject whiteStonePlayerTurnImage; // 백돌 플레이어 차례 표시 GameObject
    [Space(7)] [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;
    [SerializeField] private GameObject currentPlayerTurnCircleLeft; // 현재 턴 플레이어 표시용 GameObject / Left
    [SerializeField] private GameObject currentPlayerTurnCircleRight; // 현재 턴 플레이어 표시용 GameObject / Right
    [Space(7)] [SerializeField] private Button confirmMoveButton; // 착수 확정 버튼
    [SerializeField] private Button surrenderButton; // 항복 버튼

    [Header("플레이어 프로필 이미지")]
    [SerializeField] private Image leftProfileImage;

    [SerializeField] private Image rightProfileImage;

    [Header("프로필용 스프라이트")]
    [SerializeField] private Sprite pandaSprite;

    [SerializeField] private Sprite redPandaSprite;

    [SerializeField] private Sprite winPandaProfileSprite;
    [SerializeField] private Sprite losePandaProfileSprite;

    [SerializeField] private Sprite winRedPandaProfileSprite;
    [SerializeField] private Sprite loseRedPandaProfileSprite;

    [Header("팝업")]
    [SerializeField] [Tooltip("게임 씬에서 사용할 확인 버튼 1개짜리 팝업")]
    private GameObject oneConfirmButtonPopupPanel;

    [SerializeField] [Tooltip("메인 씬에서 사용할 취소 버튼 1개짜리 팝업")]
    private GameObject oneCancelButtonPopupPanel;

    [SerializeField] [Tooltip("게임 씬에서 사용할 버튼 2개짜리 팝업")]
    private GameObject twoButtonPopupPanel;

    [SerializeField] [Tooltip("게임 결과창 팝업")]
    private GameObject gameResultPopupPanel;

    private GamePlayManager gamePlayManager;
    private MultiplayManager multiplayManager;

    private OneButtonPanel oneConfirmButtonPopup;
    private OneButtonPanel oneCancelButtonPopup;
    private TwoButtonPanel twoButtonPopup;
    private GameResultPanel gameResultPopup;

    protected override void Awake()
    {
        base.Awake();

        if (oneConfirmButtonPopupPanel != null)
            oneConfirmButtonPopup = oneConfirmButtonPopupPanel.GetComponent<OneButtonPanel>();
        if (oneCancelButtonPopupPanel != null)
            oneCancelButtonPopup = oneCancelButtonPopupPanel.GetComponent<OneButtonPanel>();
        if (twoButtonPopupPanel != null)
            twoButtonPopup = twoButtonPopupPanel.GetComponent<TwoButtonPanel>();
        if (gameResultPopupPanel != null)
            gameResultPopup = gameResultPopupPanel.GetComponent<GameResultPanel>();

        pandaSprite = Resources.Load<Sprite>($"UI/character/Property 1=panda");
        redPandaSprite = Resources.Load<Sprite>($"UI/character/Property 1=redPanda");
    }

    private void Start()
    {
        gamePlayManager = GamePlayManager.Instance;
        multiplayManager = MultiplayManager.Instance;

        if (gamePlayManager?.gameLogicController != null)
        {
            gamePlayManager.OnGameEnd += OpenEndGamePanelInSinglePlay;
            gamePlayManager.OnGameEnd += UpdateProfileImagesOnResult;
            gamePlayManager.gameLogicController.OnPlayerStonesRandomized += InitPlayerTurnDisplay;
            gamePlayManager.gameLogicController.OnPlayerTurnChanged += UpdatePlayerTurnDisplay;
        }

        if (gamePlayManager?.boardManager != null)
        {
            gamePlayManager.boardManager.ViolateRenjuRule += OnError;
        }

        if (multiplayManager != null && GameModeManager.Mode == GameMode.MultiPlayer)
        {
            multiplayManager.MatchWaitngCallback += OpenMatchWaitingPanel;
            multiplayManager.MatchFoundCallback += OnMatchFound;
            multiplayManager.MatchCanceledCallback += OnMatchCanceled;
            multiplayManager.MatchResultCallback += OpenEndGamePanelInMultiplay;
            multiplayManager.RematchCallback += HandleRematchUI;
            multiplayManager.ErrorCallback += OnError;
        }
    }

    private void OnDisable()
    {
        if (gamePlayManager?.gameLogicController != null)
        {
            gamePlayManager.OnGameEnd -= OpenEndGamePanelInSinglePlay;
            gamePlayManager.OnGameEnd -= UpdateProfileImagesOnResult;
            multiplayManager.MatchCanceledCallback -= OnMatchCanceled;
            gamePlayManager.gameLogicController.OnPlayerStonesRandomized -= InitPlayerTurnDisplay;
            gamePlayManager.gameLogicController.OnPlayerTurnChanged -= UpdatePlayerTurnDisplay;
        }
        if (gamePlayManager?.boardManager != null)
        {
            gamePlayManager.boardManager.ViolateRenjuRule -= OnError;
        }

        if (multiplayManager != null && GameModeManager.Mode == GameMode.MultiPlayer)
        {
            multiplayManager.MatchWaitngCallback -= OpenMatchWaitingPanel;
            multiplayManager.MatchFoundCallback -= OnMatchFound;
            multiplayManager.MatchResultCallback -= OpenEndGamePanelInMultiplay;
            multiplayManager.RematchCallback -= HandleRematchUI;
            multiplayManager.ErrorCallback -= OnError;
        }
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
    }

    #region 공용

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
    /// 플레이어 차레 표시 초기화
    /// </summary>
    private void InitPlayerTurnDisplay()
    {
        if (gamePlayManager.currentGameState != GameState.Playing) return;
        if (defaultPlayerTurnImage.activeSelf) defaultPlayerTurnImage.SetActive(false);

        blackStonePlayerTurnImage?.SetActive(true);
        whiteStonePlayerTurnImage?.SetActive(false);

        UpdateCurrentTurnCircle();
    }

    /// <summary>
    /// 플레이어 차례 표시 업데이트
    /// </summary>
    private void UpdatePlayerTurnDisplay(StoneType stoneType)
    {
        if (gamePlayManager.currentGameState != GameState.Playing) return;

        // 흑돌/백돌 차례 표시 이미지 업데이트
        blackStonePlayerTurnImage?.SetActive(stoneType == StoneType.Black);
        whiteStonePlayerTurnImage?.SetActive(stoneType == StoneType.White);

        UpdateCurrentTurnCircle();
    }

    /// <summary>
    /// 현재 턴 사용자 표시 업데이트
    /// </summary>
    private void UpdateCurrentTurnCircle()
    {
        var currentTurn = gamePlayManager.gameLogicController.GetCurrentTurnPlayer();
        if (GameModeManager.Mode == GameMode.SinglePlayer)
        {
            currentPlayerTurnCircleLeft.SetActive(currentTurn == PlayerType.Player1);
            currentPlayerTurnCircleRight.SetActive(currentTurn == PlayerType.Player2);
        }
        else if (GameModeManager.Mode == GameMode.MultiPlayer)
        {
            currentPlayerTurnCircleLeft.SetActive(currentTurn == PlayerType.Me);
            currentPlayerTurnCircleRight.SetActive(currentTurn == PlayerType.Opponent);
        }
    }

    private void OnError(string errorMessage, bool goToMainScene)
    {
        if (goToMainScene)
            oneConfirmButtonPopup.OpenWithSetMessageAndButtonEvent($"오류가 발생했습니다.\n {errorMessage}",
                () => SceneController.LoadScene(SceneType.Main, 0.5f));
        else
            oneConfirmButtonPopup.OpenWithSetMessageAndButtonEvent($"오류가 발생했습니다.\n {errorMessage}");
    }

    #endregion

    #region 싱글플레이

    /// <summary>
    /// 싱글플레이 게임이 끝날때 호출되는 메소드
    /// </summary>
    /// <param name="result">게임 결과</param>
    private void OpenEndGamePanelInSinglePlay(GameResult result)
    {
        if (GameModeManager.Mode == GameMode.MultiPlayer) return;

        if (gameResultPopup != null)
        {
            gameResultPopup.OpenWithButtonEvent(result, () => { SceneController.LoadScene(SceneType.Main, 0.5f); },
                () => { gamePlayManager.ResterGame(); });
        }
    }

    #endregion

    #region 멀티플레이

    /// <summary>
    /// 매칭 중임을 알리는 팝업을 띄움
    /// </summary>
    private void OpenMatchWaitingPanel()
    {
        oneCancelButtonPopup.OpenWithSetMessageAndButtonEvent("매칭 찾는 중...", 0.5f,
            () => gamePlayManager.multiplayManager.multiplayController?.CancelMatch());
    }

    /// <summary>
    /// 매칭이 성공했을 때 실행할 메소드
    /// </summary>
    private void OnMatchFound()
    {
        CloseButtonPopup();
        UpdatePlayerProfileInMultiPlay();
    }

    /// <summary>
    /// 매칭을 취소했을 때 실행할 메소드
    /// </summary>
    private void OnMatchCanceled()
    {
        oneConfirmButtonPopup.OpenWithSetMessageAndButtonEvent("매칭이 취소되었습니다.",
            () => SceneController.LoadScene(SceneType.Main, 0.5f));
    }

    /// <summary>
    /// 멀티플레이 시 사용자 프로필을 가져옴
    /// </summary>
    private void UpdatePlayerProfileInMultiPlay()
    {
        if (GameModeManager.Mode != GameMode.MultiPlayer) return;

        Image player1ProfileImage = player1.transform.GetChild(1).GetComponent<Image>();
        TextMeshProUGUI player1GradeAndNickname = player1.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        Image player2ProfileImage = player2.transform.GetChild(1).GetComponent<Image>();
        TextMeshProUGUI player2GradeAndNickname = player2.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        // 나의 프로필은 기기에 있는 정보 사용
        GameManager gameManager = GameManager.Instance;
        player1ProfileImage.sprite = gameManager.profileImage == 1 ? pandaSprite : redPandaSprite;
        player1GradeAndNickname.text = $"{gameManager.grade}급 {gameManager.nickname}";
        // 상대의 프로필은 서버에서 받아온 정보를 사용하여 프로필 업데이트
        MatchData opponentData = MultiplayManager.Instance.MatchData;
        player2ProfileImage.sprite = opponentData.profileImage == 1 ? pandaSprite : redPandaSprite;
        player2GradeAndNickname.text = $"{opponentData.grade}급 {opponentData.nickname}";
    }

    /// <summary>
    /// 멀티플레이 게임이 끝날때 호출되는 메소드
    /// </summary>
    /// <param name="response">게임 결과에 따른 서버 반환값</param>
    /// <param name="result">게임 결과</param>
    private void OpenEndGamePanelInMultiplay(GameResultResponse response, GameResult result)
    {
        if (gameResultPopup != null)
        {
            if (response.rank.gradeChanged)
            {
                string message = $"{response.rank.grade} 등급으로 ";
                message += result == GameResult.Victory ? "승급했습니다." : "강등됐습니다.";
                oneConfirmButtonPopup.OpenWithSetMessageAndButtonEvent(message);
            }

            gameResultPopup.OpenWithButtonEvent(response, result,
                () =>
                {
                    gamePlayManager.multiplayManager.multiplayController?.LeaveRoom();
                    SceneController.LoadScene(SceneType.Main, 0.5f);
                },
                () => { gamePlayManager.multiplayManager.multiplayController?.RequestRematch(); });
        }
    }

    // 오목 결과에 따라 프로필 사진이 변경됨
    private void UpdateProfileImagesOnResult(GameResult result)
    {
        if (leftProfileImage == null || rightProfileImage == null)
        {
            Debug.LogWarning("Profile images not assigned!");
            return;
        }

        switch (result)
        {
            case GameResult.Player1Win:
                leftProfileImage.sprite = winPandaProfileSprite;
                rightProfileImage.sprite = loseRedPandaProfileSprite;
                break;

            case GameResult.Player2Win:
                leftProfileImage.sprite = losePandaProfileSprite;
                rightProfileImage.sprite = winRedPandaProfileSprite;
                break;

            case GameResult.Victory: // 내가 승리 (멀티)
                leftProfileImage.sprite = winPandaProfileSprite;
                rightProfileImage.sprite = loseRedPandaProfileSprite;
                break;

            case GameResult.Defeat: // 내가 패배 (멀티)
                leftProfileImage.sprite = losePandaProfileSprite;
                rightProfileImage.sprite = winRedPandaProfileSprite;
                break;

            case GameResult.Draw:
                leftProfileImage.sprite = winPandaProfileSprite;
                rightProfileImage.sprite = winRedPandaProfileSprite;
                break;

            default:
                Debug.Log("No profile update for result: " + result);
                break;
        }
    }

    private void HandleRematchUI(MultiplayControllerState state)
    {
        switch (state)
        {
            case MultiplayControllerState.RematchRequested:
                twoButtonPopup.SetButtonText("거절", "수락");
                twoButtonPopup.OpenWithSetMessageAndButtonEvent("상대가 재대국 신청을 하였습니다.\n받으시겠습니까?",
                    () => { gamePlayManager.multiplayManager.multiplayController?.AcceptRematch(); },
                    () => { gamePlayManager.multiplayManager.multiplayController?.RejectRematch(); });
                break;
            case MultiplayControllerState.RematchRequestSent:
                oneCancelButtonPopup.OpenWithSetMessageAndButtonEvent("재대국 신청 중입니다...", 2f,
                    () => { gamePlayManager.multiplayManager.multiplayController?.CancelRematch(); });
                break;
            case MultiplayControllerState.RematchRejected:
                CloseButtonPopup();
                oneConfirmButtonPopup.OpenWithSetMessageAndButtonEvent("상대방이 재대국을 거절했습니다.",
                    () => { gameResultPopup.DisableRematchButton(); });
                break;
            case MultiplayControllerState.RematchCanceled:
                CloseButtonPopup();
                oneConfirmButtonPopup.OpenWithSetMessageAndButtonEvent("상대방이 재대국을 취소했습니다.",
                    () => { });
                break;
            case MultiplayControllerState.RematchStarted:
                CloseButtonPopup();
                gameResultPopupPanel.SetActive(false);
                gamePlayManager.ResterGame();
                break;
            case MultiplayControllerState.ExitRoom:
                break;
            case MultiplayControllerState.OpponentLeft:
                CloseButtonPopup();
                oneConfirmButtonPopup.OpenWithSetMessageAndButtonEvent("상대방이 나갔습니다.",
                    () => { SceneController.LoadScene(SceneType.Main, 0.5f); });
                break;
        }
    }

    #endregion

    /// <summary>
    /// 항복 버튼을 눌렀을 때 호출되는 메소드
    /// </summary>
    public void OnClickSurrenderButton()
    {
        // TODO: 타이머 멈추기
        twoButtonPopup.SetButtonText("취소", "기권");
        twoButtonPopup.OpenWithSetMessageAndButtonEvent("기권 하시겠습니까?", () => { gamePlayManager.Surrender(); }, () =>
        {
            // TODO: 타이머 다시 시작
        });
    }
}