using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager> {

    public PlayerInputReceiver PlayerInput;

    [SerializeField] Camera _camera;
    public new Camera camera { get => _camera; }
    [SerializeField] Cinemachine.CinemachineImpulseSource cinemachineImpulseSource;


    [SerializeField] PlayerController _playerController;
    public PlayerController playerController { get => _playerController; }

    private float _frameTime = -1;
    public float frameTime {
        get {
            if (_frameTime < 0 && Application.targetFrameRate > 0) {
                _frameTime = 1 / Application.targetFrameRate;
            }
            return _frameTime;
        }
    }
    public float GetSecondsFromFrame(int frame) {
        return frameTime * frame;
    }

    bool _isRunningHitStop = false;

    [SerializeField] StageManager _stageManager;
    public StageManager stageManager {
        get {
            if (_stageManager == null) {
                _stageManager = FindObjectOfType<StageManager>();
            }
            return _stageManager;
        }
    }

    // リスポーン座標
    Vector3 respawnPoint= Vector3.zero;
    public void OverrideRespawnPoint(Vector3 newPos) { respawnPoint = newPos; }

    void Start() {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        _isRunningHitStop = false;

        respawnPoint = playerController.transform.position;

        MainLoop(this.GetCancellationTokenOnDestroy()).Forget();
    }

    public void PlayerRespawn() {
        // 全てのリスポーントリガーをランダマイズ
        stageManager.RandomizeAllRespawnTriggerActive();

        playerController.transform.position = respawnPoint;
    }

    async UniTask MainLoop(CancellationToken ct) {
        await UniTask.Yield(ct);

        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: ct);

        // フェードイン
        await FadeCanvasManager.Instance.FadeIn(1f, ct);
    }

    public void GenerateHitStop(float power, float duration) {
        if (_isRunningHitStop) return;

        _isRunningHitStop = true;
        DoHitStop(power, duration, this.GetCancellationTokenOnDestroy()).Forget();
    }

    async UniTask DoHitStop(float power, float duration, CancellationToken ct) {
        float defaultTimeScale = Time.timeScale;
        Time.timeScale = power;
        await UniTask.Delay(System.TimeSpan.FromSeconds(duration), ignoreTimeScale: true, cancellationToken: ct);
        Time.timeScale = defaultTimeScale;
        _isRunningHitStop = false;
    }

    public void GenerateCameraShake(Vector3 power) {
        cinemachineImpulseSource.GenerateImpulse(power);
    }
}