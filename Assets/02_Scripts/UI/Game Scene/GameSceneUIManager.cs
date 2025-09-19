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

    private Sprite pandaSprite;
    private Sprite redPandaSprite;


    [Header("팝업")]
    [SerializeField] [Tooltip("게임 씬에서 사용할 확인 버튼 1개짜리 팝업")]
    private GameObject oneConfirmButtonPopupPanel;

    [SerializeField] [Tooltip("메인 씬에서 사용할 취소 버튼 1개짜리 팝업")]
    private GameObject oneCancelButtonPopupPanel;

    [SerializeField] [Tooltip("게임 씬에서 사용할 버튼 2개짜리 팝업")]
    private GameObject twoButtonPopupPanel;

    [SerializeField] [Tooltip("게임 결과창 팝업")]
    private GameObject gameResultPopupPanel;

    private GamePlayManager gamePlayManager => GamePlayManager.Instance;

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

    private void OnEnable()
    {
        gamePlayManager.OnGameEnd += EndGameUI;
        gamePlayManager.gameLogic.OnPlayerStonesRandomized += InitPlayerTurnDisplay;
        gamePlayManager.gameLogic.OnPlayerTurnChanged += UpdatePlayerTurnDisplay;
        if (GameModeManager.Mode == GameMode.MultiPlayer)
        {
            gamePlayManager.multiplayManager.MatchFoundCallback += UpdatePlayerProfileInMultiPlay;
        }
    }

    private void OnDisable()
    {
        gamePlayManager.OnGameEnd -= EndGameUI;
        gamePlayManager.gameLogic.OnPlayerStonesRandomized -= InitPlayerTurnDisplay;
        gamePlayManager.gameLogic.OnPlayerTurnChanged -= UpdatePlayerTurnDisplay;
        if (GameModeManager.Mode == GameMode.MultiPlayer)
        {
            gamePlayManager.multiplayManager.MatchFoundCallback -= UpdatePlayerProfileInMultiPlay;
        }
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
    }

    /// <summary>
    /// 한 개짜리 확인 버튼 팝업 열기
    /// </summary>
    /// <param name="message">팝업창에 띄울 메세지</param>
    /// <param name="onConfirm">버튼을 눌렀을 때 실행할 액션</param>
    public void OpenOneConfirmButtonPopup(string message, Action onConfirm = null)
    {
        oneConfirmButtonPopup.OpenWithSetMessageAndButtonEvent(message, onConfirm);
    }

    /// <summary>
    /// 한 개짜리 취소 버튼 팝업 열기
    /// </summary>
    /// <param name="message">팝업창에 띄울 메세지</param>
    /// <param name="onCancel">버튼을 눌렀을 때 실행할 액션</param>
    public void OpenOneCancelButtonPopup(string message, Action onCancel = null)
    {
        oneCancelButtonPopup.OpenWithSetMessageAndButtonEvent(message, onCancel);
    }

    /// <summary>
    /// 한 개짜리 취소 버튼 팝업을 강제로 닫기
    /// </summary>
    public void CloseOneButtonPopup()
    {
        oneCancelButtonPopupPanel.SetActive(false);
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
        var currentTurn = gamePlayManager.gameLogic.currentTurnPlayer;
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
        Debug.Log($"{gameManager.grade}급 {gameManager.nickname} {gameManager.profileImage}");
        // 상대의 프로필은 서버에서 받아온 정보를 사용하여 프로필 업데이트
        MatchFoundData opponentData = MultiplayManager.Instance.matchFoundData;
        player2ProfileImage.sprite = opponentData.profileImage == 1 ? pandaSprite : redPandaSprite;
        player2GradeAndNickname.text = $"{opponentData.grade}급 {opponentData.nickname}";
        Debug.Log($"{opponentData.grade}급 {opponentData.nickname} {opponentData.profileImage}");
    }

    /// <summary>
    /// 게임이 끝날때 호출되는 메소드
    /// </summary>
    /// <param name="result">게임 결과</param>
    private void EndGameUI(GameResult result)
    {
        if (gameResultPopup != null)
        {
            gameResultPopup.OpenWithButtonEvent(result, () => { SceneManager.LoadScene("Main_Scene"); }, () =>
            {
                if (GameModeManager.Mode == GameMode.SinglePlayer)
                {
                    // TODO: 재대국 로직(싱글)                    
                }
                else if (GameModeManager.Mode == GameMode.MultiPlayer)
                {
                    // TODO: 재대국 로직(멀티)
                }
            });
        }
    }

    /// <summary>
    /// 항복 버튼을 눌렀을 때 호출되는 메소드
    /// </summary>
    public void OnClickSurrenderButton()
    {
        // TODO: 타이머 멈추기
        twoButtonPopup.OpenWithSetMessageAndButtonEvent("기권 하시겠습니까?", () => { gamePlayManager.Surrender(); }, () =>
        {
            // TODO: 타이머 다시 시작
        });
    }
}