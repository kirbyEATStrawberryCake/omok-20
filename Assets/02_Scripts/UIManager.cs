using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public GameObject loginCanvas;
    public GameObject registerCanvas;
    public GameObject errorLoginPanel;

    public InputField usernameInput;
    public InputField passwordInput;

    private string validUsername = "test@test";
    private string validPassword = "1234";

    public void OpenRegister()
    {
        loginCanvas.SetActive(false);
        registerCanvas.SetActive(true);
    }

    public void BackToLogin()
    {
        registerCanvas.SetActive(false);
        loginCanvas.SetActive(true);
    }

    public void TryLogin()
    {
        string inputUser = usernameInput.text;
        string inputPass = passwordInput.text;

        if (inputUser == validUsername && inputPass == validPassword)
        {
            Debug.Log("로그인 성공");
            // 다음 화면으로 이동하는 코드 등 추가
        }
        else
        {
            ShowError("계정정보가 유효하지 않습니다."); // 에러 메시지 표시
        }
    }

    public void ShowError(string message)
    {
        errorLoginPanel.SetActive(true);

    }

    public void CloseError()
    {
        errorLoginPanel.SetActive(false);
    }
}
