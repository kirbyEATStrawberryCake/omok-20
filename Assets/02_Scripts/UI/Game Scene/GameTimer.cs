using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour
{
    [Header("Timer UI References")]
    [SerializeField] private Image timerImage;

    [SerializeField] private TMP_Text timerText;

    [Header("Timer Settings")]
    [SerializeField] private float turnTimeLimit = 30f; // 턴당 제한 시간 (초)

    private float currentTime;
    private bool isTimerActive = false;

    public event UnityAction OnTimeUp; // 시간 초과 이벤트

    #region Unity Life Cycle

    private void Update()
    {
        if (!isTimerActive) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            HandleTimeUp();
        }

        UpdateTimerUI();
    }

    #endregion

    #region Timer Control Methods

    /// <summary>
    /// 타이머 시작
    /// </summary>
    public void StartTimer()
    {
        isTimerActive = true;
        ResetTimer();
    }

    /// <summary>
    /// 타이머 정지
    /// </summary>
    public void StopTimer()
    {
        if (GameModeManager.Mode == GameMode.SinglePlayer ||
            ((GameModeManager.Mode == GameMode.MultiPlayer || GameModeManager.Mode == GameMode.AI) &&
             GamePlayManager.Instance.currentGameState == GameState.GameOver))
            isTimerActive = false;
    }

    /// <summary>
    /// 타이머를 멈춘 곳에서 다시 시작
    /// </summary>
    public void ResumeTimer() { isTimerActive = true; }

    /// <summary>
    /// 타이머 리셋
    /// </summary>
    public void ResetTimer()
    {
        currentTime = turnTimeLimit;
        UpdateTimerUI();
    }

    /// <summary>
    /// 시간 초과 처리
    /// </summary>
    private void HandleTimeUp()
    {
        Debug.Log($"시간 초과!");

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
        if (timerImage != null) { timerImage.fillAmount = currentTime / turnTimeLimit; }

        if (timerText != null) { timerText.text = Mathf.CeilToInt(currentTime).ToString(); }
    }

    #endregion
}