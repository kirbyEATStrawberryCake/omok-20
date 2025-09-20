using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum TwoButtonType
{
    Confirm,
    Cancel,
    All
}

public class TwoButtonPanel : MonoBehaviour
{
    [SerializeField] [Tooltip("취소 버튼")] private Button buttonCancel;
    [SerializeField] [Tooltip("확인 버튼")] private Button buttonConfirm;

    [SerializeField] [Tooltip("팝업 창에 띄울 메세지")]
    private TextMeshProUGUI message;

    private TextMeshProUGUI cancelButtonText;
    private TextMeshProUGUI confirmButtonText;

    private void Awake()
    {
        cancelButtonText = buttonCancel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        confirmButtonText = buttonConfirm.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    private void InitPanel()
    {
        buttonCancel.interactable = true;
        buttonConfirm.interactable = true;
        buttonCancel.onClick.RemoveAllListeners();
        buttonConfirm.onClick.RemoveAllListeners();
        cancelButtonText.text = "취소";
        confirmButtonText.text = "확인";
        message.text = "";
    }

    private void SetButtonEvent(UnityAction onConfirm = null, UnityAction onCancel = null)
    {
        if (onConfirm == null && onCancel == null) return;

        if (onConfirm != null)
        {
            buttonConfirm.onClick.RemoveAllListeners();
            buttonConfirm.onClick.AddListener(onConfirm);
        }

        if (onCancel != null)
        {
            buttonCancel.onClick.RemoveAllListeners();
            buttonCancel.onClick.AddListener(onCancel);
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
    /// <param name="onConfirm">확인 버튼을 눌렀을 때 실행할 액션</param>
    /// <param name="onCancel">취소 버튼을 눌렀을 때 실행할 액션</param>
    public void OpenWithSetMessageAndButtonEvent(string message, Action onConfirm = null, Action onCancel = null)
    {
        InitPanel();
        SetMessage(message);
        SetButtonEvent(() =>
            {
                onConfirm?.Invoke();
                gameObject.SetActive(false);
            },
            () =>
            {
                onCancel?.Invoke();
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
    public void OpenWithSetMessageAndButtonEvent(string message, TwoButtonType buttonType, float delay,
        Action onConfirm = null, Action onCancel = null)
    {
        switch (buttonType)
        {
            case TwoButtonType.Confirm:
                buttonConfirm.interactable = false;
                break;
            case TwoButtonType.Cancel:
                buttonCancel.interactable = false;
                break;
            case TwoButtonType.All:
                buttonConfirm.interactable = false;
                buttonCancel.interactable = false;
                break;
        }

        OpenWithSetMessageAndButtonEvent(message, onConfirm, onCancel);
        StartCoroutine(EnableButtonWithDelay(buttonType, delay));
    }

    private IEnumerator EnableButtonWithDelay(TwoButtonType buttonType, float delay)
    {
        yield return new WaitForSeconds(delay);
        switch (buttonType)
        {
            case TwoButtonType.Confirm:
                buttonConfirm.interactable = true;
                break;
            case TwoButtonType.Cancel:
                buttonCancel.interactable = true;
                break;
            case TwoButtonType.All:
                buttonConfirm.interactable = true;
                buttonCancel.interactable = true;
                break;
        }
    }

    public void SetButtonText(string confirmText, string cancelText)
    {
        cancelButtonText.text = cancelText;
        confirmButtonText.text = confirmText;
    }
}