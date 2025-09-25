using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignUpUI : MonoBehaviour
{
    private LoginSceneUIManager loginSceneUIManager;
    private AuthManager authManager;

    [Header("Input Fields")]
    [SerializeField] [Tooltip("유저 ID(이메일) InputField")] private TMP_InputField usernameInput;
    [SerializeField] [Tooltip("패스워드 InputField")] private TMP_InputField passwordInput;
    [SerializeField] [Tooltip("패스워드 확인 InputField")] private TMP_InputField confirmPasswordInput;
    [SerializeField] [Tooltip("닉네임 InputField")] private TMP_InputField nicknameInput;

    [Header("프로필 이미지")]
    [SerializeField] [Tooltip("판다 이미지(프로필 이미지)")] private Image pandaImage;
    [SerializeField] [Tooltip("레드 판다 이미지(프로필 이미지)")] private Image redPandaImage;
    [SerializeField] [Tooltip("판다 스프라이트")] private Sprite pandaSprite;
    [SerializeField] [Tooltip("판다 스프라이트(회색, Unselected)")] private Sprite pandaGreySprite;
    [SerializeField] [Tooltip("레드 판다 스프라이트")] private Sprite redPandaSprite;
    [SerializeField] [Tooltip("레드 판다 스프라이트(회색, Unselected)")] private Sprite redPandaGreySprite;

    // private string selectedProfile = ""; // 선택된 프로필사진 ("panda" or "red_panda")
    private int selectedProfile = 0; // 선택된 프로필사진(0 : Unselected, 1 : Panda, 2 : Red Panda)

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
        confirmPasswordInput.text = "";
        nicknameInput.text = "";
        usernameInput.ActivateInputField();
        // 프로필 사진 선택 초기화
        pandaImage.sprite = pandaSprite;
        redPandaImage.sprite = redPandaSprite;
        selectedProfile = 0;
    }

    /// <summary>
    /// 프로필 선택
    /// </summary>
    public void SelectProfileImage(bool isPanda)
    {
        // selectedProfile = isPanda ? "panda" : "red_panda";
        selectedProfile = isPanda ? 1 : 2;
        pandaImage.sprite = isPanda ? pandaSprite : pandaGreySprite;
        redPandaImage.sprite = isPanda ? redPandaGreySprite : redPandaSprite;
    }

    /// <summary>
    /// 회원가입 버튼 클릭 시 호출되는 함수
    /// </summary>
    public void OnClickSignUpButton()
    {
        var validationResult = ValidateInput();
        if (validationResult == SignUpValidationResult.Success)
        {
            // 서버에 회원가입 요청
            authManager.SignUp(usernameInput.text, passwordInput.text, nicknameInput.text, selectedProfile,
                () =>
                {
                    loginSceneUIManager.ShowPopup(ValidationMessageMapper.GetMessage(validationResult),
                        () => loginSceneUIManager.OpenLoginPanel());
                },
                (errorType) =>
                {
                    string message = AuthMessageMapper.GetMessage(errorType);
                    loginSceneUIManager.ShowPopup(message, () =>
                    {
                        usernameInput.text = "";
                        usernameInput.ActivateInputField();
                    });
                });
            return;
        }

        // --- 유효성 검사 실패 시 처리 ---
        string message = ValidationMessageMapper.GetMessage(validationResult);
        Action onConfirmAction = null;

        switch (validationResult)
        {
            case SignUpValidationResult.UsernameEmpty:
                onConfirmAction = () => usernameInput.ActivateInputField();
                break;
            case SignUpValidationResult.PasswordEmpty:
                onConfirmAction = () => passwordInput.ActivateInputField();
                break;
            case SignUpValidationResult.NicknameEmpty:
                onConfirmAction = () => nicknameInput.ActivateInputField();
                break;
            case SignUpValidationResult.PasswordsDoNotMatch:
                onConfirmAction = () =>
                {
                    passwordInput.text = "";
                    confirmPasswordInput.text = "";
                    passwordInput.ActivateInputField();
                };
                break;
        }

        loginSceneUIManager.ShowPopup(message, onConfirmAction);
    }

    private SignUpValidationResult ValidateInput()
    {
        if (string.IsNullOrEmpty(usernameInput.text)) return SignUpValidationResult.UsernameEmpty;
        if (string.IsNullOrEmpty(passwordInput.text)) return SignUpValidationResult.PasswordEmpty;
        if (string.IsNullOrEmpty(nicknameInput.text)) return SignUpValidationResult.NicknameEmpty;
        if (passwordInput.text != confirmPasswordInput.text) return SignUpValidationResult.PasswordsDoNotMatch;
        if (selectedProfile == 0) return SignUpValidationResult.ProfileNotSelected;
        return SignUpValidationResult.Success;
    }
}