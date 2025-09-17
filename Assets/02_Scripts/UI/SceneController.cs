using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// �� ��ȯ�� �����ϴ� �̱��� ��Ʈ�ѷ�
/// </summary>
public class SceneController : Singleton<SceneController>
{
    /// <summary>
    /// �� �ε� ���� ���������� ����� ��ó�� ���
    /// </summary>
    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[SceneController] {scene.name} �ε� �Ϸ�");
    }

    public void LoadScene(string sceneName)
    {
        Debug.Log($"[SceneController] LoadScene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}
