using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginUI : MonoBehaviour
{
    private LoginSceneUIManager loginSceneUIManager;

    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;

    private void Awake()
    {
        loginSceneUIManager = GetComponent<LoginSceneUIManager>();
    }

    public void OnClickLogin()
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

        loginSceneUIManager.authManager.SignIn(username, password, () =>
        {
            Debug.Log("<color=green>로그인 성공!</color>");
            SceneManager.LoadScene("Main_Scene");
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