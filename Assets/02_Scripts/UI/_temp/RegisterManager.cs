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

    private string selectedProfile = ""; // 선택된 프로필사진 ("panda" or "red_panda")


    // 이미 등록된 이메일을 확인할 용도의 임시 데이터
    private string[] registeredEmails = { "test@test", "test2@test" };

    void Start()
    {
        // 초기화, 에러 메시지 비활성화
        ErrorEmailPanel.SetActive(false);
        ErrorPasswordPanel.SetActive(false);

    }

    //판다선택창
    public void SelectPanda()
    {
        selectedProfile = "panda";

        // 이미지 교체
        pandaImage.sprite = pandaSprite;
        redPandaImage.sprite = redPandaGraySprite;
    }

    public void SelectRedPanda()
    {
        selectedProfile = "red_panda";

        // 이미지 교체
        pandaImage.sprite = pandaGraySprite;
        redPandaImage.sprite = redPandaSprite;
    }

    // 회원가입 버튼 클릭 시 호출되는 함수
    public void OnRegisterButton()
    {
        // 오류 메시지 초기화
        ErrorEmailPanel.SetActive(false);
        ErrorPasswordPanel.SetActive(false);

        string email = emailInput.text;
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        // 이메일 중복 체크
        if (IsEmailRegistered(email))
        {
            ErrorEmailPanel.SetActive(true);
            return;
        }

        // 패스워드 일치 체크
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
        // 모든 입력이 올바르면 회원가입 성공
        Debug.Log("회원가입 성공!");

        PlayerPrefs.SetString("auth_token", "토큰_" + email);
        PlayerPrefs.SetString("profile", selectedProfile);
        PlayerPrefs.Save();

        // 회원 가입 확인 창
        AddMemberPanel.SetActive(true);

    }

    // 이메일이 등록되어 있는지 확인하는 함수
    private bool IsEmailRegistered(string email)
    {
        foreach (string registeredEmail in registeredEmails)
        {
            if (registeredEmail == email)
            {
                return true; // 이미 등록된 이메일이 있을 경우
            }
        }
        return false; // 등록된 이메일이 없으면 false 반환
    }

    // 오류 메시지 닫기
    public void CloseError()
    {
        ErrorEmailPanel.SetActive(false);
        ErrorPasswordPanel.SetActive(false);
    }

    //회원 가입 메시지 닫은 후에 로그인 화면으로 전환
    public void OnConfirmAddMember()
    {
        AddMemberPanel.SetActive(false); 
        LoginManager.BackToLogin(); // 로그인 화면으로 전환
    }
}

