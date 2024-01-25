using DG.Tweening;
using KanKikuchi.AudioManager;
using PMP.BetterButton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageDifficultySelectPanelController : MonoBehaviour {

    [SerializeField] CanvasGroup canvasGroup;

    [SerializeField] List<BetterButton> buttons;

    private void Start() => Initialize();

    /// <summary>
    /// èâä˙âªèàóù
    /// </summary>
    void Initialize() {
        Close();

        for (int i = 0; i < buttons.Count; i++) {
            var btn = buttons[i];
            btn.allowOnlyOnceInput = true;
            btn.ResetInteractOnlyOnceFlag();

            StageManager.StageDifficulty targetDifficulty = StageManager.StageDifficulty.None;
            switch (i) {
                case 0:
                    targetDifficulty = StageManager.StageDifficulty.Easy;
                    break;
                case 1:
                    targetDifficulty = StageManager.StageDifficulty.Normal;
                    break;
                case 2:
                    targetDifficulty = StageManager.StageDifficulty.Hard;
                    break;
            }

            btn.onSelected += () => {
                SEManager.Instance.Play(SEPath.NAVIGATE);
                btn.uiLabel.color = Color.white;
            };

            btn.onDeselected += () => {
                btn.uiLabel.color = Color.black;
            };

            btn.onClick = () => {
                if (StageManager.Instance.difficulty != StageManager.StageDifficulty.None) return;

                SEManager.Instance.Play(SEPath.SUBMIT);

                // ìÔà’ìxïœçX
                StageManager.Instance.ChangeDifficulty(targetDifficulty);

                buttons.ForEach(t => t.interactable = false);

                Close(0.5f);
            };
        }
    }

    public void Open() {
        Open(0);
    }

    public void Open(float duration = 1.0f) {
        canvasGroup.alpha = 0.0f;

        if (duration > 0.0f) {
            canvasGroup.DOFade(1, duration).OnComplete(() => {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            });
        } else {
            canvasGroup.alpha = 1.0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        // Hard ÇëIëÇµÇƒÇ®Ç≠
        buttons[2].Select();
    }

    public void Close() {
        Close(0);
    }

    public void Close(float duration = 1.0f) {
        if (duration > 0.0f) {
            canvasGroup.DOFade(0, duration).OnComplete(() => {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.alpha = 0.0f;
            });
        } else {
            canvasGroup.alpha = 0.0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}