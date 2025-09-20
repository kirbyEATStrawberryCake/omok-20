using System;
using Newtonsoft.Json;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class UserInfo
{
    [JsonProperty("userId")] public string userId;
    [JsonProperty("username")] public string username;
    [JsonProperty("nickname")] public string nickname;
    [JsonProperty("grade")] public int grade;
}

[Serializable]
public class MatchData
{
    [JsonProperty("roomId")] public string roomId;
    [JsonProperty("userId")] public string userId;
    [JsonProperty("username")] public string username;
    [JsonProperty("nickname")] public string nickname;
    [JsonProperty("grade")] public int grade;
    [JsonProperty("profileImage")] public int profileImage;
    [JsonProperty("isPlayer1First")] public bool isPlayer1First;
}

[Serializable]
public class MessageData
{
    [JsonProperty("message")] public string message;
}

[Serializable]
public class RoomData
{
    [JsonProperty("roomId")] public string roomId { get; set; }
}

[Serializable]
public class BlockData
{
    [JsonProperty("blockIdx_x")] public int blockIdx_x { get; set; }
    [JsonProperty("blockIdx_y")] public int blockIdx_y { get; set; }
}

public class MultiplayController : IDisposable
{
    private SocketIOUnity socket;
    public bool isConnected { get; private set; } = false;

    // 사용자 정보
    public UserInfo CurrentUserInfo { get; private set; }
    private string username;
    public bool amIFirstPlayer { get; private set; }

    // Room 상태 변화에 따른 액션
    private Action<MultiplayControllerState, string> onMultiplayStateChanged;

    // 게임 진행 상황에서 돌의 위치를 업데이트하는 액션
    public Action<int, int> onBlockDataChanged;

    public MultiplayController(Action<MultiplayControllerState, string> onMultiplayStateChanged,
        Action<int, int> onBlockDataChanged)
    {
        this.onMultiplayStateChanged = onMultiplayStateChanged;
        this.onBlockDataChanged = onBlockDataChanged;

        var uri = new Uri(NetworkManager.SocketServerURL);
        socket = new SocketIOUnity(uri,
            new SocketIOOptions { Transport = SocketIOClient.Transport.TransportProtocol.WebSocket });
        socket.JsonSerializer = new NewtonsoftJsonSerializer();

        // // 연결 상태 이벤트
        // socket.OnConnected += (sender, e) =>
        // {
        //     isConnected = true;
        //     Debug.Log("소켓 연결됨");
        // };
        //
        // socket.OnDisconnected += (sender, e) =>
        // {
        //     isConnected = false;
        //     Debug.Log("소켓 연결 해제됨");
        // };

        // 유저 정보 수신
        socket.OnUnityThread("userInfoLoaded", UserInfoLoaded);
        // 상대 착수 정보 수신
        socket.OnUnityThread("doOpponent", DoOpponent);
        // 상대 항복 정보 수신
        socket.OnUnityThread("opponentSurrender", OpponentSurrender);
        // 상대방이 방에서 나갔을 때 정보 수신
        socket.OnUnityThread("opponentLeft", OpponentLeft);
        // 내가 방을 나갔을때 정보 수신
        socket.OnUnityThread("exitRoom", ExitRoom);

        // 매칭 관련
        socket.OnUnityThread("matchWaiting", MatchWaiting);
        socket.OnUnityThread("matchExpanded", MatchExpanded);
        socket.OnUnityThread("matchFound", MatchFound);
        socket.OnUnityThread("matchFailed", MatchFailed);
        socket.OnUnityThread("matchCanceled", MatchCanceled);

        // 리매칭 관련
        socket.OnUnityThread("rematchRequested", RematchRequested); // 상대방의 리매치 요청
        socket.OnUnityThread("rematchRequestSent", RematchRequestSent); // 내가 상대방에게 리매치 요청을 함
        socket.OnUnityThread("rematchRejected", RematchRejected); // 상대방의 리매치 거절
        socket.OnUnityThread("rematchCanceled", RematchCanceled); // 상대방이 리매치를 취소
        socket.OnUnityThread("rematchStarted", RematchStarted); // 리매치가 성사됨

        // 에러
        socket.OnUnityThread("matchError", MatchError);
        socket.OnUnityThread("authRequired", AuthRequired);
        socket.OnUnityThread("serverError", ServerError);
        socket.OnUnityThread("userNotFound", UserNotFound);
        socket.OnUnityThread("rematchError", RematchError);
    }

    public void Connect(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            Debug.LogError("로그인 정보가 없습니다.");
            return;
        }

        this.username = username;
        socket?.Connect();

