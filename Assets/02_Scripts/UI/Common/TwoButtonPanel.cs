using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TwoButtonPanel : MonoBehaviour
{
    [SerializeField] private Button button1;
    [SerializeField] private Button button2;

    [SerializeField] private TextMeshProUGUI message;

    private void SetButtonEvent(int buttonNum, UnityAction onClick = null)
    {
        if (onClick == null) return;

        if (buttonNum == 1)
        {
            button1.onClick.RemoveAllListeners();
            button1.onClick.AddListener(onClick);
        }
        else if (buttonNum == 2)
        {
            button2.onClick.RemoveAllListeners();
            button2.onClick.AddListener(onClick);
        }
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
    public void SetMessageAndButtonEvent(int buttonNum, string message, Action onConfirm = null)
    {
        SetMessage(message);
        SetButtonEvent(buttonNum, () =>
        {
            onConfirm?.Invoke();
            gameObject.SetActive(false);
        });
        gameObject.SetActive(true);
    }
}