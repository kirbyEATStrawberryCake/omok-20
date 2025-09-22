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
    [Header("Scene Load Settings")]
    [SerializeField] private bool enableSceneLoadLogs = true;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (enableSceneLoadLogs)
        {
            Debug.Log($"<color=green>[SceneController] 씬 로드 완료: {scene.name}</color>");
        }
    }

    /// <summary>
    /// 씬 로드 (정적 메서드)
    /// </summary>
    /// <param name="sceneType">로드할 씬 타입</param>
    /// <param name="delay">지연 시간 (초)</param>
    public static void LoadScene(SceneType sceneType, float delay = 0f)
    {
        if (!ValidateSceneType(sceneType))
        {
            Debug.LogError($"[SceneController] 잘못된 씬 타입: {sceneType}");
            return;
        }

        if (delay > 0)
        {
            Instance.StartCoroutine(Instance.LoadSceneWithDelay(sceneType, delay));
        }
        else
        {
            Instance.LoadSceneImmediate(sceneType);
        }
    }


    /// <summary>
    /// 씬 타입 유효성 검사
    /// </summary>
    private static bool ValidateSceneType(SceneType sceneType)
    {
        return System.Enum.IsDefined(typeof(SceneType), sceneType);
    }

    /// <summary>
    /// 즉시 씬 로드
    /// </summary>
    private void LoadSceneImmediate(SceneType sceneType)
    {
        if (enableSceneLoadLogs)
        {
            Debug.Log($"<color=yellow>[SceneController] 씬 로드 시작: {sceneType}</color>");
        }

        SceneManager.LoadScene((int)sceneType);
    }

    /// <summary>
    /// 지연 후 씬 로드
    /// </summary>
    private IEnumerator LoadSceneWithDelay(SceneType sceneType, float delay)
    {
        if (enableSceneLoadLogs)
        {
            Debug.Log($"<color=yellow>[SceneController] 지연 씬 로드 시작: {sceneType} (지연: {delay}초)</color>");
        }

        // 딜레이 대기
        yield return new WaitForSeconds(delay);

        // 한 프레임 더 대기 (OnDestroy 완료 보장)
        yield return null;
        
        LoadSceneImmediate(sceneType);
    }
}
