using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public abstract class StageControllerBase : MonoBehaviour {

    // 初期化済みか
    public bool isInitialized { get; protected set; } = false;

    StageManager _manager;
    public StageManager manager {
        get {
            if (_manager == null) {
                _manager = StageManager.Instance;
            }
            return _manager;
        }
    }

    [Header("Canvas")]
    [SerializeField] protected GameFinishCanvasController stageClearCanvas;
    [SerializeField] protected GameFinishCanvasController gameOverCanvas;

    /// <summary>
    /// StageController初期化処理。
    /// StageManagerからコールされる
    /// </summary>
    public abstract UniTask Initialize(CancellationToken ct);

    /// <summary>
    /// プレイヤーのリスポーン位置を返す処理
    /// </summary>
    public abstract Vector2 GetSpawnPosition();

    /// <summary>
    /// プレイヤーがリスポーンするときの処理
    /// </summary>
    public abstract void OnPlayerRespawnStart();

    /// <summary>
    /// ステージリセット処理
    /// </summary>
    public abstract void ResetStage();

    /// <summary>
    /// ステージチュートリアル処理
    /// </summary>
    public abstract UniTask OnStageTutorial(CancellationToken ct);

    /// <summary>
    /// ステージリスポーン終了時の処理
    /// </summary>
    public abstract void OnPlayerRespawnEnd();

    /// <summary>
    /// プレイヤーゴール時の処理
    /// </summary>
    public abstract UniTask OnStageClear(CancellationToken ct);
}