using Cysharp.Threading.Tasks;
using KanKikuchi.AudioManager;
using System;
using System.Threading;
using UnityEngine.InputSystem;

public class TitleManager : SingletonMonoBehaviour<TitleManager> {

    private void Start() {
        // 初期化
        Initialize();

        MainLoop(this.GetCancellationTokenOnDestroy()).Forget();
    }

    async UniTask MainLoop(CancellationToken ct) {
        await UniTask.Yield(ct);

        await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: ct);

        // フェードイン
        await FadeCanvasManager.Instance.FadeIn(1f, ct);

        if (!BGMManager.Instance.IsPlaying()) {
            BGMManager.Instance.Play(BGMPath.STAGE_SELECT);
            BGMManager.Instance.FadeIn(1.0f);
        }

        await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: ct);

        // キーアクションを生成
        InputAction pressAnyKeyAction = new InputAction(type: InputActionType.PassThrough, binding: "*/<Button>", interactions: "Press");

        pressAnyKeyAction.Enable();

        await UniTask.Yield(ct);

        await UniTask.WaitUntil(() => pressAnyKeyAction.triggered, cancellationToken: ct);

        pressAnyKeyAction.Disable();

        SEManager.Instance.Play(SEPath.SUBMIT);

        await SceneLoader.Instance.LoadScene("StageSelectScene", ct);
    }

    void Initialize() { }
}