using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OneButtonPanel : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI message;

    public void SetButtonEvent(UnityAction onClick = null)
    {
        if (onClick == null) return;
        
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);
    }
    
    public void SetMessage(string message)
    {
        this.message.text = message;
    }
}
