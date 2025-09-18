using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectGamePlayModeUI : MonoBehaviour
{
    private MainSceneUIManager mainSceneUIManager => MainSceneUIManager.Instance;

    public void OnClickMultiplayMode()
    {
        SceneManager.LoadScene("Game_Scene");
    }
}