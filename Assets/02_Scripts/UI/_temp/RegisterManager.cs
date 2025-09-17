using UnityEngine;
using UnityEngine.UI;

public class RegisterManager : MonoBehaviour
{
    public InputField emailInput;
    public InputField passwordInput;
    public InputField confirmPasswordInput;

    public GameObject ErrorEmailPanel;
    public GameObject ErrorPasswordPanel;
    public GameObject AddMemberPanel;

    public LoginManager LoginManager;
    
    public Image pandaImage;
    public Image redPandaImage;

    public Sprite pandaSprite;
    public Sprite pandaGraySprite;

    public Sprite redPandaSprite;
    public Sprite redPandaGraySprite;

    private string selectedProfile = ""; // ���õ� �����ʻ��� ("panda" or "red_panda")


    // �̹� ��ϵ� �̸����� Ȯ���� �뵵�� �ӽ� ������
    private string[] registeredEmails = { "test@test", "test2@test" };

    void Start()
    {
        // �ʱ�ȭ, ���� �޽��� ��Ȱ��ȭ
        ErrorEmailPanel.SetActive(false);
        ErrorPasswordPanel.SetActive(false);

    }

    //�Ǵټ���â
    public void SelectPanda()
    {
        selectedProfile = "panda";

        // �̹��� ��ü
        pandaImage.sprite = pandaSprite;
        redPandaImage.sprite = redPandaGraySprite;
    }

    public void SelectRedPanda()
    {
        selectedProfile = "red_panda";

        // �̹��� ��ü
        pandaImage.sprite = pandaGraySprite;
        redPandaImage.sprite = redPandaSprite;
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

        if (IsEmailRegistered(email))
        {
            ErrorEmailPanel.SetActive(true);
            return;
        }

        if (password != confirmPassword)
        {
            ErrorPasswordPanel.SetActive(true);
            return;
        }
        // ��� �Է��� �ùٸ��� ȸ������ ����
        Debug.Log("ȸ������ ����!");

        PlayerPrefs.SetString("auth_token", "��ū_" + email);
        PlayerPrefs.SetString("profile", selectedProfile);
        PlayerPrefs.Save();

        // ȸ�� ���� Ȯ�� â
        AddMemberPanel.SetActive(true);

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

    //ȸ�� ���� �޽��� ���� �Ŀ� �α��� ȭ������ ��ȯ
    public void OnConfirmAddMember()
    {
        AddMemberPanel.SetActive(false); 
        LoginManager.BackToLogin(); // �α��� ȭ������ ��ȯ
    }
}

