using DG.Tweening;
using PMP.BetterButton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFinishCanvasController : MonoBehaviour {

    [SerializeField] CanvasGroup canvasGroup;

    [SerializeField] BetterButton retryBtn;
    [SerializeField] BetterButton goPerformBtn;
    [SerializeField] BetterButton stageSelectBtn;

    [SerializeField] ConfirmDialogController confirmDialog;

    private void Start() {
        ResisterListeners();
        canvasGroup.alpha = 0;
    }

    void ResisterListeners() {
        if (retryBtn)
            retryBtn.onClick = () => {
                confirmDialog.Create(
                    duration: 0.1f,
                    title: "�x��",
                    desc: "������x���K�X�e�[�W���v���C���܂����H",
                    yesBtnLabel: "�͂�",
                    noBtnLabel: "������",
                    yesBtnAction: () => {
                        confirmDialog.Close();
                        SceneLoader.Instance.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                    },
                    noBtnAction: () => {
                        confirmDialog.Close(completeCallback: retryBtn.Select);
                    },
                    openCompAction: confirmDialog.NoButton.Select
                    );
            };

        if (goPerformBtn)
            goPerformBtn.onClick = () => {
                confirmDialog.Create(
                    duration: 0.1f,
                    title: "�x��",
                    desc: "�{�ԃX�e�[�W�Ɉړ����܂����H",
                    yesBtnLabel: "�͂�",
                    noBtnLabel: "������",
                    yesBtnAction: () => {
                        confirmDialog.Close();
                        StageManager.Instance.repeatStageController.SetClearActionKey("GO_PERFORMANCE_STAGE");
                    },
                    noBtnAction: () => {
                        confirmDialog.Close(completeCallback: goPerformBtn.Select);
                    },
                    openCompAction: confirmDialog.NoButton.Select
                    );
            };

        if (stageSelectBtn)
            stageSelectBtn.onClick = () => {
                /* confirmDialog.Create(
                    duration: 0.1f,
                    title: "�x��",
                    desc: "�X�e�[�W�I����ʂɖ߂�܂��B\n�{���ɂ�낵���ł����H",
                    yesBtnLabel: "�͂�",
                    noBtnLabel: "������",
                    yesBtnAction: () => {
                        confirmDialog.Close();
                        SceneLoader.Instance.LoadScene("StageSelectScene");
                    },
                    noBtnAction: () => {
                        confirmDialog.Close(completeCallback: stageSelectBtn.Select);
                    },
                    openCompAction: confirmDialog.NoButton.Select
                    ); */

                SceneLoader.Instance.LoadScene("StageSelectScene");
            };

        retryBtn.onSelected = () => retryBtn.uiLabel.color = Color.white;
        goPerformBtn.onSelected = () => goPerformBtn.uiLabel.color = Color.white;
        stageSelectBtn.onSelected = () => stageSelectBtn.uiLabel.color = Color.white;

        retryBtn.onDeselected = () => retryBtn.uiLabel.color = Color.black;
        goPerformBtn.onDeselected = () => goPerformBtn.uiLabel.color = Color.black;
        stageSelectBtn.onDeselected = () => stageSelectBtn.uiLabel.color = Color.black;
    }

    public void Open() {
        ResisterListeners();

        stageSelectBtn.Select();

        gameObject.SetActive(true);
        canvasGroup.alpha = 0.0f;
        canvasGroup.DOFade(1.0f, 0.5f);
    }

    public void Close() {
        // gameObject.SetActive(false);
        canvasGroup.DOFade(0.0f, 0.5f).OnComplete(() => { gameObject.SetActive(false); });
    }

}