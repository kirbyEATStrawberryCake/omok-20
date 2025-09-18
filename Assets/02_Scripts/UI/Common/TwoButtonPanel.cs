using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TwoButtonPanel : MonoBehaviour
{
    [SerializeField] [Tooltip("확인 버튼")] private Button buttonConfirm;
    [SerializeField] [Tooltip("취소 버튼")] private Button buttonCancel;

    [SerializeField] [Tooltip("팝업 창에 띄울 메세지")]
    private TextMeshProUGUI message;

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
}