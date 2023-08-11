using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class FadeCanvasManager : SingletonMonoBehaviour<FadeCanvasManager> {

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image img;

    public async UniTask FadeIn(float duration, CancellationToken ct) {
        await UniTask.Yield(ct);
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        await canvasGroup.DOFade(0, duration).ToUniTask(cancellationToken: ct);
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        await UniTask.WaitUntil(() => canvasGroup.alpha == 0 && canvasGroup.blocksRaycasts == false, cancellationToken: ct);
    }

    public async UniTask FadeOut(float duration, CancellationToken ct) {
        await UniTask.Yield(ct);
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = true;
        await canvasGroup.DOFade(1, duration).ToUniTask(cancellationToken: ct);
        await UniTask.WaitUntil(() => canvasGroup.alpha == 1 && canvasGroup.blocksRaycasts == true, cancellationToken: ct);
    }
}