using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class TitleManager : SingletonMonoBehaviour<TitleManager> {

    [SerializeField] UnityEngine.UI.Button startBtn;

    private void Start() {
        MainLoop(this.GetCancellationTokenOnDestroy()).Forget();
    }

    async UniTask MainLoop(CancellationToken ct) {
        await UniTask.Yield(ct);

        bool btnPressed = false;
        startBtn.onClick.AddListener(() => {
            btnPressed = true;
        });

        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: ct);

        // �t�F�[�h�C��
        await FadeCanvasManager.Instance.FadeIn(1f, ct);

        // �X�^�[�g�{�^���I��
        startBtn.interactable = true;
        startBtn.Select();

        await UniTask.WaitUntil(() => btnPressed);

        var nextScene = await SceneLoader.Instance.LoadScene("StageSelectScene", ct);
        nextScene.allowSceneActivation = true;
    }

}