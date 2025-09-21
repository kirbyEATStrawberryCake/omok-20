using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignUpUI : MonoBehaviour
{
    private LoginSceneUIManager loginSceneUIManager;

    [Header("Input Fields")]
    [SerializeField] [Tooltip("유저 ID(이메일) InputField")]
    private TMP_InputField usernameInput;

    [SerializeField] [Tooltip("패스워드 InputField")]
    private TMP_InputField passwordInput;

    [SerializeField] [Tooltip("패스워드 확인 InputField")]
    private TMP_InputField confirmPasswordInput;

    [SerializeField] [Tooltip("닉네임 InputField")]
    private TMP_InputField nicknameInput;

    [Header("프로필 이미지")]
    [SerializeField] [Tooltip("판다 이미지(프로필 이미지)")]
    private Image pandaImage;

    [SerializeField] [Tooltip("레드 판다 이미지(프로필 이미지)")]
    private Image redPandaImage;

    [SerializeField] [Tooltip("판다 스프라이트")] private Sprite pandaSprite;

    [SerializeField] [Tooltip("판다 스프라이트(회색, Unselected)")]
    private Sprite pandaGreySprite;

    [SerializeField] [Tooltip("레드 판다 스프라이트")]
    private Sprite redPandaSprite;

    [SerializeField] [Tooltip("레드 판다 스프라이트(회색, Unselected)")]
    private Sprite redPandaGreySprite;

    // private string selectedProfile = ""; // 선택된 프로필사진 ("panda" or "red_panda")
    private int selectedProfile = 0; // 선택된 프로필사진(0 : Unselected, 1 : Panda, 2 : Red Panda)

    private void Start()
    {
        loginSceneUIManager = LoginSceneUIManager.Instance;
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
        string username = usernameInput.text;
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;
        string nickname = nicknameInput.text;

        // InputField 공백 체크
        if (string.IsNullOrEmpty(username))
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

        // 프로필 이미지를 선택했는지 체크
        if (selectedProfile != 1 && selectedProfile != 2)
        {
            loginSceneUIManager.OpenSignUpPopup(SignUpPanelType.Fail_NotSelectedProfileImage);
            return;
        }

        // 패스워드 일치 체크
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

        // 서버에 회원가입 요청
        loginSceneUIManager.authManager.SignUp(username, password, nickname, selectedProfile,
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
                            () => usernameInput.text = "");
                        break;
                }
            });
    }
}