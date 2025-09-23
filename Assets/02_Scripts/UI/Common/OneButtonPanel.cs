using System;
using UnityEngine;
using UnityEngine.UI;

public class OneButtonPanel : BasePanel
{
    [SerializeField] private Button button;

    /// <summary>
    /// 확인 버튼에 대한 액션을 설정
    /// </summary>
    /// <param name="onConfirmAction">확인 버튼 클릭 시 실행될 액션</param>
    /// <param name="delay">버튼 활성화 지연 시간</param>
    public OneButtonPanel OnConfirm(Action onConfirmAction, float delay = 0f)
    {
        SetButtonListener(button, () =>
        {
            onConfirmAction?.Invoke();
            ClosePanel();
        });

        HandleButtonDelay(delay, button);

        return this;
    }

    protected override void ResetButtons()
    {
        if (button != null)
        {
            button.interactable = true;
            button.onClick.RemoveAllListeners();
        }
    }

    /*private Action buttonAction;

    #region Abstract Methods

    protected override void InitializeButtons()
    {
        if (button != null)
        {
            button.interactable = true;
            button.onClick.RemoveAllListeners();
        }
    }


    protected override void SetButtons()
    {
        SetButtonListener(button, () =>
        {
            buttonAction?.Invoke();
            ClosePanel();
        });
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 원버튼 패널을 열고 메세지와 버튼 이벤트를 설정
    /// </summary>
    /// <param name="messageText">표시할 메세지</param>
    /// <param name="onConfirm">확인 버튼을 눌렀을 때 실행할 액션</param>
    public void OpenWithSetMessageAndButtonEvent(string messageText, Action onConfirm = null)
    {
        buttonAction = onConfirm;
        OpenPanel(messageText);
    }

    /// <summary>
    /// 원버튼 패널을 열고 메세지와 버튼 이벤트를 설정, 지연 후 버튼을 활성화
    /// </summary>
    /// <param name="messageText">표시할 메세지</param>
    /// <param name="delay">버튼 활성화 딜레이</param>
    /// <param name="onConfirm">확인 버튼을 눌렀을 때 실행할 액션</param>
    public void OpenWithSetMessageAndButtonEvent(string messageText, float delay, Action onConfirm = null)
    {
        buttonAction = onConfirm;

        if (button != null)
            button.interactable = false;

        OpenPanel(messageText);

        StartCoroutine(EnableButtonWithDelay(delay, button));
    }

    #endregion*/
}