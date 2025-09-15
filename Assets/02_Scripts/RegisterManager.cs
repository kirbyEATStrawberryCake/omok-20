using UnityEngine;
using UnityEngine.UI;

public class RegisterManager : MonoBehaviour
{
    public InputField emailInput;
    public InputField passwordInput;
    public InputField confirmPasswordInput;
    public GameObject ErrorEmailPanel;
    public GameObject ErrorPasswordPanel;

    // 이미 등록된 이메일을 확인할 용도의 임시 데이터
    private string[] registeredEmails = { "test@test", "test2@test" };

    void Start()
    {
        // 초기화, 에러 메시지 텍스트 비활성화
        ErrorEmailPanel.SetActive(false);
        ErrorPasswordPanel.SetActive(false);
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

        // 모든 입력이 올바르면 회원가입 성공
        Debug.Log("회원가입 성공!");
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
}

