using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneType
{
    Login,
    Main,
    Game,
    Gibo
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

    public static void LoadScene(SceneType sceneType, float delay = 0)
    {
        if (delay > 0)
            Instance.StartCoroutine(Instance.LoadSceneWithDelay(sceneType, delay));
        else
            SceneManager.LoadScene((int)sceneType);
    }

    private IEnumerator LoadSceneWithDelay(SceneType sceneType, float delay)
    {
        // 딜레이 대기
        yield return new WaitForSeconds(delay);

        // 한 프레임 더 대기 (OnDestroy 완료 보장)
        yield return null;
        SceneManager.LoadScene((int)sceneType);
    }
}