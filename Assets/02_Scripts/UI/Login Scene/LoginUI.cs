using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginUI : MonoBehaviour
{
    private LoginSceneUIManager loginSceneUIManager;
    private AuthManager authManager;

    [SerializeField] [Tooltip("ID(이메일) InputField")] private TMP_InputField usernameInput;
    [SerializeField] [Tooltip("비밀번호 InputField")] private TMP_InputField passwordInput;

    private void Start()
    {
        loginSceneUIManager = LoginSceneUIManager.Instance;
        authManager = NetworkManager.Instance.authManager;
    }

    private void OnEnable()
    {
        // InputField 초기화
        usernameInput.text = "";
        passwordInput.text = "";
        usernameInput.ActivateInputField();
    }

    /// <summary>
    /// 로그인 버튼 클릭 시 호출되는 함수
    /// </summary>
    public void OnClickLoginButton()
    {
        var validationResult = ValidateInput();
        if (validationResult == SignInValidationResult.Success)
        {
            // 서버에 로그인 요청
            authManager.SignIn(usernameInput.text, passwordInput.text,
                () =>
                {
                    Debug.Log("<color=green>로그인 성공!</color>");
                    // TODO: 사용자 정보 받아서 저장해두기
                    SceneController.LoadScene(SceneType.Main);
                }, (errorType) =>
                {
                    string message = AuthMessageMapper.GetMessage(errorType);
                    loginSceneUIManager.ShowPopup(message, () => passwordInput.text = "");
                });
            return;
        }

        // --- 유효성 검사 실패 시 처리 ---
        string message = ValidationMessageMapper.GetMessage(validationResult);
        Action onConfirmAction = null;
        switch (validationResult)
        {
            case SignInValidationResult.UsernameEmpty:
                onConfirmAction = () => usernameInput.ActivateInputField();
                break;
            case SignInValidationResult.PasswordEmpty:
                onConfirmAction = () => passwordInput.ActivateInputField();
                break;
        }

        loginSceneUIManager.ShowPopup(message, onConfirmAction);
    }

    /// <summary>
    /// 로그인 입력값에 대한 유효성을 검사하는 메서드
    /// </summary>
    private SignInValidationResult ValidateInput()
    {
        if (string.IsNullOrEmpty(usernameInput.text)) return SignInValidationResult.UsernameEmpty;
        if (string.IsNullOrEmpty(passwordInput.text)) return SignInValidationResult.PasswordEmpty;

        return SignInValidationResult.Success;
    }
}