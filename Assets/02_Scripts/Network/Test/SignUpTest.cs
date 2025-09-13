using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignUpTest : MonoBehaviour
{
    [SerializeField] private TMP_InputField _username;
    [SerializeField] private TMP_InputField _password;
    [SerializeField] private TMP_InputField _confirmPassword;
    [SerializeField] private TMP_InputField _nickname;
    [SerializeField] private Button confirm;
    [SerializeField] private Button cancel;

    private MessageTest messageTest;
    private AuthManager authManager;

    private void Awake()
    {
        authManager = FindFirstObjectByType<AuthManager>();
        messageTest = FindFirstObjectByType<MessageTest>();

        confirm.onClick.AddListener(() =>
        {
            string username = _username.text;
            string password = _password.text;
            string confirmPassword = _confirmPassword.text;
            string nickname = _nickname.text;

            if (string.IsNullOrEmpty(username))
            {
                messageTest.SetMessage(1, "아이디를 입력해주세요.", Color.yellow);
                Debug.LogWarning("아이디를 입력해주세요.");
                return;
            }

            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                messageTest.SetMessage(1, "비밀번호를 입력해주세요.", Color.yellow);
                Debug.LogWarning("비밀번호를 입력해주세요.");
                return;
            }

            if (string.IsNullOrEmpty(nickname))
            {
                messageTest.SetMessage(1, "닉네임을 입력해주세요.", Color.yellow);
                Debug.LogWarning("닉네임을 입력해주세요.");
                return;
            }


            if (password != confirmPassword)
            {
                messageTest.SetMessage(1, "비밀번호가 일치하지 않습니다.", Color.red);
                Debug.LogWarning("비밀번호가 일치하지 않습니다.");
                _password.text = "";
                _confirmPassword.text = "";

                return;
            }

            authManager.SignUp(username, password, nickname,
                () =>
                {
                    CleanInputFields();
                    messageTest.ClearAllMessage();
                    messageTest.SetMessage(2, "### 회원가입 성공 ! ###", Color.green);
                    Debug.Log("<color=green>### 회원가입 성공 ! ###</color>");
                },
                (errorType) =>
                {
                    switch (errorType)
                    {
                        case AuthResponseType.DUPLICATED_USERNAME:
                            messageTest.SetMessage(1, "중복된 아이디 입니다.", Color.red);
                            Debug.LogWarning("중복된 아이디 입니다.");
                            _username.text = "";
                            break;
                    }
                });
        });
        cancel.onClick.AddListener(CleanInputFields);
    }

    private void CleanInputFields()
    {
        _username.text = "";
        _password.text = "";
        _confirmPassword.text = "";
        _nickname.text = "";

        Debug.Log("회원가입 초기화");
        messageTest.SetMessage(1, "회원가입 초기화", Color.cornflowerBlue);
    }
}