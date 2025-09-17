using UnityEngine;
using UnityEngine.UI;

public class RegisterManager : MonoBehaviour
{
    public InputField emailInput;
    public InputField passwordInput;
    public InputField confirmPasswordInput;
    public GameObject ErrorEmailPanel;
    public GameObject ErrorPasswordPanel;

    // �̹� ��ϵ� �̸����� Ȯ���� �뵵�� �ӽ� ������
    private string[] registeredEmails = { "test@test", "test2@test" };

    void Start()
    {
        // �ʱ�ȭ, ���� �޽��� �ؽ�Ʈ ��Ȱ��ȭ
        ErrorEmailPanel.SetActive(false);
        ErrorPasswordPanel.SetActive(false);
    }

    // ȸ������ ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    public void OnRegisterButton()
    {
        // ���� �޽��� �ʱ�ȭ
        ErrorEmailPanel.SetActive(false);
        ErrorPasswordPanel.SetActive(false);

        string email = emailInput.text;
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        // �̸��� �ߺ� üũ
        if (IsEmailRegistered(email))
        {
            ErrorEmailPanel.SetActive(true);
            return;
        }

        // �н����� ��ġ üũ
        if (password != confirmPassword)
        {
            ErrorPasswordPanel.SetActive(true);
            return;
        }

        // ��� �Է��� �ùٸ��� ȸ������ ����
        Debug.Log("ȸ������ ����!");
    }

    // �̸����� ��ϵǾ� �ִ��� Ȯ���ϴ� �Լ�
    private bool IsEmailRegistered(string email)
    {
        foreach (string registeredEmail in registeredEmails)
        {
            if (registeredEmail == email)
            {
                return true; // �̹� ��ϵ� �̸����� ���� ���
            }
        }
        return false; // ��ϵ� �̸����� ������ false ��ȯ
    }

    // ���� �޽��� �ݱ�
    public void CloseError()
    {
        ErrorEmailPanel.SetActive(false);
        ErrorPasswordPanel.SetActive(false);
    }
}

