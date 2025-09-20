using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectGamePlayModeUI : MonoBehaviour
{
    public void OnClickSingleMode()
    {
        GameModeManager.Mode = GameMode.SinglePlayer;
        SceneController.LoadScene(SceneType.Game);
    }
    
    public void OnClickMultiplayMode()
    {
        GameModeManager.Mode = GameMode.MultiPlayer;
        SceneController.LoadScene(SceneType.Game);
    }
}