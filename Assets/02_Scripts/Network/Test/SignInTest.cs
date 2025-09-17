using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignInTest : MonoBehaviour
{
    [SerializeField] private TMP_InputField _username;
    [SerializeField] private TMP_InputField _password;
    [SerializeField] private Button login;
    [SerializeField] private Button logout;
    [SerializeField] private Button cancel;

    private MessageTest messageTest;
    private AuthManager authManager;
    private MultiplayMenuTest multiplayMenuTest;

    private void Awake()
    {
        authManager = FindFirstObjectByType<AuthManager>();
        messageTest = FindFirstObjectByType<MessageTest>();
        multiplayMenuTest = FindFirstObjectByType<MultiplayMenuTest>();

        login.onClick.AddListener(() =>
        {
            string username = _username.text;
            string password = _password.text;

            if (string.IsNullOrEmpty(username))
            {
                messageTest.SetMessage(1, "아이디를 입력해주세요.", Color.yellow);
                Debug.LogWarning("아이디를 입력해주세요.");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                messageTest.SetMessage(1, "비밀번호를 입력해주세요.", Color.yellow);
                Debug.LogWarning("비밀번호를 입력해주세요.");
                return;
            }

            authManager.SignIn(username, password, () =>
                {
                    multiplayMenuTest.multiplayController.Connect(username);
                    CleanInputFields();
                    messageTest.ClearAllMessage();
                    messageTest.SetMessage(2, "### 로그인 성공 ! ###", Color.green);
                    Debug.Log("<color=green>### 로그인 성공 ! ###</color>");
                },
                (errorType) =>
                {
                    switch (errorType)
                    {
                        case AuthResponseType.INVALID_USERNAME:
                            messageTest.SetMessage(1, "로그인 실패 : \n존재하지 않는 아이디 입니다.", Color.red);
                            Debug.LogWarning("로그인 실패 : 존재하지 않는 아이디 입니다.");
                            _username.text = "";
                            break;
                        case AuthResponseType.INVALID_PASSWORD:
                            messageTest.SetMessage(1, "로그인 실패 : \n비밀번호가 일치하지 않습니다.", Color.red);
                            Debug.LogWarning("로그인 실패 : 비밀번호가 일치하지 않습니다.");
                            _password.text = "";
                            break;
                    }
                });
        });

        logout.onClick.AddListener(() => authManager.SignOut(() =>
            {
                CleanInputFields();
                messageTest.ClearAllMessage();
                messageTest.SetMessage(2, "### 로그아웃 성공 ! ###", Color.coral);
                Debug.Log("<color=green>### 로그아웃 성공 ! ###</color>");
            },
            (errorType) =>
            {
                switch (errorType)
                {
                    case AuthResponseType.NOT_LOGGED_IN:
                        messageTest.SetMessage(1, "로그아웃 실패 : \n로그인 상태가 아닙니다.", Color.red);
                        Debug.LogWarning("로그아웃 실패 : 로그인 상태가 아닙니다.");
                        break;
                }
            }));

        cancel.onClick.AddListener(CleanInputFields);
    }

    private void CleanInputFields()
    {
        _username.text = "";
        _password.text = "";

        Debug.Log("로그인 초기화");
        messageTest.SetMessage(1, "로그인 초기화", Color.cornflowerBlue);
    }
}