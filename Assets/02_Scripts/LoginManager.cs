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
    
    // �α����ϱ�?
    public void Start()
    {
  
    }

    // �α��� ȭ�鿡�� ȸ������ ĵ������ �̵�
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
