using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegisterUI : MonoBehaviour
{
    private LoginSceneUIManager loginSceneUIManager;

    [Header("Input Fields")] [SerializeField]
    private TMP_InputField emailInput;

    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField confirmPasswordInput;
    [SerializeField] private TMP_InputField nicknameInput;

    [Header("������ �̹���")] [SerializeField] private Image pandaImage;
    [SerializeField] private Image redPandaImage;
    [SerializeField] private Sprite pandaSprite;
    [SerializeField] private Sprite pandaGraySprite;
    [SerializeField] private Sprite redPandaSprite;
    [SerializeField] private Sprite redPandaGraySprite;

    // private string selectedProfile = ""; // ���õ� �����ʻ��� ("panda" or "red_panda")
    private int selectedProfile = 0; // ���õ� �����ʻ���(0 : None, 1 : Panda, 2 : Red Panda)

    private void Awake()
    {
        loginSceneUIManager = GetComponent<LoginSceneUIManager>();
    }

    /// <summary>
    /// ������ ����
    /// </summary>
    public void SelectCharacter(bool isPanda)
    {
        // selectedProfile = isPanda ? "panda" : "red_panda";
        selectedProfile = isPanda ? 1 : 2;
        pandaImage.sprite = isPanda ? pandaSprite : pandaGraySprite;
        redPandaImage.sprite = isPanda ? redPandaGraySprite : redPandaSprite;
    }

    /// <summary>
    /// ȸ������ ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    /// </summary>
    public void OnRegisterButton()
    {
        string email = emailInput.text;
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;
        string nickname = nicknameInput.text;

        // InputField ���� üũ
        if (string.IsNullOrEmpty(email))
        {
            loginSceneUIManager.OpenSignUpPopup(SignUpPanelType.Empty_Username);
            return;
        }

        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            loginSceneUIManager.OpenSignUpPopup(SignUpPanelType.Empty_Password);
            return;
        }

        if (string.IsNullOrEmpty(nickname))
        {
            loginSceneUIManager.OpenSignUpPopup(SignUpPanelType.Empty_Nickname);
            return;
        }

        // ������ �̹����� �����ߴ��� üũ
        if (selectedProfile != 1 && selectedProfile != 2)
        {
            loginSceneUIManager.OpenSignUpPopup(SignUpPanelType.Fail_NotSelectedProfileImage);
            return;
        }

        // �н����� ��ġ üũ
        if (password != confirmPassword)
        {
            loginSceneUIManager.OpenSignUpPopup(SignUpPanelType.Fail_Password_NotMatch,
                () =>
                {
                    passwordInput.text = "";
                    confirmPasswordInput.text = "";
                });
            return;
        }

        loginSceneUIManager.authManager.SignUp(email, password, nickname, selectedProfile,
            () =>
            {
                loginSceneUIManager.OpenSignUpPopup(SignUpPanelType.Success,
                    () => loginSceneUIManager.OpenLoginPanel());
            }, (errorType) =>
            {
                switch (errorType)
                {
                    case AuthResponseType.DUPLICATED_USERNAME:
                        loginSceneUIManager.OpenSignUpPopup(SignUpPanelType.Fail_Username,
                            () => emailInput.text = "");
                        break;
                }
            });

        // ��� �Է��� �ùٸ��� ȸ������ ����
        Debug.Log("ȸ������ ����!");
    }
}