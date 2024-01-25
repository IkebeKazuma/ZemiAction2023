using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class StageManager : SingletonMonoBehaviour<StageManager> {

    public enum StageType {
        None = -1,
        FlashStage,
        RepeatStage,
        JingleStage
    }
    [Header("ステージタイプ")]
    [SerializeField] StageType _stageType;
    public StageType stageType { get { return _stageType; } }

    public StageType GetStageType() { return _stageType; }

    public bool CompareStageType(StageType type) { return _stageType == type; }

    //[Header("ステージ難易度の設定")]
    public StageDifficulty difficulty { get; private set; } = StageDifficulty.None;
    public enum StageDifficulty {
        None = -1,
        Easy,
        Normal,
        Hard
    }

    /// <summary>
    /// 難易度を変更
    /// </summary>
    public StageDifficulty ChangeDifficulty(StageDifficulty newDifficulty) {
        difficulty = newDifficulty;
        return difficulty;
    }

    /// <summary>
    /// StageControllerが初期化済みか
    /// </summary>
    public bool GetControllerInitializedState() {
        return stageType switch {
            StageType.FlashStage => flashStageController.isInitialized,
            StageType.RepeatStage => repeatStageController.isInitialized,
            StageType.JingleStage => jingleStageController.isInitialized,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    [Header("Controllers")]
    [SerializeField] FlashStageController _flashStageController;
    public FlashStageController flashStageController => _flashStageController;

    [SerializeField] RepeatStageController _repeatStageController;
    public RepeatStageController repeatStageController => _repeatStageController;

    [SerializeField] JingleStageController _jingleStageController;
    public JingleStageController jingleStageController => _jingleStageController;

    public StageControllerBase GetCurrentStageController() {
        return stageType switch {
            StageType.FlashStage => flashStageController,
            StageType.RepeatStage => repeatStageController,
            StageType.JingleStage => jingleStageController,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public UnityAction OnPlayerRespawnStartEvent { get; set; }
    public UnityAction StageResetEvent { get; set; }
    public UnityAction OnPlayerRespawnEndEvent { get; set; }

    public UnityAction OnGoalEvent;

    public async UniTask Initialize(CancellationToken ct) {
        // イベント登録
        OnPlayerRespawnStartEvent = GetActionOnPlayerRespawnStart();
        StageResetEvent = GetActionStageReset();
        OnPlayerRespawnEndEvent = GetActionOnPlayerRespawnEnd();
        // ゴール時共通処理
        OnGoalEvent = () => {
            GameManager.Instance.SetIsPlayingState(false);
            GameManager.Instance.playerCtrl.FreezeMovement();
            GameManager.Instance.InputCtrl.PlayerInput.SwitchCurrentActionMap("UI");
        };
        OnGoalEvent += GetActionOnStageClear();

        // 各ステージの初期化処理
        await InitializeStageController(ct);
    }

    async UniTask InitializeStageController(CancellationToken ct) {
        // Task
        UniTask method;
        switch (stageType) {
            case StageType.FlashStage:
                method = flashStageController.Initialize(ct);
                break;
            case StageType.RepeatStage:
                method = repeatStageController.Initialize(ct);
                break;
            case StageType.JingleStage:
                method = jingleStageController.Initialize(ct);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        await method;
    }

    UnityAction GetActionOnPlayerRespawnStart() {
        return stageType switch {
            StageType.FlashStage => flashStageController.OnPlayerRespawnStart,
            StageType.RepeatStage => repeatStageController.OnPlayerRespawnStart,
            StageType.JingleStage => jingleStageController.OnPlayerRespawnStart,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    UnityAction GetActionStageReset() {
        return stageType switch {
            StageType.FlashStage => flashStageController.ResetStage,
            StageType.RepeatStage => repeatStageController.ResetStage,
            StageType.JingleStage => jingleStageController.ResetStage,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    UnityAction GetActionOnPlayerRespawnEnd() {
        return stageType switch {
            StageType.FlashStage => flashStageController.OnPlayerRespawnEnd,
            StageType.RepeatStage => repeatStageController.OnPlayerRespawnEnd,
            StageType.JingleStage => jingleStageController.OnPlayerRespawnEnd,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public Vector2 GetSpawnPointFromController() {
        return stageType switch {
            StageType.FlashStage => flashStageController.GetSpawnPosition(),
            StageType.RepeatStage => repeatStageController.GetSpawnPosition(),
            StageType.JingleStage => jingleStageController.GetSpawnPosition(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    UnityAction GetActionOnStageClear() {
        var ct = this.GetCancellationTokenOnDestroy();
        return stageType switch {
            StageType.FlashStage => () => { flashStageController.OnStageClear(ct).Forget(); },
            StageType.RepeatStage => () => { repeatStageController.OnStageClear(ct).Forget(); },
            StageType.JingleStage => () => { jingleStageController.OnStageClear(ct).Forget(); },
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    /// 難易度選択を待機する
    /// </summary>
    public async UniTask SelectStageDifficulty(CancellationToken ct) {
        await UniTask.Yield(ct);

        // 難易度をリセット
        difficulty = StageDifficulty.None;

        // 表示
        UIManager.Instance.difficultySelectCtrl.Open(0.5f);

        GameManager.Instance.InputCtrl.PlayerInput.SwitchCurrentActionMap("UI");

        // 難易度が選択されるまで待機
        await UniTask.WaitUntil(() => difficulty != StageDifficulty.None, cancellationToken: ct);

        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
    }
}