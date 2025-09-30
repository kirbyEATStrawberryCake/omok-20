using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerProfileUIController))]
[RequireComponent(typeof(TurnIndicatorUIController))]
[RequireComponent(typeof(GamePopupController))]
public class GameSceneUIManager : Singleton<GameSceneUIManager>
{
    [SerializeField] private SinglePlayUIHandler singlePlayUIHandler;
    [SerializeField] private MultiplayUIHandler multiplayUIHandler;
    [Header("UI Components")]
    [SerializeField] private Button confirmMoveButton; // 착수 확정 버튼
    [SerializeField] private Button surrenderButton;   // 항복 버튼
    [SerializeField] private GameTimer gameTimer;

    public PlayerProfileUIController playerProfileUIController { get; private set; }
    public TurnIndicatorUIController turnIndicatorUIController { get; private set; }
    public GamePopupController gamePopupController { get; private set; }

    public MultiplayUIHandler MultiplayUIHandler
    {
        get => multiplayUIHandler;
    }

    private GamePlayManager gamePlayManager;

    #region Unity Life Cycle

    protected override void Awake()
    {
        base.Awake();

        playerProfileUIController = GetComponent<PlayerProfileUIController>();
        turnIndicatorUIController = GetComponent<TurnIndicatorUIController>();
        gamePopupController = GetComponent<GamePopupController>();
    }

    private void Start()
    {
        if (GameModeManager.Mode == GameMode.SinglePlayer)
            singlePlayUIHandler.gameObject.SetActive(true);
        else if (GameModeManager.Mode == GameMode.MultiPlayer)
            multiplayUIHandler.gameObject.SetActive(true);

        gamePlayManager = GamePlayManager.Instance;
    }

    private void OnDisable()
    {
        singlePlayUIHandler.gameObject.SetActive(false);
        multiplayUIHandler.gameObject.SetActive(false);
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode) { }

    #endregion

    #region 항복

    /// <summary>
    /// 항복 버튼을 눌렀을 때 호출되는 메소드
    /// </summary>
    public void OnClickSurrenderButton()
    {
        gamePopupController.OpenTwoButtonPopup("기권 하시겠습니까?",
            "기권", "취소",
            () => gamePlayManager.Surrender(),
            () =>
            {
                if (GameModeManager.Mode == GameMode.SinglePlayer) { gameTimer.ResumeTimer(); }
            });
    }

    /// <summary>
    /// 항복 버튼의 상호작용 유무를 변경
    /// </summary>
    /// <param name="enable"></param>
    public void SurrenderButtonEnable(bool enable) { surrenderButton.interactable = enable; }

    #endregion
}