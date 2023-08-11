using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class StageSelectManager : MonoBehaviour {

    private string selectedStageName = string.Empty;

    [SerializeField] List<Button> buttonList = new List<Button>();

    private void Start() {
        selectedStageName = string.Empty;

        MainLoop(this.GetCancellationTokenOnDestroy()).Forget();
    }

    async UniTask MainLoop(CancellationToken ct) {
        await UniTask.Yield(ct);

        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

        // フェードイン
        await FadeCanvasManager.Instance.FadeIn(1f, ct);

        buttonList.ForEach(i => {
            i.interactable = true;
        });
        buttonList[0].Select();

        await UniTask.WaitUntil(() => selectedStageName != string.Empty);

        var nextScene = await SceneLoader.Instance.LoadScene(selectedStageName, ct);
        nextScene.allowSceneActivation = true;
    }

    public void OnClick(string str) {
        selectedStageName = str;
    }
}