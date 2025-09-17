using System;
using UnityEngine;

public enum LoginPanelType
{
    Success,
    Empty_Username,
    Empty_Password,
    Invalid_Username,
    Invalid_Password
}

public enum SignUpPanelType
{
    Success,
    Empty_Username,
    Empty_Password,
    Empty_Nickname,
    Fail_Username,
    Fail_Password_NotMatch,
    Fail_NotSelectedProfileImage
}

public class LoginSceneUIManager : MonoBehaviour
{
    [SerializeField] private GameObject LoginUIPanel;
    [SerializeField] private GameObject SignUpPanel;
    [SerializeField] private GameObject popupPanelPrefab;
    private OneButtonPanel popupPanel;

    [HideInInspector] public AuthManager authManager;

    private void Awake()
    {
        authManager = GetComponent<AuthManager>();
        popupPanel = popupPanelPrefab.GetComponent<OneButtonPanel>();
    }

    public void OpenLoginPanel()
    {
        LoginUIPanel.SetActive(true);
        SignUpPanel.SetActive(false);
    }

    public void OpenSignUpPanel()
    {
        LoginUIPanel.SetActive(false);
        SignUpPanel.SetActive(true);
    }

    public void OpenLoginPopup(LoginPanelType type, Action onConfirm = null)
    {
        string message = "";
        switch (type)
        {
            case LoginPanelType.Empty_Username:
                message = "이메일을 입력해주세요.";
                break;
            case LoginPanelType.Empty_Password:
                message = "비밀번호를 입력해주세요.";
                break;
            case LoginPanelType.Invalid_Username:
                message = "존재하지 않는 이메일입니다.";
                break;
            case LoginPanelType.Invalid_Password:
                message = "비밀번호가 일치하지 않습니다.";
                break;
        }

        OpenPopupPanelInternal(message, onConfirm);
    }

    public void OpenSignUpPopup(SignUpPanelType type, Action onConfirm = null)
    {
        string message = "";
        switch (type)
        {
            case SignUpPanelType.Success:
                message = "회원가입에 성공했습니다.";
                break;
            case SignUpPanelType.Empty_Username:
                message = "이메일을 입력해주세요.";
                break;
            case SignUpPanelType.Empty_Password:
                message = "비밀번호를 입력해주세요.";
                break;
            case SignUpPanelType.Empty_Nickname:
                message = "닉네임을 입력해주세요.";
                break;
            case SignUpPanelType.Fail_Username:
                message = "이미 가입된 이메일 입니다.";
                break;
            case SignUpPanelType.Fail_Password_NotMatch:
                message = "비밀번호가 일치하지 않습니다.";
                break;
            case SignUpPanelType.Fail_NotSelectedProfileImage:
                message = "프로필 사진을 선택해주세요.";
                break;
        }

        OpenPopupPanelInternal(message, onConfirm);
    }

    private void OpenPopupPanelInternal(string message, Action onConfirm = null)
    {
        popupPanel.SetMessage(message);
        popupPanel.SetButtonEvent(() =>
        {
            onConfirm?.Invoke();
            popupPanelPrefab.SetActive(false);
        });
        popupPanelPrefab.SetActive(true);
    }
}