using TMPro;
using UnityEngine;

public class MessageTest : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI message1;
    [SerializeField] private TextMeshProUGUI message2;

    public void SetMessage(int num, string message, Color color)
    {
        if (num == 1)
        {
            message1.text = message;
            message1.color = color;
        }
        else
        {
            message2.text = message;
            message2.color = color;
        }
    }

    public void ClearMessage(int num)
    {
        if (num == 1)
        {
            message1.text = "";
            message1.color = Color.white;
        }
        else
        {
            message2.text = "";
            message2.color = Color.white;
        }
    }
}