#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class PlayerPrefsCleaner
{
    [MenuItem("Tools/Clear PlayerPrefs")]
    private static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPrefs cleared!");
    }
}
#endif