        // 연결 후 사용자 정보 전송
        if (socket != null)
        {
            socket.OnConnected += OnConnected;
            socket.OnDisconnected += OnDisconnected;
        }
    }

    private void OnConnected(object sender, EventArgs e)
    {
        socket.Emit("authenticate", username);
        isConnected = true;
        Debug.Log("소켓 연결됨");
    }

    private void OnDisconnected(object sender, string e)
    {
        isConnected = false;
        Debug.Log("소켓 연결 해제됨");
    }

    #region Server -> Client

    private void UserInfoLoaded(SocketIOResponse response)
    {
        CurrentUserInfo = response.GetValue<UserInfo>();
    }

    private void DoOpponent(SocketIOResponse response)
    {
        var data = response.GetValue<BlockData>();
        onBlockDataChanged?.Invoke(data.blockIdx_x, data.blockIdx_y);
    }

    private void OpponentSurrender(SocketIOResponse obj)
    {
        onMultiplayStateChanged?.Invoke(MultiplayControllerState.OpponentSurrender, null);
    }

    private void OpponentLeft(SocketIOResponse response)
    {
        onMultiplayStateChanged?.Invoke(MultiplayControllerState.OpponentLeft, null);
    }

    private void ExitRoom(SocketIOResponse response)
    {
        var data = response.GetValue<RoomData>();

        onMultiplayStateChanged?.Invoke(MultiplayControllerState.ExitRoom, data.roomId);
    }

    #endregion

    #region Match (Server -> Client)

    private void MatchWaiting(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();

        onMultiplayStateChanged?.Invoke(MultiplayControllerState.MatchWaiting, data.message);
    }

    private void MatchExpanded(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();

        onMultiplayStateChanged?.Invoke(MultiplayControllerState.MatchExpanded, data.message);
    }

    private void MatchFound(SocketIOResponse response)
    {
        var data = response.GetValue<MatchData>();

        amIFirstPlayer = data.isPlayer1First;
        Debug.Log($"내가 선공인가?: {amIFirstPlayer}");
        MultiplayManager.Instance?.SetOpponentData(data);

        onMultiplayStateChanged?.Invoke(MultiplayControllerState.MatchFound, data.roomId);
    }

    private void MatchFailed(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();

        onMultiplayStateChanged?.Invoke(MultiplayControllerState.MatchFailed, data.message);
    }

    private void MatchCanceled(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();

        onMultiplayStateChanged?.Invoke(MultiplayControllerState.MatchCanceled, data.message);
    }

    #endregion

    #region Rematch (Server -> Client)

    private void RematchRequested(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();

        onMultiplayStateChanged?.Invoke(MultiplayControllerState.RematchRequested, data.message);
    }

    private void RematchRequestSent(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();

        onMultiplayStateChanged?.Invoke(MultiplayControllerState.RematchRequestSent, data.message);
    }

    private void RematchRejected(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();

        onMultiplayStateChanged?.Invoke(MultiplayControllerState.RematchRejected, data.message);
    }


    private void RematchCanceled(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();

        onMultiplayStateChanged?.Invoke(MultiplayControllerState.RematchCanceled, data.message);
    }

    private void RematchStarted(SocketIOResponse response)
    {
        var data = response.GetValue<MatchData>();

        amIFirstPlayer = data.isPlayer1First;
        Debug.Log($"내가 선공인가?: {amIFirstPlayer}");
        MultiplayManager.Instance?.SetOpponentData(data);

        onMultiplayStateChanged?.Invoke(MultiplayControllerState.RematchStarted, data.roomId);
    }

    #endregion

    #region Error (Server -> Client)

    private void MatchError(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();

        onMultiplayStateChanged?.Invoke(MultiplayControllerState.Error, data.message);
    }

    private void AuthRequired(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();

        onMultiplayStateChanged?.Invoke(MultiplayControllerState.Error, data.message);
    }

    private void ServerError(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();

        onMultiplayStateChanged?.Invoke(MultiplayControllerState.Error, data.message);
    }

    private void UserNotFound(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();

        onMultiplayStateChanged?.Invoke(MultiplayControllerState.Error, data.message);
    }

    private void RematchError(SocketIOResponse response)
    {
        var data = response.GetValue<MessageData>();

        onMultiplayStateChanged?.Invoke(MultiplayControllerState.Error, data.message);
    }

    #endregion

    #region Client -> Server

    public void RequestMatch()
    {
        if (!isConnected)
        {
            Debug.LogError("소켓이 연결되지 않았습니다.");
            Debug.LogError("네트워크 연결을 확인해주세요.");
            return;
        }

        if (CurrentUserInfo == null)
        {
            Debug.LogError("사용자 정보가 로드되지 않았습니다.");
            Debug.LogError("사용자 정보를 확인해주세요.");
            return;
        }

        socket?.Emit("requestMatch");
    }

    public void CancelMatch()
    {
        socket?.Emit("cancelMatch");
    }

    public void Surrender()
    {
        socket?.Emit("surrender");
    }

    public void LeaveRoom()
    {
        socket?.Emit("leaveRoom");
    }

    // 게임 종료 알림
    public void NotifyGameEnded()
    {
        socket?.Emit("gameEnded");
    }

    public void DoPlayer(string roomId, int blockIdx_x, int blockIdx_y)
    {
        socket?.Emit("doPlayer", new
        {
            roomId, blockIdx_x, blockIdx_y
        });
    }

    public void ApplicationQuit()
    {
        socket?.Emit("applicationQuit");
    }

    #endregion

    #region Rematch (Client -> Server)

    // 리매치 요청
    public void RequestRematch()
    {
        socket?.Emit("requestRematch");
        Debug.Log("리매치 요청 전송");
    }

    // 리매치 수락
    public void AcceptRematch()
    {
        socket?.Emit("acceptRematch");
        Debug.Log("리매치 수락");
    }

    // 리매치 거절
    public void RejectRematch()
    {
        socket?.Emit("rejectRematch");
        Debug.Log("리매치 거절");
    }

    // 리매치 취소
    public void CancelRematch()
    {
        socket?.Emit("cancelRematch");
        Debug.Log("리매치 취소");
    }

    #endregion

    public void Dispose()
    {
        try
        {
            isConnected = false;

            // 소켓이 연결되어 있다면 안전하게 해제
            if (socket != null)
            {
                Debug.Log("소켓 연결 해제 시작");

                // 이벤트 핸들러 제거
                socket.OnConnected -= OnConnected;
                socket.OnDisconnected -= OnDisconnected;

                // 소켓 해제
                if (socket.Connected)
                {
                    socket.Disconnect();
                }

                socket.Dispose();
                socket = null;

                Debug.Log("소켓 연결 해제 완료");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"소켓 해제 중 예외 발생: {e.Message}");
        }
    }
}