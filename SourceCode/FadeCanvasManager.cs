using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FadeCanvasManager : SingletonMonoBehaviour<FadeCanvasManager> {

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image img;

    public async UniTask FadeIn(float duration, CancellationToken ct, UnityAction onCompletedCallback = null) {
        await UniTask.Yield(ct);
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        await UniTask.Yield(ct);
        await canvasGroup.DOFade(0, duration).ToUniTask(cancellationToken: ct);
        await UniTask.Yield(ct);
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        await UniTask.Yield(ct);
        onCompletedCallback?.Invoke();
        await UniTask.Yield(ct);
    }

    public async UniTask FadeOut(float duration, CancellationToken ct, UnityAction onCompletedCallback = null) {
        await UniTask.Yield(ct);
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = true;
        await UniTask.Yield(ct);
        await canvasGroup.DOFade(1, duration).ToUniTask(cancellationToken: ct);
        await UniTask.Yield(ct);
        onCompletedCallback?.Invoke();
        await UniTask.Yield(ct);
    }
}