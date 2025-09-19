using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectGamePlayModeUI : MonoBehaviour
{
    public void OnClickSingleMode()
    {
        GameModeManager.Mode = GameMode.SinglePlayer;
        SceneManager.LoadScene("Game_Scene");
    }
    
    public void OnClickMultiplayMode()
    {
        GameModeManager.Mode = GameMode.MultiPlayer;
        SceneManager.LoadScene("Game_Scene");
    }
}