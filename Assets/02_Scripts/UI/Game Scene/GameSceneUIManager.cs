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
    [SerializeField] private Button surrenderButton; // 항복 버튼

    public PlayerProfileUIController playerProfileUIController { get; private set; }
    public TurnIndicatorUIController turnIndicatorUIController { get; private set; }
    public GamePopupController gamePopupController { get; private set; }

    public MultiplayUIHandler MultiplayUIHandler
    {
        get => multiplayUIHandler;
    }

    public event UnityAction OnCancelSurrender;

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

        // giboManager = GiboManager.Instance;
        // giboManager.StartNewRecord(); // 새 기보 데이터 생성

        if (gamePlayManager != null)
        {
            // gamePlayManager.OnGameEnd += SaveGibo;
            // gamePlayManager.OnGameRestart += StartGibo;
        }
    }

    private void OnDisable()
    {
        singlePlayUIHandler.gameObject.SetActive(false);
        multiplayUIHandler.gameObject.SetActive(false);

        if (gamePlayManager != null)
        {
            // gamePlayManager.OnGameEnd -= SaveGibo;
            // gamePlayManager.OnGameRestart -= StartGibo;
        }
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
    }

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
                if (GameModeManager.Mode == GameMode.SinglePlayer)
                {
                    OnCancelSurrender?.Invoke();
                }
            });
    }

    /// <summary>
    /// 항복 버튼의 상호작용 유무를 변경
    /// </summary>
    /// <param name="enable"></param>
    public void SurrenderButtonEnable(bool enable)
    {
        surrenderButton.interactable = enable;
    }

    #endregion

    // TODO: 기보 처리하기
    // 게임 종료 시, 기보 저장 함수 호출
    // private void SaveGibo(GameResult result)
    // {
    //     giboManager.SaveCurrentRecord();
    // }
    //
    // // 재대국 시, 기보 생성 함수 호출
    // private void StartGibo()
    // {
    //     giboManager.StartNewRecord();
    // }

    // private IEnumerator DelayedEventSubscription()
    // {
    //     // UI 요소들이 초기화될 때까지 기다림
    //     yield return new WaitForEndOfFrame();
    //     yield return new WaitForSeconds(0.1f);
    //
    //     if (multiplayManager != null)
    //     {
    //         Debug.Log("지연된 멀티플레이 이벤트 재구독 시작");
    //
    //         // 기존 구독 해제
    //         multiplayManager.MatchCallback -= HandleMatchUI;
    //         multiplayManager.MatchResultCallback -= OpenEndGamePanelInMultiplay;
    //         multiplayManager.RematchCallback -= HandleRematchUI;
    //         multiplayManager.ErrorCallback -= OnError;
    //
    //         // 이벤트 재구독
    //         multiplayManager.MatchCallback += HandleMatchUI;
    //         multiplayManager.MatchResultCallback += OpenEndGamePanelInMultiplay;
    //         multiplayManager.RematchCallback += HandleRematchUI;
    //         multiplayManager.ErrorCallback += OnError;
    //
    //         Debug.Log("지연된 멀티플레이 이벤트 재구독 완료");
    //     }
    // }
}