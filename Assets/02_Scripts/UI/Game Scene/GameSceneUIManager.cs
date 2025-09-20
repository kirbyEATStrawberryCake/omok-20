using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSceneUIManager : Singleton<GameSceneUIManager>
{
    [Header("UI Components")]
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

    private MatchData opponentData;

    private GiboManager giboManager;

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
    }

    private void Start()
    {
        gamePlayManager = GamePlayManager.Instance;
        multiplayManager = MultiplayManager.Instance;
        giboManager = GiboManager.Instance;
        giboManager.StartNewRecord(); // 새 기보 데이터 생성

        if (gamePlayManager != null)
        {
            gamePlayManager.OnGameEnd += OpenEndGamePanelInSinglePlay;
            gamePlayManager.OnGameEnd += UpdateProfileImagesOnResult;
            gamePlayManager.OnGameEnd += SaveGibo;
            gamePlayManager.OnGameRestart += ResetProfileImage;

            if (gamePlayManager?.GameLogicController != null)
            {
                gamePlayManager.GameLogicController.OnPlayerTurnChanged += UpdatePlayerTurnDisplay;
            }

            if (gamePlayManager?.BoardManager != null)
            {
                gamePlayManager.BoardManager.ViolateRenjuRule += OnError;
            }
        }


        if (multiplayManager != null &&
            (GameModeManager.Mode == GameMode.MultiPlayer || GameModeManager.Mode == GameMode.AI))
        {
            multiplayManager.MatchCallback += HandleMatchUI;
            multiplayManager.MatchResultCallback += OpenEndGamePanelInMultiplay;
            multiplayManager.RematchCallback += HandleRematchUI;
            multiplayManager.ErrorCallback += OnError;
        }
    }

    private void OnDisable()
    {
        if (gamePlayManager != null)
        {
            gamePlayManager.OnGameEnd -= OpenEndGamePanelInSinglePlay;
            gamePlayManager.OnGameEnd -= UpdateProfileImagesOnResult;
            gamePlayManager.OnGameEnd -= SaveGibo;
            gamePlayManager.OnGameRestart -= ResetProfileImage;

            if (gamePlayManager?.GameLogicController != null)
            {
                gamePlayManager.GameLogicController.OnPlayerTurnChanged -= UpdatePlayerTurnDisplay;
            }

            if (gamePlayManager?.BoardManager != null)
            {
                gamePlayManager.BoardManager.ViolateRenjuRule -= OnError;
            }
        }

        if (multiplayManager != null &&
            (GameModeManager.Mode == GameMode.MultiPlayer || GameModeManager.Mode == GameMode.AI))
        {
            multiplayManager.MatchCallback -= HandleMatchUI;
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
        var currentTurn = gamePlayManager.GameLogicController.GetCurrentTurnPlayer();
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

    /// <summary>
    /// 오목 결과에 따라서 프로필 사진을 변경하는 메소드
    /// </summary>
    /// <param name="result"></param>
    private void UpdateProfileImagesOnResult(GameResult result)
    {
        if (leftProfileImage == null || rightProfileImage == null)
        {
            Debug.LogWarning("Profile images not assigned!");
            return;
        }

        GameManager gameManager = GameManager.Instance;

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
                leftProfileImage.sprite = gameManager.profileImage == 1
                    ? winPandaProfileSprite
                    : winRedPandaProfileSprite;
                if (GameModeManager.Mode == GameMode.MultiPlayer)
                    rightProfileImage.sprite = opponentData.profileImage == 1
                        ? losePandaProfileSprite
                        : loseRedPandaProfileSprite;
                else if (GameModeManager.Mode == GameMode.AI)
                    rightProfileImage.sprite = gameManager.profileImage == 1
                        ? loseRedPandaProfileSprite
                        : losePandaProfileSprite;
                break;

            case GameResult.Defeat: // 내가 패배 (멀티)
                leftProfileImage.sprite = gameManager.profileImage == 1
                    ? losePandaProfileSprite
                    : loseRedPandaProfileSprite;
                if (GameModeManager.Mode == GameMode.MultiPlayer)
                    rightProfileImage.sprite = opponentData.profileImage == 1
                        ? winPandaProfileSprite
                        : winRedPandaProfileSprite;
                else if (GameModeManager.Mode == GameMode.AI)
                    rightProfileImage.sprite = gameManager.profileImage == 1
                        ? winRedPandaProfileSprite
                        : winPandaProfileSprite;

                ;
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

    /// <summary>
    /// 프로필 이미지를 초기화해주는 메소드
    /// </summary>
    private void ResetProfileImage()
    {
        if (GameModeManager.Mode == GameMode.SinglePlayer)
        {
            leftProfileImage.sprite = pandaSprite;
            rightProfileImage.sprite = redPandaSprite;
        }
        else if (GameModeManager.Mode == GameMode.MultiPlayer || GameModeManager.Mode == GameMode.AI)
        {
            UpdatePlayerProfileInMultiPlay();
        }
    }

    /// <summary>
    /// 에러 발생 시 팝업을 띄워주는 메소드
    /// </summary>
    /// <param name="errorMessage">에러 메세지</param>
    /// <param name="goToMainScene">확인 버튼 클릭 시 메인 씬으로 돌아갈지 여부</param>
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
        if (GameModeManager.Mode == GameMode.MultiPlayer || GameModeManager.Mode == GameMode.AI) return;

        if (gameResultPopup != null)
        {
            gameResultPopup.OpenWithButtonEvent(result, () => { SceneController.LoadScene(SceneType.Main, 0.5f); },
                () => { gamePlayManager.ResterGame(); });
        }
    }

    #endregion

    #region 멀티플레이

    private void HandleMatchUI(MultiplayControllerState state)
    {
        switch (state)
        {
            case MultiplayControllerState.MatchWaiting:
                surrenderButton.enabled = false;
                oneCancelButtonPopup.OpenWithSetMessageAndButtonEvent("매칭 찾는 중...", 2f,
                    () => { multiplayManager.multiplayController?.CancelMatch(); });
                break;
            case MultiplayControllerState.MatchExpanded:
                Debug.Log("<color=cyan>매칭 범위 확장</color>");
                break;
            case MultiplayControllerState.MatchFailed:
                CloseButtonPopup();
                surrenderButton.enabled = true;
                UpdatePlayerProfileInMultiPlay();
                Debug.Log("<color=magenta>매칭 실패 AI 대전으로 전환합니다.</color>");
                break;
            case MultiplayControllerState.MatchFound:
                CloseButtonPopup();
                surrenderButton.enabled = true;
                UpdatePlayerProfileInMultiPlay();
                break;
            case MultiplayControllerState.MatchCanceled:
                oneConfirmButtonPopup.OpenWithSetMessageAndButtonEvent("매칭이 취소되었습니다.",
                    () => StartCoroutine(SafeExitGame()));
                break;
        }
    }

    /// <summary>
    /// 멀티플레이 시 사용자 프로필을 가져옴
    /// </summary>
    private void UpdatePlayerProfileInMultiPlay()
    {
        if (GameModeManager.Mode != GameMode.MultiPlayer && GameModeManager.Mode != GameMode.AI) return;

        TextMeshProUGUI player1GradeAndNickname = player1.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI player2GradeAndNickname = player2.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        // 나의 프로필은 기기에 있는 정보 사용
        GameManager gameManager = GameManager.Instance;
        leftProfileImage.sprite = gameManager.profileImage == 1 ? pandaSprite : redPandaSprite;
        player1GradeAndNickname.text = $"{gameManager.grade}급 {gameManager.nickname}";
        // 상대의 프로필은 서버에서 받아온 정보를 사용하여 프로필 업데이트
        if (GameModeManager.Mode == GameMode.MultiPlayer)
        {
            opponentData = MultiplayManager.Instance.MatchData;
            rightProfileImage.sprite = opponentData.profileImage == 1 ? pandaSprite : redPandaSprite;
            player2GradeAndNickname.text = $"{opponentData.grade}급 {opponentData.nickname}";
            giboManager.SetGiboProfileData(opponentData.nickname, opponentData.profileImage, opponentData.grade);
        }
        else if (GameModeManager.Mode == GameMode.AI)
        {
            rightProfileImage.sprite = gameManager.profileImage == 1 ? redPandaSprite : pandaSprite;
            // TODO: AI 랜덤이름? -> 일단 값은 고정적으로 세팅했습니다!
            giboManager.SetGiboProfileData("AI1", 2, 10);
        }
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
                () => { StartCoroutine(SafeExitGame()); },
                () =>
                {
                    if (GameModeManager.Mode == GameMode.MultiPlayer)
                        multiplayManager.multiplayController?.RequestRematch();
                    else if (GameModeManager.Mode == GameMode.AI)
                    {
                        float waitTime = Random.Range(1f, 3f);
                        StartCoroutine(FakeRematch(waitTime));
                    }
                });
        }
    }

    private IEnumerator SafeExitGame()
    {
        if (multiplayManager?.multiplayController != null &&
            GameModeManager.Mode == GameMode.MultiPlayer)
        {
            multiplayManager.multiplayController.LeaveRoom();
            yield return new WaitForSeconds(0.3f);
        }

        SceneController.LoadScene(SceneType.Main, 0.5f);


        // SceneController.LoadScene(SceneType.Main, 0.5f);
        // // 방 나가기
        // if (multiplayManager?.multiplayController != null)
        // {
        //     multiplayManager.multiplayController.LeaveRoom();
        //     yield return new WaitForSeconds(0.5f); // 서버 처리 대기
        // }
        //
        // // 소켓 연결 해제
        // if (multiplayManager?.multiplayController != null)
        // {
        //     multiplayManager.multiplayController.Dispose();
        //     yield return new WaitForSeconds(0.3f); // 소켓 해제 대기
        // }
        //
        // SceneController.LoadScene(SceneType.Main, 0.5f);
    }

    private IEnumerator FakeRematch(float waitTime)
    {
        oneCancelButtonPopup.OpenWithSetMessageAndButtonEvent("재대국 신청 중입니다...", 2f,
            () => { StartCoroutine(SafeExitGame()); });
        yield return new WaitForSeconds(waitTime);
        CloseButtonPopup();
        gameResultPopupPanel.gameObject.SetActive(false);
        gamePlayManager.ResterGame();
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
                twoButtonPopup.SetButtonText("거절", "수락");
                twoButtonPopup.OpenWithSetMessageAndButtonEvent("상대가 재대국 신청을 하였습니다.\n받으시겠습니까?",
                    () => { multiplayManager.multiplayController?.AcceptRematch(); },
                    () => { multiplayManager.multiplayController?.RejectRematch(); });
                break;
            case MultiplayControllerState.RematchRequestSent:
                oneCancelButtonPopup.OpenWithSetMessageAndButtonEvent("재대국 신청 중입니다...", 2f,
                    () => { multiplayManager.multiplayController?.CancelRematch(); });
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

    // 게임 종료 시, 기보 저장 함수 호출
    private void SaveGibo(GameResult result)
    {
        giboManager.SaveCurrentRecord();
    }

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