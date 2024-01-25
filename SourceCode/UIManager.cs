using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class UIManager : SingletonMonoBehaviour<UIManager> {

    [SerializeField] RectTransform flashIconContainer;
    List<RectTransform> flashIcons = new List<RectTransform>();

    [Header("難易度パネル")]
    [SerializeField] StageDifficultySelectPanelController _difficultySelectCtrl;
    public StageDifficultySelectPanelController difficultySelectCtrl {
        get => _difficultySelectCtrl;
    }

    [SerializeField] TextMeshProUGUI stagePhaseText;
    [SerializeField] Animator stagePhaseTextAnim;

    [Header("チュートリアル")]
    [SerializeField] CanvasGroup tutorialCanvasGroup;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() { }

    public void UpdateFlashCountStateText() {

        FlashStageController ctrl = StageManager.Instance.flashStageController;

        if (!ctrl) return;

        int remaining = ctrl.flashLimitCount - ctrl.currentFlashedCount;
        int max = ctrl.flashLimitCount;

        for (int i = 0; i < flashIconContainer.childCount; i++) {
            if (i < remaining) {
                ChangeFlashIconState(flashIconContainer.GetChild(i), false);
                flashIconContainer.GetChild(i).gameObject.SetActive(true);
            } else if (i < max) {
                ChangeFlashIconState(flashIconContainer.GetChild(i), true);
                flashIconContainer.GetChild(i).gameObject.SetActive(true);
            } else {
                flashIconContainer.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    void ChangeFlashIconState(Transform target, bool isEmpty) {
        var empty = target.Find("Empty");
        var filled = target.Find("Filled");

        if (isEmpty) {
            filled.gameObject.SetActive(false);
            empty.gameObject.SetActive(true);
        } else {
            filled.gameObject.SetActive(true);
            empty.gameObject.SetActive(false);
        }
    }

    public void DisplayCurrentStagePhase() {
        Color col = stagePhaseText.color;
        col.a = 0;
        stagePhaseText.color = col;

        stagePhaseText.text = $"ステージ {StageManager.Instance.flashStageController.currentStagePhase}";

        stagePhaseTextAnim.Play("StagePhase Display Test");
    }

    public void HideCurrentStagePhase() {
        stagePhaseTextAnim.Play("Empty");
    }

    public void ShowTutorial(float duration) {
        if (!tutorialCanvasGroup.gameObject.activeSelf) {
            tutorialCanvasGroup.alpha = 0.0f;
            tutorialCanvasGroup.gameObject.SetActive(true);
        }
        tutorialCanvasGroup.DOFade(1.0f, duration);
    }

    public void HideTutorial(float duration) {
        tutorialCanvasGroup.DOFade(0.0f, duration).SetDelay(0.5f).OnComplete(() => {
            tutorialCanvasGroup.gameObject.SetActive(false);
        });
    }
}