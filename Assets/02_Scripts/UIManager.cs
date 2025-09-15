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
            Debug.Log("�α��� ����");
            // ���� ȭ������ �̵��ϴ� �ڵ� �� �߰�
        }
        else
        {
            ShowError("���������� ��ȿ���� �ʽ��ϴ�."); // ���� �޽��� ǥ��
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
