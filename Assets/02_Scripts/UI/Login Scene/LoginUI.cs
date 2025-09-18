using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginUI : MonoBehaviour
{
    private LoginSceneUIManager loginSceneUIManager => LoginSceneUIManager.Instance;

    [SerializeField][Tooltip("ID(이메일) InputField")] private TMP_InputField usernameInput;
    [SerializeField][Tooltip("비밀번호 InputField")] private TMP_InputField passwordInput;

    private void OnEnable()
    {
        // InputField 초기화
        usernameInput.text = "";
        passwordInput.text = "";
    }

    public void OnClickLoginButton()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        // InputField 공백 체크
        if (string.IsNullOrEmpty(username))
        {
            loginSceneUIManager.OpenLoginPopup(LoginPanelType.Empty_Username);
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            loginSceneUIManager.OpenLoginPopup(LoginPanelType.Empty_Password);
            return;
        }

        // 서버에 로그인 요청
        loginSceneUIManager.authManager.SignIn(username, password, () =>
        {
            Debug.Log("<color=green>로그인 성공!</color>");
            // SceneManager.LoadScene("Main_Scene");
        }, (errorType) =>
        {
            switch (errorType)
            {
                case AuthResponseType.INVALID_USERNAME:
                    loginSceneUIManager.OpenLoginPopup(LoginPanelType.Invalid_Username, () => usernameInput.text = "");
                    break;
                case AuthResponseType.INVALID_PASSWORD:
                    loginSceneUIManager.OpenLoginPopup(LoginPanelType.Invalid_Password, () => passwordInput.text = "");
                    break;
            }
        });
    }
}