using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
public static class EditorSceneLoader
{
    // 게임이 시작될 때의 씬 이름을 저장할 static 변수
    public static string StartupSceneName { get; private set; }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnBeforeSceneLoad()
    {
        // 플레이 버튼을 누른 씬의 이름을 저장
        StartupSceneName = SceneManager.GetActiveScene().name;
    }
}
#endif