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
    private GameLogicController gameLogic;

    private float currentTime;
    private bool isTimerActive = false;
    private bool isGameActive = false;

    public event UnityAction OnTimeUp; // 시간 초과 이벤트

    #region Unity Events

    private void Start()
    {
        gamePlayManager = GamePlayManager.Instance;
        gameLogic = gamePlayManager.GameLogicController;

        // GamePlayManager 이벤트 구독
        gamePlayManager.OnGameStart += OnGameStart;
        gamePlayManager.OnGameEnd += OnGameEnd;

        // GameLogic 이벤트 구독
        if (gameLogic != null)
        {
            gameLogic.OnPlayerTurnChanged += OnPlayerTurnChanged;
        }

        // BoardManager OnPlaceStone 이벤트 구독 (착수 시에만 타이머 초기화)
        if (gamePlayManager.BoardManager != null)
        {
            // gamePlayManager.BoardManager.OnPlaceStone += OnStonePlace;
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

        if (gameLogic != null)
        {
            gameLogic.OnPlayerTurnChanged -= OnPlayerTurnChanged;
        }

        if (gamePlayManager?.BoardManager != null)
        {
            // gamePlayManager.BoardManager.OnPlaceStone -= OnStonePlace;
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

        Debug.Log($"턴 변경됨 - 현재 턴: {currentStone}, 타이머 리셋 및 시작");
    }

    private IEnumerator WaitForGameActiveAndStart()
    {
        while (!isGameActive)
        {
            yield return null;
        }

        StartTimer();
    }

    // private void OnStonePlace(int x, int y)
    // {
    //     // 실제 착수가 이루어졌을 때만 타이머 리셋 및 정지
    //     Debug.Log($"착수 완료: ({x}, {y}) - 타이머 리셋 및 정지");
    //     ResetTimer();
    //     StopTimer();
    // }

    #endregion

    #region Timer Control Methods

    /// <summary>
    /// 타이머 시작
    /// </summary>
    private void StartTimer()
    {
        if (!isGameActive) return;
        isTimerActive = true;

        // // 멀티플레이에서는 내 턴일 때만 타이머 활성화
        // if (GameModeManager.Mode == GameMode.MultiPlayer)
        // {
        //     bool isMyTurn = (gameLogic.GetCurrentTurnPlayer() == PlayerType.Me);
        //     isTimerActive = isMyTurn;
        // }
        // else
        // {
        //     // 싱글플레이에서는 항상 활성화
        //     isTimerActive = true;
        // }

        Debug.Log($"타이머 시작 - 현재 턴: {gameLogic.GetCurrentTurnPlayer()}, 활성화: {isTimerActive}");
    }

    /// <summary>
    /// 타이머 정지
    /// </summary>
    private void StopTimer()
    {
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

        // 강제로 턴 넘기기
        // ForceSkipTurn();
    }

    /// <summary>
    /// 강제 턴 스킵 (시간 초과 시)
    /// </summary>
    private void ForceSkipTurn()
    {
        if (gamePlayManager.currentGameState != GameState.Playing) return;

        if (GameModeManager.Mode == GameMode.MultiPlayer)
        {
            // 멀티플레이: 내 턴에서 시간 초과 시 상대방 턴으로
            if (gameLogic.GetCurrentTurnPlayer() == PlayerType.Me)
            {
                // 서버에 턴 스킵 전송 (실제 구현에서는 MultiplayManager를 통해)
                Debug.Log("멀티플레이: 시간 초과로 턴 스킵");
                // gamePlayManager.multiplayManager.SendTurnSkip();
            }
        }
        else
        {
            // 싱글플레이: GameLogic의 SkipTurn 메서드 호출
            Debug.Log("싱글플레이: 시간 초과로 턴 스킵");
            // gameLogic.SkipTurn();
        }
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