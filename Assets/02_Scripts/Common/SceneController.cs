using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneType
{
    Login,
    Main,
    Game
}

/// <summary>
/// �� ��ȯ�� �����ϴ� �̱��� ��Ʈ�ѷ�
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