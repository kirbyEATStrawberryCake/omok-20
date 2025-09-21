using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour
{
    [Header("Timer UI References")]
    [SerializeField] private Image timerImage;

    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Timer Settings")]
    [SerializeField] private float turnTimeLimit = 30f; // 턴당 제한 시간 (초)

    private GamePlayManager gamePlayManager;
    private GameSceneUIManager uiManager;
    private GameLogicController gameLogic;

    private float currentTime;
    private bool isTimerActive = false;
    private bool isGameActive = false;

    public event UnityAction OnTimeUp; // 시간 초과 이벤트

    #region Unity Events

    private void Start()
    {
        gamePlayManager = GamePlayManager.Instance;
        uiManager = gamePlayManager.UIManager;
        gameLogic = gamePlayManager.GameLogicController;

        // GamePlayManager 이벤트 구독
        if (gamePlayManager != null)
        {
            gamePlayManager.OnGameStart += OnGameStart;
            gamePlayManager.OnGameEnd += OnGameEnd;
        }

        if (uiManager != null)
        {
            uiManager.OnCancelSurrender += StartTimer;
        }

        // GameLogic 이벤트 구독
        if (gameLogic != null)
        {
            gameLogic.OnPlayerTurnChanged += OnPlayerTurnChanged;
        }
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        if (gamePlayManager != null)
        {
            gamePlayManager.OnGameStart -= OnGameStart;
            gamePlayManager.OnGameEnd -= OnGameEnd;
        }

        if (uiManager != null)
        {
            uiManager.OnCancelSurrender -= StartTimer;
        }

        if (gameLogic != null)
        {
            gameLogic.OnPlayerTurnChanged -= OnPlayerTurnChanged;
        }
    }

    private void Update()
    {
        if (!isTimerActive || !isGameActive) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            HandleTimeUp();
        }

        UpdateTimerUI();
    }

    #endregion

    #region Event Handlers

    private void OnGameStart()
    {
        isGameActive = true;
        ResetTimer();
    }

    private void OnGameEnd(GameResult result)
    {
        isGameActive = false;
        StopTimer();
    }

    private void OnPlayerTurnChanged(StoneType currentStone)
    {
        // 턴이 변경될 때마다 타이머 리셋 및 시작
        ResetTimer();
        if (isGameActive)
        {
            StartTimer();
        }
        else
        {
            StartCoroutine(WaitForGameActiveAndStart());
        }

        // Debug.Log($"턴 변경됨 - 현재 턴: {currentStone}, 타이머 리셋 및 시작");
    }

    private IEnumerator WaitForGameActiveAndStart()
    {
        while (!isGameActive)
        {
            yield return null;
        }

        StartTimer();
    }

    #endregion

    #region Timer Control Methods

    /// <summary>
    /// 타이머 시작
    /// </summary>
    public void StartTimer()
    {
        if (!isGameActive) return;
        isTimerActive = true;
    }

    /// <summary>
    /// 타이머 정지
    /// </summary>
    public void StopTimer()
    {
        if (GameModeManager.Mode == GameMode.SinglePlayer)
            isTimerActive = false;
    }

    /// <summary>
    /// 타이머 리셋
    /// </summary>
    private void ResetTimer()
    {
        currentTime = turnTimeLimit;
        UpdateTimerUI();
    }

    /// <summary>
    /// 시간 초과 처리
    /// </summary>
    private void HandleTimeUp()
    {
        StopTimer();
        Debug.Log($"시간 초과! 현재 턴 플레이어: {gameLogic.GetCurrentTurnPlayer()}");

        OnTimeUp?.Invoke();

        // 다음 턴을 위해 타이머 리셋
        ResetTimer();
    }

    #endregion

    #region UI Update

    /// <summary>
    /// 타이머 UI 업데이트
    /// </summary>
    private void UpdateTimerUI()
    {
        if (timerImage != null)
        {
            timerImage.fillAmount = currentTime / turnTimeLimit;
        }

        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(currentTime).ToString();
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 현재 남은 시간 반환
    /// </summary>
    public float GetRemainingTime()
    {
        return currentTime;
    }

    /// <summary>
    /// 타이머 활성 상태 반환
    /// </summary>
    public bool IsTimerActive()
    {
        return isTimerActive && isGameActive;
    }

    /// <summary>
    /// 제한 시간 설정
    /// </summary>
    public void SetTimeLimit(float timeLimit)
    {
        turnTimeLimit = timeLimit;
        ResetTimer();
    }

    #endregion
}