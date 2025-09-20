using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OneButtonPanel : MonoBehaviour
{
    [SerializeField] private Button button;

    [SerializeField] [Tooltip("팝업 창에 띄울 메세지")]
    private TextMeshProUGUI message;

    private void InitPanel()
    {
        button.interactable = true;
        button.onClick.RemoveAllListeners();
        message.text = "";
    }

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
    /// <param name="onConfirm">확인 버튼을 눌렀을 때 실행할 액션</param>
    public void OpenWithSetMessageAndButtonEvent(string message, Action onConfirm = null)
    {
        InitPanel();
        SetMessage(message);
        SetButtonEvent(() =>
        {
            onConfirm?.Invoke();
            gameObject.SetActive(false);
        });
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 외부에서 OneButtonPanel의 메세지와 버튼 클릭 시 수행할 액션을 설정하는 메소드, 버튼을 딜레이 이후 활성화 시킴
    /// </summary>
    /// <param name="message">메세지</param>
    /// <param name="onConfirm">확인 버튼을 눌렀을 때 실행할 액션</param>
    /// <param name="delay">버튼 활성화 딜레이</param>
    public void OpenWithSetMessageAndButtonEvent(string message, float delay, Action onConfirm = null)
    {
        button.interactable = false;
        OpenWithSetMessageAndButtonEvent(message, onConfirm);
        StartCoroutine(EnableButtonWithDelay(delay));
    }

    private IEnumerator EnableButtonWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        button.interactable = true;
    }
}