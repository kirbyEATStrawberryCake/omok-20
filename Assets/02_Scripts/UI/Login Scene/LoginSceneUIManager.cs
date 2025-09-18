using System;
using UnityEngine;
using UnityEngine.SceneManagement;

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

[RequireComponent(typeof(AuthManager))]
public class LoginSceneUIManager : Singleton<LoginSceneUIManager>
{
    [SerializeField] [Tooltip("로그인 화면 패널")]
    private GameObject LoginPanel;

    [SerializeField] [Tooltip("회원가입 화면 패널")]
    private GameObject SignUpPanel;

    [SerializeField] [Tooltip("로그인 씬에서 사용할 버튼 1개짜리 팝업")]
    private GameObject popupPanel;

    private OneButtonPanel popup;
    public AuthManager authManager { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        authManager = GetComponent<AuthManager>();
        popup = popupPanel.GetComponent<OneButtonPanel>();
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        // 로그인 씬 호출 시 로그인 화면이 보이게 설정
        OpenLoginPanel();
    }

    /// <summary>
    /// 로그인 화면을 띄우는 메소드
    /// </summary>
    public void OpenLoginPanel()
    {
        LoginPanel.SetActive(true);
        SignUpPanel.SetActive(false);
    }

    /// <summary>
    /// 회원가입 화면을 띄우는 메소드
    /// </summary>
    public void OpenSignUpPanel()
    {
        LoginPanel.SetActive(false);
        SignUpPanel.SetActive(true);
    }

    /// <summary>
    /// 로그인 화면에서 사용할 팝업창 메소드
    /// </summary>
    /// <param name="type">팝업 유형</param>
    /// <param name="onConfirm">버튼 클릭시 수행할 액션</param>
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

        popup.OpenWithSetMessageAndButtonEvent(message, onConfirm);
    }

    /// <summary>
    /// 회원가입 화면에서 사용할 팝업창 메소드
    /// </summary>
    /// <param name="type">팝업 유형</param>
    /// <param name="onConfirm">버튼 클릭시 수행할 액션</param>
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

        popup.OpenWithSetMessageAndButtonEvent(message, onConfirm);
    }
}