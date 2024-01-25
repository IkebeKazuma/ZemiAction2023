using Cysharp.Threading.Tasks;
using DG.Tweening;
using KanKikuchi.AudioManager;
using PMP.BetterButton;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StageSelectManager : MonoBehaviour {

    private string selectedStageName = string.Empty;

    [SerializeField] BetterButton flashStageBtn;
    [SerializeField] BetterButton repeatStageBtn;

    [SerializeField] RectTransform pointer;
    [SerializeField] RectTransform pointerTriangle;
    [SerializeField] Vector2 pointerTriangleOffset;
    [SerializeField] float pointerUpDownRange = 0.5f;
    [SerializeField] float pointerUpDownDuration = 0.5f;
    Vector2 pointerTargetPosition;
    [SerializeField] float pointerMoveSpeed = 5.0f;

    [Header("Stage Preview")]
    [SerializeField] CanvasGroup stageInfoPreviewCGFlash;
    [SerializeField] CanvasGroup stageInfoPreviewCGRepeat;
    [SerializeField] TMPro.TextMeshProUGUI stageNameText;
    string stageSelectState = "";
    UnityAction decideAct, cancelAct;

    [SerializeField]BetterButton backToTitleBtn;

    List<LayoutGroup> allLayoutGroups = new List<LayoutGroup>();


    private void Start() {
        selectedStageName = string.Empty;
        stageSelectState = string.Empty;

        stageInfoPreviewCGFlash.alpha = 0.0f;
        stageInfoPreviewCGRepeat.alpha = 0.0f;

        allLayoutGroups = new List<LayoutGroup>();
        allLayoutGroups = FindObjectsByType<LayoutGroup>(FindObjectsSortMode.None).ToList();

        MainLoop(this.GetCancellationTokenOnDestroy()).Forget();
    }

    async UniTask MainLoop(CancellationToken ct) {
        await UniTask.Yield(ct);

        pointer.gameObject.SetActive(false);

        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

        if (!BGMManager.Instance.IsPlaying()) {
            BGMManager.Instance.Play(BGMPath.STAGE_SELECT);
            BGMManager.Instance.FadeIn(1.0f);
        }

        flashStageBtn.onClick += () => selectedStageName = "FlashScene";
        repeatStageBtn.onClick += () => selectedStageName = "RepeatScene";

        flashStageBtn.onSelected += () => {
            pointerTargetPosition = flashStageBtn.rectTransform.anchoredPosition + new Vector2(0.0f, flashStageBtn.rectTransform.sizeDelta.y / 2);
            if (!pointer.gameObject.activeSelf) {
                pointer.anchoredPosition = pointerTargetPosition;
                pointer.gameObject.SetActive(true);
            }
            SEManager.Instance.Play(SEPath.NAVIGATE);
        };
        repeatStageBtn.onSelected += () => {
            pointerTargetPosition = repeatStageBtn.rectTransform.anchoredPosition + new Vector2(0.0f, repeatStageBtn.rectTransform.sizeDelta.y / 2);
            if (!pointer.gameObject.activeSelf) {
                pointer.anchoredPosition = pointerTargetPosition;
                pointer.gameObject.SetActive(true);
            }
            SEManager.Instance.Play(SEPath.NAVIGATE);
        };

        backToTitleBtn.onSelected = () => {
            SEManager.Instance.Play(SEPath.NAVIGATE);
            pointer.gameObject.SetActive(false);
        };

        backToTitleBtn.onClick = () => {
            SEManager.Instance.Play(SEPath.SUBMIT);
            EventSystem.current.SetSelectedGameObject(null);
            SceneLoader.Instance.LoadScene("TitleScene");
        };

        // フェードイン
        await FadeCanvasManager.Instance.FadeIn(1f, ct);

        flashStageBtn.interactable = true;
        repeatStageBtn.interactable = true;

        flashStageBtn.Select();

        while (true) {
            await UniTask.Yield(ct);

            // ステージを選ぶのを待つ
            await UniTask.WaitUntil(() => selectedStageName != string.Empty);

            decideAct += () => stageSelectState = "Decided";
            cancelAct += () => stageSelectState = "Canceled";

            flashStageBtn.interactable = false;
            repeatStageBtn.interactable = false;

            UpdateAllLayoutGroups();

            SEManager.Instance.Play(SEPath.FLIP_PAGE);
            if (selectedStageName == "FlashScene")
                await stageInfoPreviewCGFlash.DOFade(1.0f, 0.5f);
            else if (selectedStageName == "RepeatScene")
                await stageInfoPreviewCGRepeat.DOFade(1.0f, 0.5f);

            // 決定 or キャンセル 待ち
            await UniTask.WaitUntil(() => stageSelectState != string.Empty);
            if (stageSelectState == "Decided") {
                SEManager.Instance.Play(SEPath.SUBMIT);
                await SceneLoader.Instance.LoadScene(selectedStageName, ct);
                break;
            } else if (stageSelectState == "Canceled") {
                SEManager.Instance.Play(SEPath.CANCEL_CLOSE);
                string tempSelectedStageName = selectedStageName;

                selectedStageName = string.Empty;
                stageSelectState = string.Empty;
                decideAct = null;
                cancelAct = null;
                await stageInfoPreviewCGFlash.DOFade(0.0f, 0.5f);
                await stageInfoPreviewCGRepeat.DOFade(0.0f, 0.5f);

                flashStageBtn.interactable = true;
                repeatStageBtn.interactable = true;
                if (tempSelectedStageName == "FlashScene") flashStageBtn.Select(); else repeatStageBtn.Select();

                continue;
            }
        }
    }

    private void Update() {
        pointerTriangle.localPosition = new Vector2(pointerTriangleOffset.x, pointerTriangleOffset.y + Mathf.PingPong(Time.time / pointerUpDownDuration, pointerUpDownRange));

        if (Vector2.Distance(pointerTargetPosition, pointer.position) > 0.01f) {
            pointer.anchoredPosition = Vector2.Lerp(pointer.anchoredPosition, pointerTargetPosition, Time.deltaTime * pointerMoveSpeed);
        } else
            pointer.anchoredPosition = pointerTargetPosition;
    }

    public void OnStageDecide(InputAction.CallbackContext context) {
        if (context.performed)
            decideAct?.Invoke();
    }

    public void OnStageCancel(InputAction.CallbackContext context) {
        if (context.performed)
            cancelAct?.Invoke();
    }

    public void UpdateAllLayoutGroups() {
        foreach (var lg in allLayoutGroups) {
            lg.CalculateLayoutInputHorizontal();
            lg.CalculateLayoutInputVertical();
            lg.SetLayoutHorizontal();
            lg.SetLayoutVertical();
        }
    }
}