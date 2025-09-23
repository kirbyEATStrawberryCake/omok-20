using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class BasePanel : MonoBehaviour
{
    [SerializeField] [Tooltip("팝업 창에 띄울 메세지")]
    private TMP_Text message;

    /// <summary>
    /// 패널을 열고 생성
    /// </summary>
    public virtual T Show<T>(string message) where T : BasePanel
    {
        gameObject.SetActive(true);
        if (this.message != null)
        {
            this.message.text = message;
        }

        ResetButtons();

        return this as T;
    }

    protected void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    protected abstract void ResetButtons();

    protected void SetButtonListener(Button button, UnityAction action)
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            if (action != null)
                button.onClick.AddListener(action);
        }
    }

    protected void HandleButtonDelay(float delay, params Button[] buttons)
    {
        if (delay > 0 && buttons != null && buttons.Any())
        {
            foreach (var button in buttons)
            {
                if (button != null)
                    button.interactable = false;
            }

            StartCoroutine(EnableButtonWithDelay(delay, buttons));
        }
    }

    private IEnumerator EnableButtonWithDelay(float delay, Button[] buttons)
    {
        yield return new WaitForSeconds(delay);

        foreach (var button in buttons)
        {
            if (button != null)
                button.interactable = true;
        }
    }


    /*protected virtual void InitPanel()
    {
        ClearMessage();
        InitializeButtons();
    }

    protected abstract void InitializeButtons();
    protected abstract void SetButtons();

    protected void OpenPanel(string messageText)
    {
        InitPanel();
        SetMessage(messageText);
        SetButtons();
        gameObject.SetActive(true);
    }


    protected void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    protected void ClearMessage()
    {
        if (message != null)
            message.text = String.Empty;
    }

    protected void SetMessage(string messageText)
    {
        if (message != null)
            message.text = messageText;
    }


    protected void SetButtonListener(Button button, UnityAction action)
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();

            if (action != null)
                button.onClick.AddListener(action);
        }
    }

    protected IEnumerator EnableButtonWithDelay(float delay, params Button[] buttons)
    {
        yield return new WaitForSeconds(delay);

        foreach (var button in buttons)
        {
            if (button != null)
                button.interactable = true;
        }
    }*/
}