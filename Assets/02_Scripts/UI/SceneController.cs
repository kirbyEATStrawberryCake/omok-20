using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 전환을 관리하는 싱글턴 컨트롤러
/// </summary>
public class SceneController : Singleton<SceneController>
{
    /// <summary>
    /// 씬 로드 이후 전역적으로 공통된 후처리 담당
    /// </summary>
    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[SceneController] {scene.name} 로드 완료");
    }

    public void LoadScene(string sceneName)
    {
        Debug.Log($"[SceneController] LoadScene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}
