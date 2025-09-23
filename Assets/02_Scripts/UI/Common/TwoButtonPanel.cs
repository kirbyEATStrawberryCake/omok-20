using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TwoButtonType
{
    Confirm,
    Cancel,
    All
}

public class TwoButtonPanel : BasePanel
{
    [SerializeField] [Tooltip("취소 버튼")] private Button cancelButton;
    [SerializeField] [Tooltip("확인 버튼")] private Button confirmButton;

    [SerializeField] [Tooltip("취소 버튼 텍스트")]
    private TMP_Text cancelButtonText;

    [SerializeField] [Tooltip("확인 버튼 텍스트")]
    private TMP_Text confirmButtonText;

    private const string DEFAULT_CANCEL_TEXT = "취소";
    private const string DEFAULT_CONFIRM_TEXT = "확인";

    /// <summary>
    /// 버튼에 대한 액션을 설정
    /// </summary>
    public TwoButtonPanel OnButtons(Action onConfirm, Action onCancel = null)
    {
        SetButtonListener(confirmButton, () =>
        {
            onConfirm?.Invoke();
            ClosePanel();
        });

        SetButtonListener(cancelButton, () =>
        {
            onCancel?.Invoke();
            ClosePanel();
        });

        return this;
    }

    /// <summary>
    /// 버튼 텍스트를 설정
    /// </summary>
    public TwoButtonPanel WithButtonText(string confirmText = null, string cancelText = null)
    {
        if (confirmButtonText != null && !string.IsNullOrEmpty(confirmText))
            confirmButtonText.text = confirmText;

        if (cancelButtonText != null && !string.IsNullOrEmpty(cancelText))
            cancelButtonText.text = cancelText;

        return this;
    }

    /// <summary>
    /// 특정 버튼의 활성화를 지연시킴
    /// </summary>
    public TwoButtonPanel WithDelay(float delay, TwoButtonType buttonType)
    {
        if (delay <= 0) return this;

        List<Button> buttonsToDelay = new();
        if (buttonType == TwoButtonType.Confirm || buttonType == TwoButtonType.All)
            buttonsToDelay.Add(confirmButton);
        if (buttonType == TwoButtonType.Cancel || buttonType == TwoButtonType.All)
            buttonsToDelay.Add(cancelButton);

        HandleButtonDelay(delay, buttonsToDelay.ToArray());

        return this;
    }

    protected override void ResetButtons()
    {
        if (confirmButton != null)
        {
            confirmButton.interactable = true;
            confirmButton.onClick.RemoveAllListeners();
        }

        if (cancelButton != null)
        {
            cancelButton.interactable = true;
            cancelButton.onClick.RemoveAllListeners();
        }

        if (confirmButtonText != null) confirmButtonText.text = DEFAULT_CONFIRM_TEXT;
        if (cancelButtonText != null) cancelButtonText.text = DEFAULT_CANCEL_TEXT;
    }


    /*private Action confirmButtonAction;
    private Action cancelButtonAction;

    #region Abstract Methods

    protected override void InitializeButtons()
    {
        SetButtonInteractable(true, true);
        ClearButtonListeners();
        ResetButtonTexts();
    }

    protected override void SetButtons()
    {
        SetButtonListener(confirmButton, () =>
        {
            confirmButtonAction?.Invoke();
            ClosePanel();
        });

        SetButtonListener(cancelButton, () =>
        {
            cancelButtonAction?.Invoke();
            ClosePanel();
        });
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 투버튼 패널을 열고 메세지와 버튼 이벤트를 설정
    /// </summary>
    /// <param name="messageText">표시할 메세지</param>
    /// <param name="onConfirm">확인 버튼을 눌렀을 때 실행할 액션</param>
    /// <param name="onCancel">취소 버튼을 눌렀을 때 실행할 액션</param>
    public void OpenWithSetMessageAndButtonEvent(string messageText, Action onConfirm = null, Action onCancel = null)
    {
        confirmButtonAction = onConfirm;
        cancelButtonAction = onCancel;

        OpenPanel(messageText);
    }


    /// <summary>
    /// 투버튼 패널을 열고 메세지와 버튼 이벤트를 설정, 지연 후 버튼을 활성화
    /// </summary>
    /// <param name="messageText">표시할 메세지</param>
    /// <param name="buttonType">딜레이 후 활성화할 버튼 타입</param>
    /// <param name="delay">버튼 활성화 딜레이</param>
    /// <param name="onConfirm">확인 버튼을 눌렀을 때 실행할 액션</param>
    /// <param name="onCancel">취소 버튼을 눌렀을 때 실행할 액션</param>
    public void OpenWithSetMessageAndButtonEvent(string messageText, TwoButtonType buttonType, float delay,
        Action onConfirm = null, Action onCancel = null)
    {
        confirmButtonAction = onConfirm;
        cancelButtonAction = onCancel;

        OpenPanel(messageText);
        DisableButtonsByType(buttonType);

        switch (buttonType)
        {
            case TwoButtonType.Confirm:
                StartCoroutine(EnableButtonWithDelay(delay, confirmButton));
                break;
            case TwoButtonType.Cancel:
                StartCoroutine(EnableButtonWithDelay(delay, cancelButton));
                break;
            case TwoButtonType.All:
                StartCoroutine(EnableButtonWithDelay(delay, confirmButton, cancelButton));
                break;
        }
    }

    /// <summary>
    /// 버튼 텍스트 설정
    /// </summary>
    /// <param name="confirmText">확인 버튼 텍스트</param>
    /// <param name="cancelText">취소 버튼 텍스트</param>
    public void SetButtonText(string confirmText, string cancelText)
    {
        if (cancelButtonText != null)
            cancelButtonText.text = cancelText;

        if (confirmButtonText != null)
            confirmButtonText.text = confirmText;
    }

    #endregion

    #region Private Methods

    private void SetButtonInteractable(bool confirmInteractable, bool cancelInteractable)
    {
        if (confirmButton != null)
            confirmButton.interactable = confirmInteractable;

        if (cancelButton != null)
            cancelButton.interactable = cancelInteractable;
    }

    private void ClearButtonListeners()
    {
        if (confirmButton != null)
            confirmButton.onClick.RemoveAllListeners();

        if (cancelButton != null)
            cancelButton.onClick.RemoveAllListeners();
    }

    private void ResetButtonTexts()
    {
        if (confirmButtonText != null)
            confirmButtonText.text = DEFAULT_CONFIRM_TEXT;

        if (cancelButtonText != null)
            cancelButtonText.text = DEFAULT_CANCEL_TEXT;
    }

    private void DisableButtonsByType(TwoButtonType buttonType)
    {
        switch (buttonType)
        {
            case TwoButtonType.Confirm:
                SetButtonInteractable(false, true);
                break;
            case TwoButtonType.Cancel:
                SetButtonInteractable(true, false);
                break;
            case TwoButtonType.All:
                SetButtonInteractable(true, true);
                break;
        }
    }

    private void EnableButtonsByType(TwoButtonType buttonType)
    {
        switch (buttonType)
        {
            case TwoButtonType.Confirm:
                if (confirmButton != null)
                    confirmButton.interactable = true;
                break;
            case TwoButtonType.Cancel:
                if (cancelButton != null)
                    cancelButton.interactable = true;
                break;
            case TwoButtonType.All:
                SetButtonInteractable(true, true);
                break;
        }
    }

    #endregion*/
}