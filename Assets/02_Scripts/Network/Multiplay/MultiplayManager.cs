using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// 멀티플레이 게임의 전반적인 관리를 담당하는 싱글톤 매니저
/// 매칭, 게임 진행, 결과 처리 등을 관리
/// </summary>
public class MultiplayManager : Singleton<MultiplayManager>
{
    #region Properties

    public MultiplayController multiplayController { get; private set; }
    public MatchData MatchData { get; private set; }

    #endregion


    #region Private Fields

    private BoardManager boardManager;
    private GamePlayManager gamePlayManager;
    private StatsManager statsManager;
    private string roomId;

    #endregion

    #region Events

    public event UnityAction<MultiplayControllerState> MatchCallback;
    public event UnityAction<GameResultResponse, GameResult> MatchResultCallback;
    public event UnityAction<MultiplayControllerState> RematchCallback;
    public event UnityAction<string, bool> ErrorCallback;
    public event UnityAction OnRoomLeft;

    #endregion

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake();
        if (Instance == this)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        statsManager = NetworkManager.Instance.statsManager;
    }

    private void OnDisable()
    {
        UnsubscribeFromGamePlayManagerEvents();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        multiplayController?.Dispose();
        multiplayController = null;
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex != (int)SceneType.Game) return;
        if (GameModeManager.Mode == GameMode.SinglePlayer) return;

        if (multiplayController == null)
        {
            InitializeMultiplayController();
        }

        StartCoroutine(DelayedGameSceneInitialization());
    }

    protected override void OnApplicationQuit()
    {
        multiplayController?.ApplicationQuit();
        multiplayController?.Dispose();
        multiplayController = null;
    }

    #endregion

    #region Initialization

    /// <summary>
    /// MultiplayController 초기화
    /// </summary>
    private void InitializeMultiplayController()
    {
        try
        {
            multiplayController = new MultiplayController(
                HandleMultiplayStateChanged,
                HandleOpponentMove);

            multiplayController.Connect(GameManager.Instance.username);
            Debug.Log("<color=green>MultiplayController 생성 및 연결 완료</color>");
        }
        catch (Exception ex)
        {
            Debug.LogError($"<color=red>MultiplayController 초기화 실패: {ex.Message}</color>");
            multiplayController = null;
        }
    }

    /// <summary>
    /// 게임 씬에서의 지연된 초기화
    /// </summary>
    private IEnumerator DelayedGameSceneInitialization()
    {
        yield return StartCoroutine(WaitForGamePlayManager());

        if (gamePlayManager == null)
        {
            Debug.LogError("<color=red>GamePlayManager 초기화 시간 초과!</color>");
            yield break;
        }

        boardManager = gamePlayManager.BoardManager; // 참조 업데이트
        SubscribeToGamePlayManagerEvents();

        if (roomId != null)
        {
            MatchCallback?.Invoke(MultiplayControllerState.MatchFound);
            Debug.Log($"<color=cyan>기존 매칭 복원 - 방 ID: {roomId}</color>");
        }
        else
        {
            StartCoroutine(WaitForConnectionAndRequestMatch());
        }
    }

    /// <summary>
    /// GamePlayManager가 준비될 때까지 대기
    /// </summary>
    private IEnumerator WaitForGamePlayManager()
    {
        float timeout = 5f;
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            gamePlayManager = GamePlayManager.Instance;
            if (gamePlayManager != null && gamePlayManager.GameLogicController != null)
            {
                Debug.Log("<color=green>GamePlayManager 준비 완료</color>");
                yield break;
            }

            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
    }

    #endregion

    #region Event Management

    /// <summary>
    /// GamePlayManager 이벤트 구독
    /// </summary>
    private void SubscribeToGamePlayManagerEvents()
    {
        if (!ValidateEventSubscriptionRequirements()) return;

        try
        {
            UnsubscribeFromGamePlayManagerEvents();

            gamePlayManager.OnGameEnd += HandleGameEnd;
            gamePlayManager.OnSurrender += multiplayController.Surrender;

            Debug.Log("<color=green>GamePlayManager 이벤트 구독 완료</color>");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"<color=red>이벤트 구독 실패: {ex.Message}</color>");
        }
    }

    /// <summary>
    /// GamePlayManager 이벤트 구독 해제
    /// </summary>
    private void UnsubscribeFromGamePlayManagerEvents()
    {
        if (gamePlayManager == null) return;

        try
        {
            gamePlayManager.OnGameEnd -= HandleGameEnd;

            if (multiplayController != null)
            {
                gamePlayManager.OnSurrender -= multiplayController.Surrender;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"<color=yellow>이벤트 구독 해제 중 오류 (무시): {ex.Message}</color>");
        }
    }

    /// <summary>
    /// 이벤트 구독 요구사항 검증
    /// </summary>
    private bool ValidateEventSubscriptionRequirements()
    {
        if (gamePlayManager == null)
        {
            Debug.LogWarning("<color=yellow>GamePlayManager가 null입니다.</color>");
            return false;
        }

        if (multiplayController == null)
        {
            Debug.LogWarning("<color=yellow>MultiplayController가 null입니다.</color>");
            return false;
        }

        return true;
    }

    #endregion

    #region Public API

    /// <summary>
    /// 매칭 완료시 서버에서 보낸 상대방 정보를 저장하는 메소드
    /// </summary>
    public void SetOpponentData(MatchData data)
    {
        MatchData = data;
    }

    /// <summary>
    /// 착수 정보를 서버로 보냄
    /// </summary>
    public void GoStone(int x = -1, int y = -1)
    {
        if (x == -1 || y == -1)
        {
            string message = $"착수 실패\n좌표가 비어있습니다.";
            ErrorCallback?.Invoke(message, false);
            Debug.LogError("<color=red>착수 실패 : 좌표가 비어있습니다.</color>");
            return;
        }

        if (multiplayController == null)
        {
            Debug.LogError("<color=red>MultiplayController가 null입니다.</color>");
            return;
        }


        // Debug.Log($"멀티플레이 착수 : {x}, {y}, roomId : {roomId}");
        multiplayController?.DoPlayer(roomId, x, y);
    }

    #endregion

    #region Multiplay State Handling

    /// <summary>
    /// 멀티플레이 상태 변화 처리
    /// </summary>
    private void HandleMultiplayStateChanged(MultiplayControllerState state, string response)
    {
        switch (state)
        {
            case MultiplayControllerState.MatchFound:
                HandleMatchFound(response);
                break;

            case MultiplayControllerState.MatchWaiting:
            case MultiplayControllerState.MatchExpanded:
            case MultiplayControllerState.MatchCanceled:
                MatchCallback?.Invoke(state);
                break;

            case MultiplayControllerState.MatchFailed:
                HandleMatchFailed();
                break;

            case MultiplayControllerState.OpponentSurrender:
                HandleOpponentSurrender();
                break;

            case MultiplayControllerState.RematchRequested:
            case MultiplayControllerState.RematchRequestSent:
            case MultiplayControllerState.RematchRejected:
            case MultiplayControllerState.RematchCanceled:
            case MultiplayControllerState.RematchStarted:
                RematchCallback?.Invoke(state);
                break;

            case MultiplayControllerState.OpponentLeft:
                HandleOpponentLeft();
                break;

            case MultiplayControllerState.ExitRoom:
                HandleExitRoom();
                break;

            case MultiplayControllerState.Error:
                HandleError(response);
                break;

            default:
                Debug.LogWarning($"<color=yellow>처리되지 않은 멀티플레이 상태: {state}</color>");
                break;
        }
    }

    private void HandleMatchFound(string response)
    {
        roomId = response;
        MatchCallback?.Invoke(MultiplayControllerState.MatchFound);
        Debug.Log($"<color=green>매칭 성공 - 방 ID: {roomId}</color>");
    }

    private void HandleMatchFailed()
    {
        GameModeManager.Mode = GameMode.AI;
        MatchCallback?.Invoke(MultiplayControllerState.MatchFailed);
        Debug.Log("<color=yellow>매칭 실패 - AI 모드로 전환</color>");
    }

    private void HandleOpponentSurrender()
    {
        Debug.Log("<color=cyan>상대방이 항복했습니다.</color>");
        gamePlayManager?.EndGame(GameResult.Victory);
    }

    private void HandleOpponentLeft()
    {
        roomId = null;
        RematchCallback?.Invoke(MultiplayControllerState.OpponentLeft);
        Debug.Log("<color=yellow>상대방이 나갔습니다.</color>");
    }

    private void HandleExitRoom()
    {
        OnRoomLeft?.Invoke();
        roomId = null;
        RematchCallback?.Invoke(MultiplayControllerState.ExitRoom);
        Debug.Log("<color=yellow>방에서 나갔습니다.</color>");
    }

    private void HandleError(string response)
    {
        ErrorCallback?.Invoke(response, true);
        Debug.LogError($"<color=red>멀티플레이 에러: {response}</color>");
    }

    #endregion

    #region Game Flow

    /// <summary>
    /// 연결 대기 후 매칭 요청
    /// </summary>
    private IEnumerator WaitForConnectionAndRequestMatch()
    {
        if (multiplayController == null)
        {
            Debug.LogError("<color=red>MultiplayController가 null입니다.</color>");
            yield break;
        }

        while (!multiplayController.isConnected)
        {
            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log("<color=green>소켓 연결 완료. 매칭 요청 시작.</color>");
        multiplayController.RequestMatch();
    }

    /// <summary>
    /// 상대방 수 처리
    /// </summary>
    private void HandleOpponentMove(int x, int y)
    {
        Debug.Log($"<color=yellow>상대방 착수: ({x}, {y})</color>");
        // boardManager가 null인 경우 다시 찾아서 설정
        if (boardManager == null)
        {
            gamePlayManager = GamePlayManager.Instance;
            boardManager = gamePlayManager?.BoardManager;
        }

        if (boardManager == null)
        {
            Debug.LogError("<color=red>BoardManager를 찾을 수 없습니다!</color>");
            return;
        }

        boardManager.PlaceOpponentStone(x, y);
    }

    /// <summary>
    /// 게임이 끝났을 때 서버로 게임 결과를 전송
    /// </summary>
    private void HandleGameEnd(GameResult result)
    {
        statsManager.UpdateGameResult(result,
            (response) =>
            {
                MatchResultCallback?.Invoke(response, result);
                Debug.Log($"<color=green>### 게임 결과({result.ToString()}) 등록 성공 ! ###</color>");
            },
            HandleGameResultError);

        multiplayController?.NotifyGameEnded();
    }

    /// <summary>
    /// 게임 결과 등록 실패 처리
    /// </summary>
    private void HandleGameResultError(StatsResponseType errorType)
    {
        string errorMessage = errorType switch
        {
            StatsResponseType.INVALID_GAME_RESULT => "게임 결과가 올바르지 않습니다.",
            StatsResponseType.CANNOT_FOUND_USER => "유저를 찾지 못했습니다.",
            StatsResponseType.NOT_LOGGED_IN => "로그인 상태가 아닙니다.",
            _ => "알 수 없는 오류가 발생했습니다."
        };

        Debug.LogError($"<color=red>게임 결과 등록 실패: {errorMessage}</color>");
    }

    #endregion
}