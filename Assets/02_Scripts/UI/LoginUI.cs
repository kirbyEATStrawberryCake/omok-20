using UnityEngine;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    public AuthManager authManager;

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

        string storedToken = PlayerPrefs.GetString("auth_token", "");

        if (inputUser == validUsername && inputPass == validPassword)
        {
            Debug.Log("�α��� ����!");

            PlayerPrefs.SetString("login_token", "�α���_" + inputUser);
            PlayerPrefs.Save();

            // ���� ȭ������ �̵��ϴ� �ڵ�
            // SceneManager.LoadScene("GameScene");
        }
        else
        {
            ShowError("ȸ�������� �����ϴ�."); // ���� �޽��� ǥ��
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