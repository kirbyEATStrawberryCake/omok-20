using UnityEngine;
using UnityEngine.UI;

public class ViewInfoTest : MonoBehaviour
{
    [SerializeField] private Button points;
    [SerializeField] private Button grade;
    [SerializeField] private Button record;
    [SerializeField] private Button Ranking;

    private MessageTest messageTest;
    private StatsManager statsManager;
    private PointsManager pointsManager;

    private void Awake()
    {
        pointsManager = FindFirstObjectByType<PointsManager>();
        statsManager = FindFirstObjectByType<StatsManager>();
        messageTest = FindFirstObjectByType<MessageTest>();

        points.onClick.AddListener(() =>
        {
            pointsManager.GetPoints((response) =>
                {
                    var nickname = response.identity.nickname;
                    var username = response.identity.username;
                    var userPoints = response.points.ToString();

                    messageTest.ClearAllMessage();
                    messageTest.SetMessage(3, $"{nickname}({username})", Color.black);
                    messageTest.SetMessage(2, $"### 포인트 정보 가져오기 성공 ! ### \n현재 승급 포인트 : {userPoints}", Color.green);
                    Debug.Log(
                        $"<color=green>### 포인트 정보 가져오기 성공 ! ### \n {nickname}({username})의 현재 승급 포인트 : {userPoints}</color>");
                },
                (errorType) =>
                {
                    switch (errorType)
                    {
                        case PointsResponseType.CANNOT_FOUND_USER:
                            messageTest.SetMessage(1, "포인트 정보 가져오기 실패 : \n유저를 찾지 못했습니다.", Color.red);
                            Debug.LogWarning("포인트 정보 가져오기 실패 : 유저를 찾지 못했습니다.");
                            break;
                        case PointsResponseType.NOT_LOGGED_IN:
                            messageTest.SetMessage(1, "포인트 정보 가져오기 실패 : \n로그인 상태가 아닙니다.", Color.red);
                            Debug.LogWarning("포인트 정보 가져오기 실패 : 로그인 상태가 아닙니다.");
                            break;
                    }
                });
        });

        grade.onClick.AddListener(() =>
        {
            pointsManager.GetGrade((response) =>
                {
                    var nickname = response.identity.nickname;
                    var username = response.identity.username;
                    var userGrade = response.grade.ToString();

                    messageTest.ClearAllMessage();
                    messageTest.SetMessage(3, $"{nickname}({username})", Color.black);
                    messageTest.SetMessage(2, $"### 등급 정보 가져오기 성공 ! ### \n현재 등급 : {userGrade}", Color.green);
                    Debug.Log(
                        $"<color=green>### 등급 정보 가져오기 성공 ! ### \n {nickname}({username})의 현재 등급 : {userGrade}</color>");
                },
                (errorType) =>
                {
                    switch (errorType)
                    {
                        case PointsResponseType.CANNOT_FOUND_USER:
                            messageTest.SetMessage(1, "등급 정보 가져오기 실패 : \n유저를 찾지 못했습니다.", Color.red);
                            Debug.LogWarning("등급 정보 가져오기 실패 : 유저를 찾지 못했습니다.");
                            break;
                        case PointsResponseType.NOT_LOGGED_IN:
                            messageTest.SetMessage(1, "등급 정보 가져오기 실패 : \n로그인 상태가 아닙니다.", Color.red);
                            Debug.LogWarning("등급 정보 가져오기 실패 : 로그인 상태가 아닙니다.");
                            break;
                    }
                });
        });

        record.onClick.AddListener(() =>
        {
            statsManager.GetRecord((response) =>
                {
                    var nickname = response.identity.nickname;
                    var username = response.identity.username;
                    messageTest.ClearAllMessage();
                    messageTest.SetMessage(1, "### 전적 가져오기 성공 ! ###", Color.green);
                    messageTest.SetMessage(3, $"{nickname}({username})", Color.black);
                    messageTest.SetRecord(response.record, Color.gray);
                    Debug.Log($"<color=green>### 전적 가져오기 성공 ! ###</color>");
                },
                (errorType) =>
                {
                    switch (errorType)
                    {
                        case StatsResponseType.CANNOT_FOUND_USER:
                            messageTest.SetMessage(1, "전적 정보 가져오기 실패 : \n유저를 찾지 못했습니다.", Color.red);
                            Debug.LogWarning("전적 정보 가져오기 실패 : 유저를 찾지 못했습니다.");
                            break;
                        case StatsResponseType.NOT_LOGGED_IN:
                            messageTest.SetMessage(1, "전적 정보 가져오기 실패 : \n로그인 상태가 아닙니다.", Color.red);
                            Debug.LogWarning("전적 정보 가져오기 실패 : 로그인 상태가 아닙니다.");
                            break;
                    }
                });
        });
    }
}