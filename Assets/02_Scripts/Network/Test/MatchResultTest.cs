using UnityEngine;
using UnityEngine.UI;

public class MatchResultTest : MonoBehaviour
{
    [SerializeField] private Button win;
    [SerializeField] private Button lose;

    private MessageTest messageTest;
    private StatsManager statsManager;

    private void Awake()
    {
        statsManager = FindFirstObjectByType<StatsManager>();
        messageTest = FindFirstObjectByType<MessageTest>();

        win.onClick.AddListener(() =>
        {
            statsManager.UpdateGameResult(GameResult.Victory,
                () =>
                {
                    messageTest.ClearAllMessage();
                    messageTest.SetMessage(2, "### 게임 결과(승리) 등록 성공 ! ###", Color.green);
                    Debug.Log("<color=green>### 게임 결과(승리) 등록 성공 ! ###</color>");
                },
                (errorType) =>
                {
                    switch (errorType)
                    {
                        case StatsResponseType.INVALID_GAME_RESULT:
                            messageTest.SetMessage(1, "경기 결과 등록 실패 : \n게임 결과가 올바르지 않습니다.", Color.red);
                            Debug.LogError("<color=red>경기 결과 등록 실패 : 게임 결과가 올바르지 않습니다.</color>");
                            break;
                        case StatsResponseType.CANNOT_FOUND_USER:
                            messageTest.SetMessage(1, "경기 결과 등록 실패 : \n유저를 찾지 못했습니다.", Color.red);
                            Debug.LogError("<color=red>경기 결과 등록 실패 : 유저를 찾지 못했습니다.</color>");
                            break;
                        case StatsResponseType.NOT_LOGGED_IN:
                            messageTest.SetMessage(1, "경기 결과 등록 실패 : \n로그인 상태가 아닙니다.", Color.red);
                            Debug.LogError("<color=red>경기 결과 등록 실패 : 로그인 상태가 아닙니다.</color>");
                            break;
                    }
                });
        });

        lose.onClick.AddListener(() =>
        {
            statsManager.UpdateGameResult(GameResult.Defeat, () =>
                {
                    messageTest.ClearAllMessage();
                    messageTest.SetMessage(2, "### 게임 결과(패배) 등록 성공 ! ###", Color.green);
                    Debug.Log("<color=green>### 게임 결과(패배) 등록 성공 ! ###</color>");
                },
                (errorType) =>
                {
                    switch (errorType)
                    {
                        case StatsResponseType.INVALID_GAME_RESULT:
                            messageTest.SetMessage(1, "경기 결과 등록 실패 : \n게임 결과가 올바르지 않습니다.", Color.red);
                            Debug.LogError("<color=red>경기 결과 등록 실패 : 게임 결과가 올바르지 않습니다.</color>");
                            break;
                        case StatsResponseType.CANNOT_FOUND_USER:
                            messageTest.SetMessage(1, "경기 결과 등록 실패 : \n유저를 찾지 못했습니다.", Color.red);
                            Debug.LogError("<color=red>경기 결과 등록 실패 : 유저를 찾지 못했습니다.</color>");
                            break;
                        case StatsResponseType.NOT_LOGGED_IN:
                            messageTest.SetMessage(1, "경기 결과 등록 실패 : \n로그인 상태가 아닙니다.", Color.red);
                            Debug.LogError("<color=red>경기 결과 등록 실패 : 로그인 상태가 아닙니다.</color>");
                            break;
                    }
                });
        });
    }
}