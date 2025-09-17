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
    public MultiplayController multiplayController { get; private set; }
    private string roomId;

    private void Awake()
    {
        messageTest = FindFirstObjectByType<MessageTest>();

        multiplayController = new MultiplayController((state, response) =>
        {
            switch (state)
            {
                case MultiplayControllerState.MatchWaiting:
                    messageTest?.ClearAllMessage();
                    messageTest?.SetMessage(1, "매칭을 시작합니다.", Color.cyan);
                    Debug.Log("<color=cyan>매칭을 시작합니다.</color>");
                    break;
                case MultiplayControllerState.MatchFound:
                    messageTest?.ClearAllMessage();
                    messageTest?.SetMessage(1, "매칭 성공! " + response, Color.cyan);
                    this.roomId = response;
                    Debug.Log("<color=cyan>매칭 성공!</color>");
                    break;
                case MultiplayControllerState.MatchCanceled:
                    messageTest?.ClearAllMessage();
                    messageTest?.SetMessage(1, "매칭을 취소합니다.", Color.cyan);
                    Debug.Log("<color=cyan>매칭을 취소합니다.</color>");
                    break;
                case MultiplayControllerState.ExitRoom:
                    messageTest?.ClearAllMessage();
                    messageTest?.SetMessage(1, "방에서 나갑니다.", Color.magenta);
                    Debug.Log("<color=magenta>방에서 나갑니다.</color>");
                    break;
                case MultiplayControllerState.OpponentLeft:
                    messageTest?.ClearAllMessage();
                    messageTest?.SetMessage(1, "상대방이 나갔습니다.", Color.cyan);
                    Debug.Log("<color=cyan>상대방이 나갔습니다.</color>");
                    break;
                case MultiplayControllerState.Error:
                    messageTest?.ClearAllMessage();
                    messageTest?.SetMessage(1, "에러! " + response, Color.red);
                    Debug.Log($"<color=red>에러! {response}</color>");
                    break;
            }
        });

        startMatching.onClick.AddListener(() =>
        {
            // if (multiplayController != null)
            // {
            //     messageTest.ClearAllMessage();
            //     messageTest.SetMessage(1, "이미 매칭중입니다.", Color.red);
            //     Debug.LogError("<color=red>이미 매칭중입니다.</color>");
            //     return;
            // }

            messageTest.SetMessage(1, "매칭 시작", Color.yellow);
            Debug.Log("<color=yellow>매칭 시작</color>");

            multiplayController?.RequestMatch();
        });

        cancel.onClick.AddListener(() =>
        {
            // if (multiplayController != null)
            // {
            //     messageTest.ClearAllMessage();
            //     messageTest.SetMessage(1, "이미 매칭중입니다.", Color.red);
            //     Debug.LogError("<color=red>이미 매칭중입니다.</color>");
            //     return;
            // }

            multiplayController?.CancelMatch();

            messageTest?.ClearAllMessage();
            messageTest?.SetMessage(1, "매칭 취소", Color.orange);
            Debug.Log("<color=blue>매칭 취소</color>");
        });

        exitRoom.onClick.AddListener(() =>
        {
            if (multiplayController == null)
            {
                messageTest?.ClearAllMessage();
                messageTest?.SetMessage(1, "방에 입장한 상태가 아닙니다.", Color.red);
                Debug.LogError("<color=red>방에 입장한 상태가 아닙니다.</color>");
                return;
            }

            multiplayController?.LeaveRoom();
            multiplayController?.Dispose();
        });

        goStone.onClick.AddListener(() =>
        {
            if (x.text == "" || y.text == "")
            {
                messageTest?.ClearAllMessage();
                messageTest?.SetMessage(1, "착수 실패 : 좌표가 비어있습니다.", Color.red);
                Debug.LogError("<color=red>착수 실패 : 좌표가 비어있습니다.</color>");
                return;
            }

            multiplayController?.DoPlayer(roomId, int.Parse(x.text), int.Parse(y.text));
        });

        multiplayController.onBlockDataChanged += (x, y) =>
        {
            messageTest?.ClearAllMessage();
            messageTest?.SetMessage(2, "x : " + x + ", y : " + y, Color.yellow);
            Debug.Log("<color=yellow>x : " + x + ", y : " + y + "</color>");
        };
    }

    private void OnApplicationQuit()
    {
        multiplayController?.LeaveRoom();
        multiplayController?.Dispose();
    }
}