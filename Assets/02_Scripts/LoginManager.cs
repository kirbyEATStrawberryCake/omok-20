using UnityEngine;
using UnityEngine.UI;

public class LoginManger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject loginCanvas;
    public GameObject registerCanvas;
    
    public InputField id;
    public InputField password;

    public Text notify;
    
    // 로그인하기?
    public void Start()
    {
  
    }

    // 로그인 화면에서 회원가입 캔버스로 이동
    public void OpenRegistr()
    {
        loginCanvas.SetActive(true);
        registerCanvas.SetActive(true);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
