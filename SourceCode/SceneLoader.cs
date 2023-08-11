using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : SingletonMonoBehaviour<SceneLoader> {

    [SerializeField] string loadSceneName = "LoadScene";

    AsyncOperation loadScene;
    AsyncOperation nextScene;

    public async UniTask<AsyncOperation> LoadScene(string nextSceneName, CancellationToken ct) {

        // 一秒かけて暗転
        await FadeCanvasManager.Instance.FadeOut(1f, ct);

        // Loading のシーンを事前読み込み
        loadScene = SceneManager.LoadSceneAsync(loadSceneName, LoadSceneMode.Additive);
        loadScene.allowSceneActivation = false;

        await UniTask.WaitUntil(() => loadScene.progress >= 0.9f);

        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: ct);

        await UniTask.Yield(ct);

        loadScene.allowSceneActivation = true;

        await UniTask.Yield(ct);

        await FadeCanvasManager.Instance.FadeIn(1f, ct);

        // シーンロード
        nextScene = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
        nextScene.allowSceneActivation = false;

        await UniTask.WaitUntil(() => nextScene.progress >= 0.9f);

        await UniTask.Yield(ct);

        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: ct);

        await FadeCanvasManager.Instance.FadeOut(1f, ct);

        await UniTask.Yield(ct);

        // nextScene.allowSceneActivation = true;
        return nextScene;
    }
}