using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OneButtonPanel : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI message;

    private void SetButtonEvent(UnityAction onClick = null)
    {
        if (onClick == null) return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);
    }
    
    private void SetMessage(string message)
    {
        this.message.text = message;
    }

    /// <summary>
    /// 외부에서 OneButtonPanel의 메세지와 버튼 클릭 시 수행할 액션을 설정하는 메소드
    /// </summary>
    /// <param name="message">메세지</param>
    /// <param name="onConfirm"></param>
    public void SetMessageAndButtonEvent(string message, Action onConfirm = null)
    {
        SetMessage(message);
        SetButtonEvent(() =>
        {
            onConfirm?.Invoke();
            gameObject.SetActive(false);
        });
        gameObject.SetActive(true);
    }
}