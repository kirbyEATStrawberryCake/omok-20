using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneType
{
    Login,
    Main,
    Game
}

/// <summary>
/// 씬 전환을 관리하는 싱글턴 컨트롤러
/// </summary>
public class SceneController : Singleton<SceneController>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
    }

    public static void LoadScene(SceneType sceneType)
    {
        SceneManager.LoadScene((int)sceneType);
    }
}