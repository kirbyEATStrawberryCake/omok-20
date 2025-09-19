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
    [SerializeField] private float turnTimeLimit = 30f; // �ϴ� ���� �ð� (��)

    private GamePlayManager gamePlayManager => GamePlayManager.Instance;
    private GameLogic gameLogic => gamePlayManager.gameLogic;

    private float currentTime;
    private bool isTimerActive = false;
    private bool isGameActive = false;

    public event UnityAction OnTimeUp; // �ð� �ʰ� �̺�Ʈ

    #region Unity Events

    private void OnEnable()
    {
        // GamePlayManager �̺�Ʈ ����
        gamePlayManager.OnGameStart += OnGameStart;
        gamePlayManager.OnGameEnd += OnGameEnd;

        // GameLogic �̺�Ʈ ����
        gameLogic.OnPlayerStonesRandomized += OnPlayerStonesRandomized;
        gameLogic.OnPlayerTurnChanged += OnPlayerTurnChanged;

        // BoardManager OnPlaceStone �̺�Ʈ ���� (���� �ÿ��� Ÿ�̸� �ʱ�ȭ)
        if (gamePlayManager.boardManager != null)
        {
            gamePlayManager.boardManager.OnPlaceStone += OnStonePlace;
        }
    }

    private void OnDisable()
    {
        // �̺�Ʈ ���� ����
        if (gamePlayManager != null)
        {
            gamePlayManager.OnGameStart -= OnGameStart;
            gamePlayManager.OnGameEnd -= OnGameEnd;
        }

        if (gameLogic != null)
        {
            gameLogic.OnPlayerStonesRandomized -= OnPlayerStonesRandomized;
            gameLogic.OnPlayerTurnChanged -= OnPlayerTurnChanged;
        }

        if (gamePlayManager?.boardManager != null)
        {
            gamePlayManager.boardManager.OnPlaceStone -= OnStonePlace;
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

    private void OnPlayerStonesRandomized()
    {
        // �� ���� �� ù �� ����
        StartTimer();
    }

    private void OnPlayerTurnChanged(StoneType currentStone)
    {
        // OnPlaceStone �̺�Ʈ�� ������ �� ���� �ÿ��� Ÿ�̸� ó��
        if (gamePlayManager.boardManager == null)
        {
            ResetTimer();
            StartTimer();
            return;
        }

        // OnPlaceStone �̺�Ʈ�� �ִ� ���, ���� �Ŀ� Ÿ�̸Ӱ� ó���ǹǷ� ���⼭�� ���۸�
        StartTimer();
    }

    private void OnStonePlace(int x, int y)
    {
        // ������ �̷������ Ÿ�̸� ���� �� ���� (���� �ϱ���)
        ResetTimer();
        StopTimer();
    }

    #endregion

    #region Timer Control Methods

    /// <summary>
    /// Ÿ�̸� ����
    /// </summary>
    public void StartTimer()
    {
        if (!isGameActive) return;

        // ��Ƽ�÷��̿����� �� ���� ���� Ÿ�̸� Ȱ��ȭ
        if (GameModeManager.Mode == GameMode.MultiPlayer)
        {
            bool isMyTurn = (gameLogic.currentTurnPlayer == PlayerType.Me);
            isTimerActive = isMyTurn;
        }
        else
        {
            // �̱��÷��̿����� �׻� Ȱ��ȭ
            isTimerActive = true;
        }

        Debug.Log($"Ÿ�̸� ���� - ���� ��: {gameLogic.currentTurnPlayer}, Ȱ��ȭ: {isTimerActive}");
    }

    /// <summary>
    /// Ÿ�̸� ����
    /// </summary>
    public void StopTimer()
    {
        isTimerActive = false;
    }

    /// <summary>
    /// Ÿ�̸� ����
    /// </summary>
    public void ResetTimer()
    {
        currentTime = turnTimeLimit;
        UpdateTimerUI();
    }

    /// <summary>
    /// �ð� �ʰ� ó��
    /// </summary>
    private void HandleTimeUp()
    {
        StopTimer();
        Debug.Log($"�ð� �ʰ�! ���� �� �÷��̾�: {gameLogic.currentTurnPlayer}");

        OnTimeUp?.Invoke();

        // ������ �� �ѱ��
        ForceSkipTurn();
    }

    /// <summary>
    /// ���� �� ��ŵ (�ð� �ʰ� ��)
    /// </summary>
    private void ForceSkipTurn()
    {
        if (gamePlayManager.currentGameState != GameState.Playing) return;

        if (GameModeManager.Mode == GameMode.MultiPlayer)
        {
            // ��Ƽ�÷���: �� �Ͽ��� �ð� �ʰ� �� ���� ������
            if (gameLogic.currentTurnPlayer == PlayerType.Me)
            {
                // ������ �� ��ŵ ���� (���� ���������� MultiplayManager�� ����)
                Debug.Log("��Ƽ�÷���: �ð� �ʰ��� �� ��ŵ");
                // gamePlayManager.multiplayManager.SendTurnSkip();
            }
        }
        else
        {
            // �̱��÷���: GameLogic�� SkipTurn �޼��� ȣ��
            Debug.Log("�̱��÷���: �ð� �ʰ��� �� ��ŵ");
            gameLogic.SkipTurn();
        }
    }

    #endregion

    #region UI Update

    /// <summary>
    /// Ÿ�̸� UI ������Ʈ
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
    /// ���� ���� �ð� ��ȯ
    /// </summary>
    public float GetRemainingTime()
    {
        return currentTime;
    }

    /// <summary>
    /// Ÿ�̸� Ȱ�� ���� ��ȯ
    /// </summary>
    public bool IsTimerActive()
    {
        return isTimerActive && isGameActive;
    }

    /// <summary>
    /// ���� �ð� ����
    /// </summary>
    public void SetTimeLimit(float timeLimit)
    {
        turnTimeLimit = timeLimit;
        ResetTimer();
    }
}
    #endregion
