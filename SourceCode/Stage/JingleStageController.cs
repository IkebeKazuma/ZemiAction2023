using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class JingleStageController : StageControllerBase {

    public override async UniTask Initialize(CancellationToken ct) {
        await UniTask.Yield(ct);

        // process

        isInitialized = true;
    }

    public override Vector2 GetSpawnPosition() {
        return new Vector2();
    }

    public override void OnPlayerRespawnEnd() { }

    public override void OnPlayerRespawnStart() { }

    public override void ResetStage() { }

    public override async UniTask OnStageClear(CancellationToken ct) {
        await UniTask.Yield(ct);
    }

    public override async UniTask OnStageTutorial(CancellationToken ct) {
        await UniTask.Yield(ct);

        Debug.Log("ジングル チュートリアル開始");
        Debug.Log("１秒待機");

        await UniTask.Delay(System.TimeSpan.FromSeconds(1f));

        Debug.Log("チュートリアル終了");
    }
}