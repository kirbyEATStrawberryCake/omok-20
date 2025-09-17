using System;
using Newtonsoft.Json;
using SocketIOClient;

public class RoomData
{
    [JsonProperty("roomId")] public string roomId { get; set; }
}

public class BlockData
{
    [JsonProperty("blockIdx")] public int blockIdx { get; set; }
}

public class MultiplayController : IDisposable
{
    private SocketIOUnity socket;

    // Room 상태 변화에 따른 액션
    private Action<MultiplayControllerState, string> onMultiplayStateChanged;

    // 게임 진행 상황에서 돌의 위치를 업데이트하는 액션
    public Action<int> onBlockDataChanged;

    public MultiplayController(Action<MultiplayControllerState, string> onMultiplayStateChanged)
    {
        this.onMultiplayStateChanged = onMultiplayStateChanged;
        
        var uri = new Uri(NetworkManager.SocketServerURL);
        socket = new SocketIOUnity(uri,
            new SocketIOOptions { Transport = SocketIOClient.Transport.TransportProtocol.WebSocket });

        socket.OnUnityThread("createRoom", CreateRoom);
        socket.OnUnityThread("joinRoom", JoinRoom);
        socket.OnUnityThread("exitRoom", ExitRoom);

        socket.OnUnityThread("opponentJoined", OpponentJoined);
        socket.OnUnityThread("opponentLeft", OpponentLeft);

        socket.OnUnityThread("doOpponent", DoOpponent);

        socket.Connect();
    }

    #region Server -> Client

    private void CreateRoom(SocketIOResponse response)
    {
        var data = response.GetValue<RoomData>();

        onMultiplayStateChanged?.Invoke(MultiplayControllerState.CreateRoom, data.roomId);
    }

    private void JoinRoom(SocketIOResponse response)
    {
        var data = response.GetValue<RoomData>();

        onMultiplayStateChanged?.Invoke(MultiplayControllerState.JoinRoom, data.roomId);
    }

    private void ExitRoom(SocketIOResponse response)
    {
        var data = response.GetValue<RoomData>();

        onMultiplayStateChanged?.Invoke(MultiplayControllerState.ExitRoom, data.roomId);
    }

    private void OpponentJoined(SocketIOResponse response)
    {
        onMultiplayStateChanged?.Invoke(MultiplayControllerState.OpponentJoined, null);
    }

    private void OpponentLeft(SocketIOResponse response)
    {
        onMultiplayStateChanged?.Invoke(MultiplayControllerState.OpponentLeft, null);
    }

    private void DoOpponent(SocketIOResponse response)
    {
        var data = response.GetValue<BlockData>();
        onBlockDataChanged?.Invoke(data.blockIdx);
    }

    #endregion

    #region Client -> Server

    public void LeaveRoom()
    {
        socket?.Emit("leaveRoom");
    }

    public void DoPlayer(string roomId, int position)
    {
        socket?.Emit("doPlayer", roomId, position);
    }

    #endregion

    public void Dispose()
    {
        socket?.Disconnect();
        socket?.Dispose();
        socket = null;
    }
}