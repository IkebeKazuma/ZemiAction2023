using Cysharp.Threading.Tasks;
using KanKikuchi.AudioManager;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneLoader : SingletonMonoBehaviour<SceneLoader> {

    [SerializeField] string loadSceneName = "LoadScene";

    AsyncOperation loadScene;
    AsyncOperation nextScene;

    bool isRunning = false;

    public void LoadScene(string nextSceneName, UnityAction initMethod = null) {
        if (isRunning) return;

        LoadScene(nextSceneName, this.GetCancellationTokenOnDestroy(), initMethod).Forget();
    }

    public async UniTask LoadScene(string nextSceneName, CancellationToken ct, UnityAction initMethod = null) {

        isRunning = true;

        var currentSceneName = SceneManager.GetActiveScene().name;

        bool titleToStageSelect = currentSceneName == "TitleScene" && nextSceneName == "StageSelectScene";
        bool stageSelectToTitle = currentSceneName == "StageSelectScene" && nextSceneName == "TitleScene";
        if(!titleToStageSelect && !stageSelectToTitle) {
            BGMManager.Instance.FadeOut(1f);
        }
        SEManager.Instance.FadeOut(1f);

        // 暗転
        await FadeCanvasManager.Instance.FadeOut(1.0f, ct, () => {
            try { GameMenuManager.Instance.CloseMenu(0); } catch { }
        });

        // Loading のシーンを事前読み込み
        loadScene = SceneManager.LoadSceneAsync(loadSceneName, LoadSceneMode.Additive);
        loadScene.allowSceneActivation = false;

        await UniTask.WaitUntil(() => loadScene.progress >= 0.9f);

        await UniTask.Delay(TimeSpan.FromSeconds(0.05f), cancellationToken: ct);

        await UniTask.Yield(ct);

        loadScene.allowSceneActivation = true;

        await UniTask.Yield(ct);

        await FadeCanvasManager.Instance.FadeIn(0.25f, ct);

        // シーンロード
        nextScene = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
        nextScene.allowSceneActivation = false;

        await UniTask.WaitUntil(() => nextScene.progress >= 0.9f);

        await UniTask.Yield(ct);

        await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: ct);

        await FadeCanvasManager.Instance.FadeOut(1.0f, ct);

        await UniTask.Yield(ct);

        nextScene.allowSceneActivation = true;

        isRunning = false;
    }
}