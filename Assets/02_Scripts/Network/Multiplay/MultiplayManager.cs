using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MultiplayManager : Singleton<MultiplayManager>
{
    // 테스트용
    // public string username;
    // public string password;

    public MultiplayController multiplayController { get; private set; }

    private BoardManager boardManager;
    private GamePlayManager gamePlayManager;
    private StatsManager statsManager;
    private string roomId;
    public MatchData MatchData { get; private set; }

    public event UnityAction<MultiplayControllerState> MatchCallback;
    public event UnityAction<GameResultResponse, GameResult> MatchResultCallback;
    public event UnityAction<MultiplayControllerState> RematchCallback;
    public event UnityAction<string, bool> ErrorCallback;

    protected override void Awake()
    {
        base.Awake();

        if (GameModeManager.Mode == GameMode.SinglePlayer) return;

        multiplayController = new MultiplayController((state, response) =>
            {
                switch (state)
                {
                    // ---------- 매칭 ---------- 
                    case MultiplayControllerState.MatchFound:
                        this.roomId = response;
                        MatchCallback?.Invoke(state);
                        break;
                    case MultiplayControllerState.MatchWaiting:
                    case MultiplayControllerState.MatchExpanded:
                    case MultiplayControllerState.MatchCanceled:
                        MatchCallback?.Invoke(state);
                        break;
                    case MultiplayControllerState.MatchFailed:
                        GameModeManager.Mode = GameMode.AI;
                        MatchCallback?.Invoke(state);
                        break;
                    case MultiplayControllerState.OpponentSurrender:
                        Debug.Log("<color=cyan>상대방이 항복했습니다.</color>");
                        EndGame(GameResult.Victory);
                        break;
                    // ---------- 리매칭 ----------
                    case MultiplayControllerState.RematchRequested:
                    case MultiplayControllerState.RematchRequestSent:
                    case MultiplayControllerState.RematchRejected:
                    case MultiplayControllerState.RematchCanceled:
                    case MultiplayControllerState.RematchStarted:
                        RematchCallback?.Invoke(state);
                        break;
                    case MultiplayControllerState.OpponentLeft:
                    case MultiplayControllerState.ExitRoom:
                        this.roomId = null;
                        RematchCallback?.Invoke(state);
                        break;
                    // ---------- 에러 ----------
                    case MultiplayControllerState.Error:
                        ErrorCallback?.Invoke(response, true);
                        Debug.Log($"<color=red>에러! {response}</color>");
                        break;
                }
            },
            DoOpponent);

        multiplayController.Connect(GameManager.Instance.username);
    }

    private void Start()
    {
        if (GameModeManager.Mode == GameMode.SinglePlayer) return;

        statsManager = NetworkManager.Instance.statsManager;
        gamePlayManager = GamePlayManager.Instance;
        boardManager = gamePlayManager?.BoardManager;
        if (gamePlayManager != null)
        {
            gamePlayManager.OnGameEnd += EndGame;
            gamePlayManager.OnSurrender += multiplayController.Surrender;
        }
    }

    private void OnDisable()
    {
        if (GameModeManager.Mode == GameMode.SinglePlayer) return;

        if (gamePlayManager != null)
        {
            gamePlayManager.OnGameEnd -= EndGame;
            gamePlayManager.OnSurrender -= multiplayController.Surrender;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        multiplayController = null;
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex != (int)SceneType.Game) return;
        if (GameModeManager.Mode == GameMode.SinglePlayer) return;

        if (roomId != null) // 이미 매칭 중이라면
        {
            MatchCallback?.Invoke(MultiplayControllerState.MatchFound);
            return;
        }

        StartCoroutine(WaitForConnectionAndRequestMatch());
    }

    private IEnumerator WaitForConnectionAndRequestMatch()
    {
        while (!multiplayController.isConnected)
        {
            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log("소켓 연결 완료. 매칭 요청 시작.");
        multiplayController.RequestMatch();
    }

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

        // Debug.Log($"멀티플레이 착수 : {x}, {y}, roomId : {roomId}");
        multiplayController?.DoPlayer(roomId, x, y);
    }

    /// <summary>
    /// 상대방이 착수한 정보를 받아서 BoardManager로 넘겨줌
    /// </summary>
    private void DoOpponent(int x, int y)
    {
        Debug.Log($"<color=yellow>상대방 돌 생성: ({x}, {y})</color>");
        boardManager.PlaceOpponentStone(x, y);
    }

    /// <summary>
    /// 게임이 끝났을 때 서버로 게임 결과를 전송
    /// </summary>
    private void EndGame(GameResult result)
    {
        statsManager.UpdateGameResult(result,
            (response) =>
            {
                MatchResultCallback?.Invoke(response, result);
                Debug.Log($"<color=green>### 게임 결과({result.ToString()}) 등록 성공 ! ###</color>");
            },
            (errorType) =>
            {
                switch (errorType)
                {
                    case StatsResponseType.INVALID_GAME_RESULT:
                        Debug.LogError("<color=red>경기 결과 등록 실패 : 게임 결과가 올바르지 않습니다.</color>");
                        break;
                    case StatsResponseType.CANNOT_FOUND_USER:
                        Debug.LogError("<color=red>경기 결과 등록 실패 : 유저를 찾지 못했습니다.</color>");
                        break;
                    case StatsResponseType.NOT_LOGGED_IN:
                        Debug.LogError("<color=red>경기 결과 등록 실패 : 로그인 상태가 아닙니다.</color>");
                        break;
                }
            });

        multiplayController?.NotifyGameEnded();
    }

    /// <summary>
    /// 어플리케이션 종료 시 연결 끊음
    /// </summary>
    private void OnApplicationQuit()
    {
        multiplayController?.ApplicationQuit();
        multiplayController?.Dispose();
    }
}