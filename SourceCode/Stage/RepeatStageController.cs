using Cysharp.Threading.Tasks;
using KanKikuchi.AudioManager;
using PMP.UnityLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class RepeatStageController : StageControllerBase {

    [SerializeField] GameFinishCanvasController pracStageClearCanvas;

    // 0 : ���K�X�e�[�W
    // 1 : �{�ԃX�e�[�W
    public enum StagePhase {
        None=-1,
        Practice,
        Performance
    }
    public StagePhase currentPhase { get; private set; } = StagePhase.None;

    string clearActionKey = "";

    [Header("��Փx��Tilemap")]
    [SerializeField] StageDataHolder tilemapEasy;
    [SerializeField] StageDataHolder tilemapNormal;
    [SerializeField] StageDataHolder tilemapHard;

    [Header("�w�i�p")]
    [SerializeField] SpriteRenderer bgPrac;
    [SerializeField] SpriteRenderer bgPerform;
    [SerializeField] SpriteRenderer[] hideLayers;
    [SerializeField] SpriteRenderer[] obstacleLayers;

    Vector2 spawnPointDefault;

    Tilemap currentTilemap;

    public void SetClearActionKey(string str) => clearActionKey = str;

    public override async UniTask Initialize(CancellationToken ct) {
        await UniTask.Yield(ct);

        spawnPointDefault = GameManager.Instance.playerCtrl.transform.position;

        currentPhase = StagePhase.Practice;

        // ��Փx�I��
        await manager.SelectStageDifficulty(ct);

        SwitchBG(currentPhase, StageManager.Instance.difficulty);

        // �X�e�[�W�\��
        ShowStage();

        BGMManager.Instance.Play(BGMPath.REPEAT_STAGE_PRAC);
        BGMManager.Instance.FadeIn(1.0f);

        isInitialized = true;
    }

    /// <summary>
    /// ��Փx����X�e�[�W�f�[�^��Ԃ�
    /// </summary>
    public StageDataHolder GetStageDataFromDifficulty(StageManager.StageDifficulty difficulty) {
        return difficulty switch {
            StageManager.StageDifficulty.Easy => tilemapEasy,
            StageManager.StageDifficulty.Normal => tilemapNormal,
            StageManager.StageDifficulty.Hard => tilemapHard,
            _ => null
        };
    }

    void SwitchBG(StagePhase phase, StageManager.StageDifficulty difficulty) {
        bgPrac.gameObject.SetActive(false);
        bgPerform.gameObject.SetActive(false);
        foreach (var item in hideLayers) item.gameObject.SetActive(false);
        foreach (var item in obstacleLayers) item.gameObject.SetActive(false);

        if (phase == StagePhase.Practice) {
            bgPrac.gameObject.SetActive(true);
            switch (difficulty) {
                case StageManager.StageDifficulty.Easy:
                    obstacleLayers[0].gameObject.SetActive(true);
                    break;
                case StageManager.StageDifficulty.Normal:
                    obstacleLayers[1].gameObject.SetActive(true);
                    break;
                case StageManager.StageDifficulty.Hard:
                    obstacleLayers[2].gameObject.SetActive(true);
                    break;
            }
            return;
        }

        if (phase == StagePhase.Performance) {
            bgPerform.gameObject.SetActive(true);
            switch (difficulty) {
                case StageManager.StageDifficulty.Easy:
                    hideLayers[0].gameObject.SetActive(true);
                    break;
                case StageManager.StageDifficulty.Normal:
                    hideLayers[1].gameObject.SetActive(true);
                    break;
                case StageManager.StageDifficulty.Hard:
                    hideLayers[2].gameObject.SetActive(true);
                    break;
            }
        }
    }

    public override Vector2 GetSpawnPosition() {
        var target = GetStageDataFromDifficulty(StageManager.Instance.difficulty);
        return target ? target.GetSpawnPosition() : spawnPointDefault;
    }

    public override void OnPlayerRespawnStart() { }

    public override void ResetStage() { }

    public override void OnPlayerRespawnEnd() { }

    void ShowStage() {

        tilemapEasy.gameObject.SetActive(false);
        tilemapNormal.gameObject.SetActive(false);
        tilemapHard.gameObject.SetActive(false);

        Tilemap targetTilemap = GetStageDataFromDifficulty(StageManager.Instance.difficulty).tilemap;

        if (!targetTilemap) return;

        currentTilemap = targetTilemap;
        currentTilemap.gameObject.SetActive(true);
    }

    public override async UniTask OnStageClear(CancellationToken ct) {
        await UniTask.Yield(ct);

        clearActionKey = "";

        switch (currentPhase) {
            case StagePhase.Practice:
                var gm = GameManager.Instance;

                // �v���C���[�̓������~
                gm.playerCtrl.FreezeMovement();

                // �t�F�[�h�A�E�g
                await FadeCanvasManager.Instance.FadeOut(0.15f, ct);

                await UniTask.Delay(System.TimeSpan.FromSeconds(0.1f), cancellationToken: ct);

                await UniTask.Yield(ct);

                // �v���C���[���Z�b�g
                gm.playerCtrl.ResetPlayerStates();

                await UniTask.Yield(ct);

                // �t�F�[�h�C��
                await FadeCanvasManager.Instance.FadeIn(0.15f, ct);

                await UniTask.Yield(ct);

                await UniTask.Delay(System.TimeSpan.FromSeconds(0.1f), cancellationToken: ct);

                // �v���C���[�𑀍�\��
                gm.playerCtrl.UnfreezeMovement();

                gm.SetIsPlayingState(true);

                break;
            case StagePhase.Performance:
                // �N���A���
                stageClearCanvas.Open();

                BGMManager.Instance.Stop();
                SEManager.Instance.Play(SEPath.GAMECLEAR);
                break;
            default:
                throw new System.ArgumentOutOfRangeException();
        }
    }

    public async UniTask GoPerformPhase(CancellationToken ct) {

        BGMManager.Instance.FadeOut(1.0f);

        // �t�F�[�h
        await FadeCanvasManager.Instance.FadeOut(1.0f, ct);

        await UniTask.Yield(ct);

        GameMenuManager.Instance.CloseMenu();
        pracStageClearCanvas.Close();
        currentPhase = StagePhase.Performance;

        // �X�e�[�W��s����
        currentTilemap.color = currentTilemap.color.SetAlphaAsNew(0.0f);
        currentTilemap.GetComponentsInChildren<RespawnTrigger>().ToList().ForEach(i => i.SpriteRenderer.enabled = false);

        SwitchBG(currentPhase, StageManager.Instance.difficulty);

        // ���j���[�̍\����ύX
        GameMenuManager.Instance.ConfigureMenu();

        // �v���C���[
        GameManager.Instance.playerCtrl.ResetPlayerStates();

        await UniTask.Yield(ct);

        // �ҋ@
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.1f));

        BGMManager.Instance.Play(BGMPath.REPEAT_STAGE_PERFORM);
        BGMManager.Instance.FadeIn(1.0f);

        await FadeCanvasManager.Instance.FadeIn(1.0f, ct);

        await UniTask.Yield(ct);

        GameManager.Instance.SetIsPlayingState(true);

        GameManager.Instance.playerCtrl.UnfreezeMovement();
    }

    /// <summary>
    /// �Q�[���I�[�o�[���̏���
    /// </summary>
    public async UniTask OnGameOver(CancellationToken ct) {
        await UniTask.Yield(ct);

        var gm  = GameManager.Instance;

        gm.InputCtrl.PlayerInput.SwitchCurrentActionMap("UI");
        gm.SetIsPlayingState(false);

        await gm.playerCtrl.Die(ct);

        await UniTask.Yield(ct);

        // ���
        gameOverCanvas.Open();

        BGMManager.Instance.Stop();
        SEManager.Instance.Play(SEPath.GAMEOVER);
    }

    public override async UniTask OnStageTutorial(CancellationToken ct) {
        await UniTask.Yield(ct);

        float inDur = 0.5f;
        UIManager.Instance.ShowTutorial(inDur);
        await UniTask.Delay(System.TimeSpan.FromSeconds(inDur));

        // 0.5�b�͕\�����Ă���
        await UniTask.Delay(System.TimeSpan.FromSeconds(0.5f));

        // �R���g���[���[�̓��͑҂�
        InputAction pressAnyKeyAction = new InputAction(type: InputActionType.PassThrough, binding: "*/<Button>", interactions: "Press");
        pressAnyKeyAction.Enable();

        await UniTask.Yield(ct);

        await UniTask.WaitUntil(() => pressAnyKeyAction.triggered, cancellationToken: ct);

        pressAnyKeyAction.Disable();

        SEManager.Instance.Play(SEPath.SUBMIT);

        float outDur = 0.5f;
        UIManager.Instance.HideTutorial(outDur);
        await UniTask.Delay(System.TimeSpan.FromSeconds(outDur));
    }
}