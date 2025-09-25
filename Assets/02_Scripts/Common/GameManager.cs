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
}