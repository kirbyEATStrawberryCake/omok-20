using System;
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
        
        points.onClick.AddListener(()=>
        {
            pointsManager.GetPoints((response) =>
                {
                    var nickname = response.identity.nickname;
                    var username = response.identity.username;
                    var userPoints = response.points.ToString();
                    
                    messageTest.ClearMessage(1);
                    messageTest.SetMessage(2, $"### 포인트 정보 가져오기 성공 ! ### \n {userPoints}", Color.green);
                    Debug.Log($"<color=green>### 포인트 정보 가져오기 성공 ! ### \n {userPoints}</color>");
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
        
        grade.onClick.AddListener(()=>
        {
            pointsManager.GetGrade((response) =>
                {
                    var nickname = response.identity.nickname;
                    var username = response.identity.username;
                    var userGrade = response.grade.ToString();
                    
                    messageTest.ClearMessage(1);
                    messageTest.SetMessage(2, $"### 등급 정보 가져오기 성공 ! ### \n {userGrade}", Color.green);
                    Debug.Log($"<color=green>### 등급 정보 가져오기 성공 ! ### \n {userGrade}</color>");
                    
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
    }
}