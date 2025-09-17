using System;
using Newtonsoft.Json;
using SocketIOClient;
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
public class MatchFoundData
{
    [JsonProperty("roomId")] public string roomId;
    [JsonProperty("userId")] public string userId;
    [JsonProperty("username")] public string username;
    [JsonProperty("nickname")] public string nickname;
    [JsonProperty("grade")] public int grade;
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
    private bool isConnected = false;

    // 사용자 정보
    public UserInfo CurrentUserInfo { get; private set; }

    // Room 상태 변화에 따른 액션
    private Action<MultiplayControllerState, string> onMultiplayStateChanged;

    // 게임 진행 상황에서 돌의 위치를 업데이트하는 액션
    public Action<int, int> onBlockDataChanged;

    public MultiplayController(Action<MultiplayControllerState, string> onMultiplayStateChanged)
    {
        this.onMultiplayStateChanged = onMultiplayStateChanged;

        var uri = new Uri(NetworkManager.SocketServerURL);
        socket = new SocketIOUnity(uri,
            new SocketIOOptions { Transport = SocketIOClient.Transport.TransportProtocol.WebSocket });

        // 연결 상태 이벤트
        socket.OnConnected += (sender, e) =>
        {
            isConnected = true;
            Debug.Log("소켓 연결됨");
        };

        socket.OnDisconnected += (sender, e) =>
        {
            isConnected = false;
            Debug.Log("소켓 연결 해제됨");
        };

        socket.OnUnityThread("userInfoLoaded", UserInfoLoaded);
        socket.OnUnityThread("exitRoom", ExitRoom);
        socket.OnUnityThread("opponentLeft", OpponentLeft);

        socket.OnUnityThread("doOpponent", DoOpponent);
        
        socket.OnUnityThread("matchWaiting", MatchWaiting);
        socket.OnUnityThread("matchExpanded", MatchExpanded);
        socket.OnUnityThread("matchFound", MatchFound);
        socket.OnUnityThread("matchFailed", MatchFailed);
        socket.OnUnityThread("matchCanceled", MatchCanceled);

        socket.OnUnityThread("matchError", MatchError);
        socket.OnUnityThread("authRequired", AuthRequired);
        socket.OnUnityThread("serverError", ServerError);
        socket.OnUnityThread("userNotFound", UserNotFound);
    }

    public void Connect(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            Debug.LogError("로그인 정보가 없습니다.");
            return;
        }

        socket?.Connect();

        // 연결 후 사용자 정보 전송
        socket.OnConnected += (sender, e) => { socket.Emit("authenticate", username); };
    }

    #region Server -> Client

    private void UserInfoLoaded(SocketIOResponse response)
    {
        CurrentUserInfo = response.GetValue<UserInfo>();
    }

    private void ExitRoom(SocketIOResponse response)
    {
        var data = response.GetValue<RoomData>();

        onMultiplayStateChanged?.Invoke(MultiplayControllerState.ExitRoom, data.roomId);
    }

    private void OpponentLeft(SocketIOResponse response)
    {
        onMultiplayStateChanged?.Invoke(MultiplayControllerState.OpponentLeft, null);
    }

    private void DoOpponent(SocketIOResponse response)
    {
        var data = response.GetValue<BlockData>();
        onBlockDataChanged?.Invoke(data.blockIdx_x, data.blockIdx_y);
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
        var data = response.GetValue<MatchFoundData>();

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

    public void LeaveRoom()
    {
        socket?.Emit("leaveRoom");
    }

    public void DoPlayer(string roomId, int blockIdx_x, int blockIdx_y)
    {
        socket?.Emit("doPlayer", new
        {
            roomId, blockIdx_x, blockIdx_y
        });
    }

    #endregion

    public void Dispose()
    {
        isConnected = false;
        socket?.Disconnect();
        socket?.Dispose();
        socket = null;
    }
}