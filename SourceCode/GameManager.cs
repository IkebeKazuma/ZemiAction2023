using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using DG.Tweening;

public class GameManager : SingletonMonoBehaviour<GameManager> {

    public bool isPlaying { get; set; }

    public PlayerInputReceiver InputCtrl;

    [SerializeField] Camera _camera;
    public new Camera camera { get => _camera; }
    [SerializeField] Cinemachine.CinemachineImpulseSource cinemachineImpulseSource;

    [SerializeField] PlayerController _playerController;
    public PlayerController playerCtrl { get => _playerController; }

    // リスポーン
    public bool isRespawning { get; private set; } = false;

    [SerializeField] Material pageImgShader;

    void Start() {
        // QualitySettings.vSyncCount = 0;
        // Application.targetFrameRate = 60;

        pageImgShader.DOFloat(1, "_Flip", 0);

        MainLoop(this.GetCancellationTokenOnDestroy()).Forget();
    }

    async UniTask MainLoop(CancellationToken ct) {
        await UniTask.Yield(ct);

        var gameMenuManager = GameMenuManager.Instance;
        var stageManager = StageManager.Instance;

        // プレイヤー初期化
        playerCtrl.Initialize();

        await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: ct);

        // StageManager 初期化
        await stageManager.Initialize(ct);

        playerCtrl.ResetPlayerStates();

        await UniTask.Yield(ct);

        // フェードイン
        await FadeCanvasManager.Instance.FadeIn(1f, ct);

        await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: ct);

        // チュートリアル
        await stageManager.GetCurrentStageController().OnStageTutorial(ct);

        // メニュー機能開放
        gameMenuManager.Unlock();

        // プレイヤーを操作可能に
        playerCtrl.UnfreezeMovement();

        SetIsPlayingState(true);

        if (stageManager.stageType == StageManager.StageType.FlashStage) {
            // ステージ数表示
            UIManager.Instance.DisplayCurrentStagePhase();
        }

        while (true) {
            await UniTask.Yield(ct);

            if (!isPlaying) continue;

            // メニュー入力
            if (InputCtrl.start) {
                InputCtrl.StartInput(false);

                if (gameMenuManager.GetLockState()) continue;

                gameMenuManager.SetMenuShowingState(null);
            }

            // 自滅入力
            if (InputCtrl.restart) {
                InputCtrl.RestartInput(false);

                // リピートステージ本番はスルー
                if (stageManager.CompareStageType(StageManager.StageType.RepeatStage) && stageManager.repeatStageController.currentPhase == RepeatStageController.StagePhase.Performance) continue;

                PlayerRespawn();
            }
        }
    }

    public void SetIsPlayingState(bool newState) {
        isPlaying = newState;
    }

    public void PlayerRespawn() {
        // リピートステージの場合
        var sm = StageManager.Instance;
        if (sm.CompareStageType(StageManager.StageType.RepeatStage)) {
            if (sm.repeatStageController.currentPhase ==  RepeatStageController.StagePhase.Performance) {   // 本番ステージ
                sm.repeatStageController.OnGameOver(this.GetCancellationTokenOnDestroy()).Forget();
                return;
            }
        }

        if (isRespawning) return;

        SetIsPlayingState(false);

        isRespawning = true;
        PlayerRespawnTask(this.GetCancellationTokenOnDestroy()).Forget();
    }

    async UniTask PlayerRespawnTask(CancellationToken ct) {

        StageManager stageManager = StageManager.Instance;
        GameMenuManager gameMenu = GameMenuManager.Instance;

        // メニューを閉じる
        gameMenu.CloseMenu(0);
        gameMenu.Lock();

        await UniTask.Yield(ct);

        // 各ステージのプレイヤーリスポーン時の処理を実行
        stageManager.OnPlayerRespawnStartEvent?.Invoke();

        // プレイヤー死亡処理（待機時間）
        await playerCtrl.Die(ct, 0.5f);

        // フェードアウト
        await FadeCanvasManager.Instance.FadeOut(0.2f, ct);

        await UniTask.Yield(ct);

        // 各ステージのリセット
        stageManager.StageResetEvent?.Invoke();

        // プレイヤーを移動
        playerCtrl.MoveToRespawnPoint();

        await UniTask.Yield(ct);

        // 死後硬直
        float bindTime = 1.0f;
        if (stageManager.GetStageType() == StageManager.StageType.FlashStage) {
            bindTime = 1.0f;
        } else if (stageManager.GetStageType() == StageManager.StageType.RepeatStage) {
            bindTime = 0.25f;
        }

        // 待機
        await UniTask.Delay(TimeSpan.FromSeconds(0.1f));

        // フェードイン
        await FadeCanvasManager.Instance.FadeIn(0.2f, ct);

        // プレイヤー復活
        await playerCtrl.RecoverFromDeath(ct, bindTime);

        await UniTask.Yield(ct);

        stageManager.OnPlayerRespawnEndEvent?.Invoke();

        // メニュー機能開放
        gameMenu.Unlock();

        await UniTask.Yield(ct);

        isRespawning = false;

        SetIsPlayingState(true);
    }

    public void GenerateCameraShake(Vector3 power) {
        cinemachineImpulseSource.GenerateImpulse(power);
    }
}