using UnityEngine;
using UnityEngine.SceneManagement;

public class GiboExit : MonoBehaviour
{
    public void OnClickGiboExit()
    {
        SceneController.LoadScene(SceneType.Main);
    }

}
