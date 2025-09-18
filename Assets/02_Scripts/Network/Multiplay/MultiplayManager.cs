using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MultiplayManager : Singleton<MultiplayManager>
{
    // 테스트용
    public string username;
    public string password;

    public MultiplayController multiplayController { get; private set; }

    private GameSceneUIManager gameSceneUIManager => GameSceneUIManager.Instance;
    private string roomId;

    public event UnityAction MatchFoundCallback;

    protected override void Awake()
    {
        base.Awake();
        // TODO: 멀티모드 테스트
        GameModeManager.Mode = GameMode.MultiPlayer;
        AuthManager authManager = gameObject.AddComponent<AuthManager>();
        authManager.SignIn(username, password, () => Debug.Log("로그인 성공"), (e) => Debug.Log("로그인 실패"));

        multiplayController = new MultiplayController((state, response) =>
            {
                switch (state)
                {
                    case MultiplayControllerState.MatchWaiting:
                        // 매칭 중임을 알리는 팝업을 띄움
                        gameSceneUIManager?.OpenOneCancelButtonPopup("매칭 찾는 중...",
                            () => multiplayController.CancelMatch());
                        Debug.Log("<color=cyan>매칭 찾는 중...</color>");
                        break;
                    case MultiplayControllerState.MatchExpanded:
                        // 매칭 확장
                        Debug.Log("<color=yellow>매칭 범위 확장</color>");
                        break;
                    case MultiplayControllerState.MatchFound:
                        // 매칭 중임을 알리는 팝업을 강제로 닫음
                        gameSceneUIManager?.CloseOneButtonPopup();

                        this.roomId = response;
                        MatchFoundCallback.Invoke();
                        Debug.Log("<color=green>매칭 성공!</color>");
                        break;
                    case MultiplayControllerState.MatchCanceled:
                        // 매칭 취소 팝업을 띄움, 확인 버튼을 누르면 메인 씬으로 이동
                        gameSceneUIManager?.OpenOneConfirmButtonPopup("매칭이 취소되었습니다.",
                            () => SceneManager.LoadScene("Main_Scene"));
                        Debug.Log("<color=cyan>매칭을 취소합니다.</color>");
                        break;
                    case MultiplayControllerState.MatchFailed:
                        Debug.Log("<color=magenta>매칭 실패 AI 대전으로 전환합니다.</color>");
                        // TODO: AI대전으로 전환
                        break;
                    case MultiplayControllerState.ExitRoom:
                        // messageTest?.ClearAllMessage();
                        // messageTest?.SetMessage(1, "방에서 나갑니다.", Color.magenta);
                        Debug.Log("<color=magenta>방에서 나갑니다.</color>");
                        break;
                    case MultiplayControllerState.OpponentLeft:
                        // messageTest?.ClearAllMessage();
                        // messageTest?.SetMessage(1, "상대방이 나갔습니다.", Color.cyan);
                        Debug.Log("<color=cyan>상대방이 나갔습니다.</color>");
                        break;
                    case MultiplayControllerState.Error:
                        // messageTest?.ClearAllMessage();
                        // messageTest?.SetMessage(1, "에러! " + response, Color.red);
                        gameSceneUIManager?.OpenOneConfirmButtonPopup($"오류가 발생했습니다.\n {response}",
                            () => SceneManager.LoadScene("Main_Scene"));
                        Debug.Log($"<color=red>에러! {response}</color>");
                        break;
                }
            },
            DoOpponent);

        // 소켓 연결
        multiplayController.Connect(username);
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Game_Scene") return;
        // 싱글 플레이로 진입 시 무시
        if (GameModeManager.Mode == GameMode.SinglePlayer) return;

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

    public void GoStone(int x = -1, int y = -1)
    {
        if (x == -1 || y == -1)
        {
            gameSceneUIManager?.OpenOneConfirmButtonPopup($"착수 실패\n좌표가 비어있습니다.");
            Debug.LogError("<color=red>착수 실패 : 좌표가 비어있습니다.</color>");
            return;
        }

        Debug.Log($"착수 : {x}, {y}, roomId : {roomId}");
        multiplayController?.DoPlayer(roomId, x, y);
    }

    private void DoOpponent(int x, int y)
    {
        Debug.Log("<color=yellow>x : " + x + ", y : " + y + "</color>");
        GamePlayManager.Instance.boardManager.PlaceOpponentStone(x, y);
    }

    /// <summary>
    /// 어플리케이션 종료 시 연결 끊음
    /// </summary>
    private void OnApplicationQuit()
    {
        multiplayController?.LeaveRoom();
        multiplayController?.Dispose();
    }
}