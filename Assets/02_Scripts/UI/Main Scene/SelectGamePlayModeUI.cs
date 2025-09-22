using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectGamePlayModeUI : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = true;


    /// <summary>
    /// 싱글 플레이어 모드 선택
    /// </summary>
    public void OnClickSingleMode()
    {
        SetGameModeAndLoadScene(GameMode.SinglePlayer);
    }

    /// <summary>
    /// 멀티플레이어 모드 선택
    /// </summary>
    public void OnClickMultiplayMode()
    {
        SetGameModeAndLoadScene(GameMode.MultiPlayer);
    }

    /// <summary>
    /// 게임 모드 설정 및 게임 씬 로드
    /// </summary>
    private void SetGameModeAndLoadScene(GameMode mode)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"<color=cyan>게임 모드 변경: {mode}</color>");
        }

        GameModeManager.Mode = mode;
        SceneController.LoadScene(SceneType.Game);
    }
}