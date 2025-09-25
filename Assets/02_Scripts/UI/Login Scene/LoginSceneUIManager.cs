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

public class LoginSceneUIManager : Singleton<LoginSceneUIManager>
{
    [SerializeField] [Tooltip("로그인 화면 패널")] private GameObject LoginPanel;
    [SerializeField] [Tooltip("회원가입 화면 패널")] private GameObject SignUpPanel;
    [SerializeField] [Tooltip("로그인 씬에서 사용할 버튼 1개짜리 팝업")] private GameObject popupPanel;

    private OneButtonPanel popup;

    protected override void Awake()
    {
        base.Awake();
        popup = popupPanel.GetComponent<OneButtonPanel>();
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Login_Scene") return;

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

    public void ShowPopup(string message, Action onConfirm = null)
    {
        popup.Show<OneButtonPanel>(message).OnConfirm(onConfirm);
    }
}