using Cysharp.Threading.Tasks;
using DG.Tweening;
using KanKikuchi.AudioManager;
using PMP.BetterButton;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuManager : SingletonMonoBehaviour<GameMenuManager> {

    bool isShowingMenu = false;

    // メニューの開閉操作をロック
    bool isLockedMenu;

    [SerializeField] CanvasGroup menuCanvas;
    [SerializeField] CanvasGroup gameCanvas;

    [Header("Menu Buttons")]
    [SerializeField] BetterButton resumeBtn;
    [SerializeField] BetterButton restartBtn;
    [SerializeField] BetterButton changeDifficultyBtn;
    [SerializeField] BetterButton goPerformBtn;
    [SerializeField] BetterButton stageSelectBtn;

    [Header("Dialog")]
    [SerializeField] ConfirmDialogController confirmDialog;


    [SerializeField] Material pageImgShader;

    private void Start() {
        SetLockState(true);

        isShowingMenu = false;
        //menuCanvas.alpha = 0;
        menuCanvas.interactable = false;
        menuCanvas.blocksRaycasts = false;

        ConfigureMenu();
        ResisterButtonEvents();
    }

    public void Lock() {
        SetLockState(true);
    }

    public void Unlock() {
        SetLockState(false);
    }

    public void SetLockState(bool newState) {
        isLockedMenu = newState;
    }

    public bool GetLockState() => isLockedMenu;

    /// <summary>
    /// メニューを構成する
    /// </summary>
    public void ConfigureMenu() {
        StageManager.StageType type = StageManager.Instance.GetStageType();

        bool resumeBtnActive = true;
        bool restartBtnActive = true;
        bool changeDifficultyBtnActive = true;
        bool goActualPerformanceBtnActive = true;
        bool returnToStageSelectBtnActive = true;

        switch (type) {
            case StageManager.StageType.FlashStage:
                goActualPerformanceBtnActive = false;
                break;
            case StageManager.StageType.RepeatStage:
                var phase = StageManager.Instance.repeatStageController.currentPhase;
                // 練習
                if (phase == RepeatStageController.StagePhase.Practice) {
                } else
                // 本番
                if (phase == RepeatStageController.StagePhase.Performance) {
                    restartBtnActive = false;
                    changeDifficultyBtnActive = false;
                    goActualPerformanceBtnActive = false;
                }
                break;
            case StageManager.StageType.JingleStage:
                changeDifficultyBtnActive = false;
                goActualPerformanceBtnActive = false;
                break;
            default: break;
        }

        resumeBtn.gameObject.SetActive(resumeBtnActive);
        restartBtn.gameObject.SetActive(restartBtnActive);
        changeDifficultyBtn.gameObject.SetActive(changeDifficultyBtnActive);
        goPerformBtn.gameObject.SetActive(goActualPerformanceBtnActive);
        stageSelectBtn.gameObject.SetActive(returnToStageSelectBtnActive);
    }

    void ResisterButtonEvents() {
        resumeBtn.onClick = null;
        restartBtn.onClick = null;
        changeDifficultyBtn.onClick = null;
        goPerformBtn.onClick = null;
        stageSelectBtn.onClick = null;

        resumeBtn.onSelected = null;
        restartBtn.onSelected = null;
        changeDifficultyBtn.onSelected = null;
        goPerformBtn.onSelected = null;
        stageSelectBtn.onSelected = null;

        resumeBtn.onDeselected = null;
        restartBtn.onDeselected = null;
        changeDifficultyBtn.onDeselected = null;
        goPerformBtn.onDeselected = null;
        stageSelectBtn.onDeselected = null;

        resumeBtn.onClick += CloseMenu;
        restartBtn.onClick += Restart;
        changeDifficultyBtn.onClick += ChangeDifficulty;
        goPerformBtn.onClick += GoPerform;
        stageSelectBtn.onClick += ReturnToStageSelect;

        resumeBtn.onClick += () => SEManager.Instance.Play(SEPath.SUBMIT);
        restartBtn.onClick += () => SEManager.Instance.Play(SEPath.SUBMIT);
        changeDifficultyBtn.onClick += () => SEManager.Instance.Play(SEPath.SUBMIT);
        goPerformBtn.onClick += () => SEManager.Instance.Play(SEPath.SUBMIT);
        stageSelectBtn.onClick += () => SEManager.Instance.Play(SEPath.SUBMIT);

        resumeBtn.onSelected += () => {
            SEManager.Instance.Play(SEPath.NAVIGATE);
            resumeBtn.uiLabel.color = Color.white;
        };
        restartBtn.onSelected += () => {
            SEManager.Instance.Play(SEPath.NAVIGATE);
            restartBtn.uiLabel.color = Color.white;
        };
        changeDifficultyBtn.onSelected += () => {
            SEManager.Instance.Play(SEPath.NAVIGATE);
            changeDifficultyBtn.uiLabel.color = Color.white;
        };
        goPerformBtn.onSelected += () => {
            SEManager.Instance.Play(SEPath.NAVIGATE);
            goPerformBtn.uiLabel.color = Color.white;
        };
        stageSelectBtn.onSelected += () => {
            SEManager.Instance.Play(SEPath.NAVIGATE);
            stageSelectBtn.uiLabel.color = Color.white;
        };

        resumeBtn.onDeselected += () => resumeBtn.uiLabel.color = Color.black;
        restartBtn.onDeselected += () => restartBtn.uiLabel.color = Color.black;
        changeDifficultyBtn.onDeselected += () => changeDifficultyBtn.uiLabel.color = Color.black;
        goPerformBtn.onDeselected += () => goPerformBtn.uiLabel.color = Color.black;
        stageSelectBtn.onDeselected += () => stageSelectBtn.uiLabel.color = Color.black;
    }

    public void SetMenuShowingState(bool? newState = null) {
        if (newState == null) {
            if (isShowingMenu) CloseMenu(0.35f); else OpenMenu(0.35f);
            return;
        }

        if (newState == true) {
            OpenMenu(0.35f);
        } else {
            CloseMenu(0.35f);
        }
    }

    public void CloseMenu() {
        CloseMenu(0.35f);
    }

    public void CloseMenu(float duration) {

        DeactivateAllButtons();

        pageImgShader.DOFloat(1, "_Flip", duration);
        if (gameCanvas) gameCanvas.DOFade(1.0f, duration * 0.5f);

        if (duration > 0) SEManager.Instance.Play(SEPath.FLIP_PAGE, pitch: 0.9438743127f);

        GameManager.Instance.InputCtrl.PlayerInput.SwitchCurrentActionMap("Player");
        if (StageManager.Instance.stageType == StageManager.StageType.FlashStage) StageManager.Instance.flashStageController.stageGlowEff.Play();

        menuCanvas.interactable = false;
        menuCanvas.blocksRaycasts = false;
        DeactivateAllButtons();

        isShowingMenu = false;
    }

    public void OpenMenu() {
        OpenMenu(0);
    }

    public void OpenMenu(float duration) {

        isShowingMenu = true;

        pageImgShader.DOFloat(-1, "_Flip", duration);
        if (gameCanvas) gameCanvas.DOFade(0.0f, duration * 0.5f);

        if (duration > 0) SEManager.Instance.Play(SEPath.FLIP_PAGE, pitch: 1.059463094f);

        GameManager.Instance.InputCtrl.PlayerInput.SwitchCurrentActionMap("UI");
        if (StageManager.Instance.stageType == StageManager.StageType.FlashStage) StageManager.Instance.flashStageController.stageGlowEff.Pause();

        menuCanvas.interactable = true;
        menuCanvas.blocksRaycasts = true;
        ActivateAllButtons();

        resumeBtn.Select();
    }

    void Restart() {
        CloseMenu();
        GameManager.Instance.PlayerRespawn();

        SEManager.Instance.Play(SEPath.SUBMIT);
    }

    void ChangeDifficulty() {
        confirmDialog.Create(
            duration: 0.1f,
            title: "警告",
            desc: "難易度を変更すると最初からになります。\n本当によろしいですか？",
            yesBtnLabel: "はい",
            noBtnLabel: "いいえ",
            yesBtnAction: () => {
                DeactivateAllButtons();
                confirmDialog.Close();
                SceneLoader.Instance.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            },
            noBtnAction: () => { confirmDialog.Close(completeCallback: changeDifficultyBtn.Select); },
            openCompAction: confirmDialog.NoButton.Select
            );
    }

    void GoPerform() {
        confirmDialog.Create(
                duration: 0.1f,
                title: "警告",
                desc: "本番ステージに移動しますか？",
                yesBtnLabel: "はい",
                noBtnLabel: "いいえ",
                yesBtnAction: () => {
                    DeactivateAllButtons();
                    confirmDialog.Close();
                    StageManager.Instance.repeatStageController.GoPerformPhase(this.GetCancellationTokenOnDestroy()).Forget();
                },
                noBtnAction: () => {
                    confirmDialog.Close(completeCallback: goPerformBtn.Select);
                },
                openCompAction: confirmDialog.NoButton.Select
                );
    }

    void ReturnToStageSelect() {
        confirmDialog.Create(
            duration: 0.1f,
            title: "警告",
            desc: "ステージ選択画面に戻ります。\n本当によろしいですか？",
            yesBtnLabel: "はい",
            noBtnLabel: "いいえ",
            yesBtnAction: () => {
                confirmDialog.Close();
                DeactivateAllButtons();
                SceneLoader.Instance.LoadScene("StageSelectScene");
            },
            noBtnAction: () => {
                confirmDialog.Close(completeCallback: stageSelectBtn.Select);
            },
            openCompAction: confirmDialog.NoButton.Select
            );
    }

    void ActivateAllButtons() {
        resumeBtn.interactable = true;
        restartBtn.interactable = true;
        changeDifficultyBtn.interactable = true;
        goPerformBtn.interactable = true;
        stageSelectBtn.interactable = true;
    }

    void DeactivateAllButtons() {
        resumeBtn.interactable = false;
        restartBtn.interactable = false;
        changeDifficultyBtn.interactable = false;
        goPerformBtn.interactable = false;
        stageSelectBtn.interactable = false;
    }

}