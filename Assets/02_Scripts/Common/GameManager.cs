using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public string username { get; private set; }
    public string nickname { get; private set; }
    public int grade { get; private set; }
    public int profileImage { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
    }

    public void SetUserInfo(string username, string nickname, int grade, int profileImage)
    {
        this.username = username;
        this.nickname = nickname;
        this.grade = grade;
        this.profileImage = profileImage;
    }
}