using DG.Tweening;
using KanKikuchi.AudioManager;
using PMP.BetterButton;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ConfirmDialogController : MonoBehaviour {

    [SerializeField] CanvasGroup canvasGroup;

    [SerializeField] TextMeshProUGUI titleLabel;
    [SerializeField] TextMeshProUGUI descLabel;

    [SerializeField] BetterButton yesBtn;
    [SerializeField] BetterButton noBtn;

    public BetterButton YesButton => yesBtn;
    public BetterButton NoButton => noBtn;

    // Start is called before the first frame update
    void Start() {
        Close(0);
    }

    public void SetTitleText(string str) { titleLabel.text = str; }

    public void SetDescText(string str) { descLabel.text = str; }

    /// <summary>
    /// ‘I‘ðŽˆ‚ð—LŒø‰»
    /// </summary>
    public void ActivateChoises() {
        YesButton.interactable = true;
        NoButton.interactable = true;
    }

    /// <summary>
    /// ‘I‘ðŽˆ‚ð–³Œø‰»
    /// </summary>
    public void DeactivateChoises() {
        YesButton.interactable = false;
        NoButton.interactable = false;
    }

    void InitializeChoises() {
        YesButton.allowOnlyOnceInput = true;
        NoButton.allowOnlyOnceInput = true;

        YesButton.ResetInteractOnlyOnceFlag();
        NoButton.ResetInteractOnlyOnceFlag();

        YesButton.onClick = DeactivateChoises;
        NoButton.onClick = DeactivateChoises;

        YesButton.onClick += () => { SEManager.Instance.Play(SEPath.SUBMIT); };
        NoButton.onClick += () => { SEManager.Instance.Play(SEPath.CANCEL_CLOSE); };

        YesButton.onSelected = () => { SEManager.Instance.Play(SEPath.NAVIGATE); YesButton.uiLabel.color = Color.white; };
        NoButton.onSelected = () => { SEManager.Instance.Play(SEPath.NAVIGATE); NoButton.uiLabel.color = Color.white; };

        YesButton.onDeselected = () => { YesButton.uiLabel.color = Color.black; };
        NoButton.onDeselected = () => { NoButton.uiLabel.color = Color.black; };
    }

    public void Create(float duration, string title, string desc, string yesBtnLabel, string noBtnLabel, UnityAction yesBtnAction, UnityAction noBtnAction, Action openCompAction = null) {
        InitializeChoises();

        SetTitleText(title);
        SetDescText(desc);

        YesButton.ChangeLabelText(yesBtnLabel);
        NoButton.ChangeLabelText(noBtnLabel);

        YesButton.onClick += yesBtnAction;
        NoButton.onClick += noBtnAction;

        ActivateChoises();

        Open(duration, openCompAction);
    }

    void Open(float duration = 0.1f, Action completeCallback = null) {
        GameMenuManager.Instance.Lock();

        canvasGroup.alpha = 0;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.DOFade(1f, duration).OnComplete(() => {
            completeCallback?.Invoke();
        });
    }

    public void Close(float duration = 0.1f, Action completeCallback = null) {
        GameMenuManager.Instance.Unlock();

        canvasGroup.alpha = 1;
        canvasGroup.DOFade(0f, duration).OnComplete(() => {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            completeCallback?.Invoke();
        });
    }

}