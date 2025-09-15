using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayMenuTest : MonoBehaviour
{
    [SerializeField] private Button startMatching;
    [SerializeField] private Button cancel;
    [SerializeField] private Button exitRoom;
    
    [SerializeField] private TMP_InputField x;
    [SerializeField] private TMP_InputField y;
    [SerializeField] private Button goStone;

    private MessageTest messageTest;
    private MultiplayController multiplayController;
    private string roomId;

    private void Awake()
    {
        messageTest = FindFirstObjectByType<MessageTest>();

        startMatching.onClick.AddListener(() =>
        {
            if (multiplayController != null)
            {
                messageTest.ClearAllMessage();
                messageTest.SetMessage(1, "이미 매칭중입니다.", Color.red);
                Debug.LogError("<color=red>이미 매칭중입니다.</color>");
                return;
            }

            messageTest.SetMessage(1, "매칭 시작", Color.yellow);
            Debug.Log("<color=yellow>매칭 시작</color>");

            multiplayController = new MultiplayController((state, roomId) =>
            {
                this.roomId = roomId;
                switch (state)
                {
                    case MultiplayControllerState.CreateRoom:
                        messageTest.ClearAllMessage();
                        messageTest.SetMessage(1, "매칭 성공! 방을 생성합니다.", Color.blue);
                        Debug.Log("<color=blue>매칭 성공! 방을 생성합니다.</color>");
                        break;
                    case MultiplayControllerState.JoinRoom:
                        messageTest.ClearAllMessage();
                        messageTest.SetMessage(1, "매칭 성공! 방에 입장합니다.", Color.blue);
                        Debug.Log("<color=blue>매칭 성공! 방에 입장합니다.</color>");
                        break;
                    case MultiplayControllerState.ExitRoom:
                        messageTest.ClearAllMessage();
                        messageTest.SetMessage(1, "방에서 나갑니다.", Color.magenta);
                        Debug.Log("<color=magenta>방에서 나갑니다.</color>");
                        break;
                    case MultiplayControllerState.OpponentJoined:
                        messageTest.ClearAllMessage();
                        messageTest.SetMessage(1, "상대방이 입장했습니다.", Color.cyan);
                        Debug.Log("<color=cyan>상대방이 입장했습니다.</color>");
                        break;
                    case MultiplayControllerState.OpponentLeft:
                        messageTest.ClearAllMessage();
                        messageTest.SetMessage(1, "상대방이 나갔습니다.", Color.cyan);
                        Debug.Log("<color=cyan>상대방이 나갔습니다.</color>");
                        break;
                }
            });
        });

        cancel.onClick.AddListener(() =>
        {
            if (multiplayController != null)
            {
                messageTest.ClearAllMessage();
                messageTest.SetMessage(1, "이미 매칭중입니다.", Color.red);
                Debug.LogError("<color=red>이미 매칭중입니다.</color>");
                return;
            }

            messageTest.ClearAllMessage();
            messageTest.SetMessage(1, "매칭 취소", Color.orange);
            Debug.Log("<color=blue>매칭 취소<color>");
        });

        exitRoom.onClick.AddListener(() =>
        {
            if (multiplayController == null)
            {
                messageTest.ClearAllMessage();
                messageTest.SetMessage(1, "방에 입장한 상태가 아닙니다.", Color.red);
                Debug.LogError("<color=red>방에 입장한 상태가 아닙니다.</color>");
                return;
            }

            multiplayController?.LeaveRoom();
            multiplayController?.Dispose();
        });
    }

    private void OnApplicationQuit()
    {
        multiplayController?.LeaveRoom();
        multiplayController?.Dispose();
    }
}