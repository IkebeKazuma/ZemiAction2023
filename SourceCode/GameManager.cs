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

    // ���X�|�[��
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

        // �v���C���[������
        playerCtrl.Initialize();

        await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: ct);

        // StageManager ������
        await stageManager.Initialize(ct);

        playerCtrl.ResetPlayerStates();

        await UniTask.Yield(ct);

        // �t�F�[�h�C��
        await FadeCanvasManager.Instance.FadeIn(1f, ct);

        await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: ct);

        // �`���[�g���A��
        await stageManager.GetCurrentStageController().OnStageTutorial(ct);

        // ���j���[�@�\�J��
        gameMenuManager.Unlock();

        // �v���C���[�𑀍�\��
        playerCtrl.UnfreezeMovement();

        SetIsPlayingState(true);

        if (stageManager.stageType == StageManager.StageType.FlashStage) {
            // �X�e�[�W���\��
            UIManager.Instance.DisplayCurrentStagePhase();
        }

        while (true) {
            await UniTask.Yield(ct);

            if (!isPlaying) continue;

            // ���j���[����
            if (InputCtrl.start) {
                InputCtrl.StartInput(false);

                if (gameMenuManager.GetLockState()) continue;

                gameMenuManager.SetMenuShowingState(null);
            }

            // ���œ���
            if (InputCtrl.restart) {
                InputCtrl.RestartInput(false);

                // ���s�[�g�X�e�[�W�{�Ԃ̓X���[
                if (stageManager.CompareStageType(StageManager.StageType.RepeatStage) && stageManager.repeatStageController.currentPhase == RepeatStageController.StagePhase.Performance) continue;

                PlayerRespawn();
            }
        }
    }

    public void SetIsPlayingState(bool newState) {
        isPlaying = newState;
    }

    public void PlayerRespawn() {
        // ���s�[�g�X�e�[�W�̏ꍇ
        var sm = StageManager.Instance;
        if (sm.CompareStageType(StageManager.StageType.RepeatStage)) {
            if (sm.repeatStageController.currentPhase ==  RepeatStageController.StagePhase.Performance) {   // �{�ԃX�e�[�W
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

        // ���j���[�����
        gameMenu.CloseMenu(0);
        gameMenu.Lock();

        await UniTask.Yield(ct);

        // �e�X�e�[�W�̃v���C���[���X�|�[�����̏��������s
        stageManager.OnPlayerRespawnStartEvent?.Invoke();

        // �v���C���[���S�����i�ҋ@���ԁj
        await playerCtrl.Die(ct, 0.5f);

        // �t�F�[�h�A�E�g
        await FadeCanvasManager.Instance.FadeOut(0.2f, ct);

        await UniTask.Yield(ct);

        // �e�X�e�[�W�̃��Z�b�g
        stageManager.StageResetEvent?.Invoke();

        // �v���C���[���ړ�
        playerCtrl.MoveToRespawnPoint();

        await UniTask.Yield(ct);

        // ����d��
        float bindTime = 1.0f;
        if (stageManager.GetStageType() == StageManager.StageType.FlashStage) {
            bindTime = 1.0f;
        } else if (stageManager.GetStageType() == StageManager.StageType.RepeatStage) {
            bindTime = 0.25f;
        }

        // �ҋ@
        await UniTask.Delay(TimeSpan.FromSeconds(0.1f));

        // �t�F�[�h�C��
        await FadeCanvasManager.Instance.FadeIn(0.2f, ct);

        // �v���C���[����
        await playerCtrl.RecoverFromDeath(ct, bindTime);

        await UniTask.Yield(ct);

        stageManager.OnPlayerRespawnEndEvent?.Invoke();

        // ���j���[�@�\�J��
        gameMenu.Unlock();

        await UniTask.Yield(ct);

        isRespawning = false;

        SetIsPlayingState(true);
    }

    public void GenerateCameraShake(Vector3 power) {
        cinemachineImpulseSource.GenerateImpulse(power);
    }
}