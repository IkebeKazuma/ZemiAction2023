using Cinemachine;
using Cysharp.Threading.Tasks;
using KanKikuchi.AudioManager;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class FlashStageController : StageControllerBase {

    [SerializeField] Light2D globalLight;

    [SerializeField] CinemachineBrain cinemachineBrain;
    CinemachineBlendDefinition.Style cashedCinemachineBrainBlendStyle;

    private float currentLightIntensity = 1;

    // ���݂̃X�e�[�W�i�K
    public int currentStagePhase { get; private set; } = -1;
    public void SetCurrentStagePhase(int newState) { currentStagePhase = newState; }

    [Header("�t���b�V���̐ݒ�")]
    [SerializeField] bool useFlashLimit = true;
    [SerializeField, Range(0, 10)] float lightIntensity = 1;
    [SerializeField] float enterDuration = 0;
    float exitDuration = 1f;   // ��������
    [SerializeField] float exitDurationEasy = 3f;
    [SerializeField] float exitDurationNormal = 2f;
    [SerializeField] float exitDurationHard = 1f;
    [SerializeField] int additionalFlashCountEasy;
    [SerializeField] int additionalFlashCountNormal;
    [SerializeField] int additionalFlashCountHard;
    public int currentFlashedCount { get; private set; } = 0;
    public int flashLimitCount { get; private set; } = -1;

    [Header("�X�e�[�W�ړ��ݒ�")]
    [SerializeField] float playerFreezeDurationOnStageChanging = 0.5f;

    // ���݂̃X�e�[�W���X�g�̃C���f�b�N�X
    int currentStageListIndex = -1;

    [Header("�S�ẴX�e�[�W���X�g")]
    [SerializeField, Tooltip("���̃��X�g�̃I�u�W�F�N�g�������_���ɃA�N�e�B�x�[�g�����")]
    List<GameObject> allStageTilemaps = new List<GameObject>();

    [System.Serializable]
    public class StageContainer {
        public Transform container;

        public StageDataHolder stage1Data;
        public StageDataHolder stage2Data;
        public StageDataHolder stage3Data;

        public StageDataHolder GetStageDataHolderFromStagePhase(int sp) {
            switch (sp) {
                case 1: return stage1Data;
                case 2: return stage2Data;
                case 3: return stage3Data;
                default: return null;
            }
        }
    }

    List<StageContainer> stageContainerList;

    public ParticleSystem stageGlowEff;

    public StageContainer GetStageData() {
        return stageContainerList[currentStageListIndex];
    }

    public override async UniTask Initialize(CancellationToken ct) {
        await UniTask.Yield(ct);

        CancelFlash();

        // �X�e�[�W�̃f�[�^���܂Ƃ߂����X�g���쐬���Ă���
        CreateStageContainerList();

        SetCurrentStagePhase(1);

        await UniTask.Delay(10, cancellationToken:ct);

        // ��Փx�I��
        await manager.SelectStageDifficulty(ct);

        BGMManager.Instance.Play(BGMPath.FLASH_STAGE);
        BGMManager.Instance.FadeIn(1.0f);

        RandomizeStage();

        RefreshFlash();

        isInitialized = true;
    }

    private void Update() {
        if (currentLightIntensity > 0.0f) {
            currentLightIntensity = Mathf.Max(currentLightIntensity - (Time.deltaTime / exitDuration), 0.0f);
            ApplyGlobalLightIntensity();
        }
    }

    public void Flash() {
        if (useFlashLimit)
            if (currentFlashedCount >= flashLimitCount) return;

        currentFlashedCount++;

        currentLightIntensity = lightIntensity;
        ApplyGlobalLightIntensity();

        SEManager.Instance.Play(SEPath.AUDIOSTOCK_81019);

        GameManager.Instance.playerCtrl.FreezeMovementTemp(exitDuration);

        UIManager.Instance.UpdateFlashCountStateText();
    }

    public int GetFlashLimitCount() => GetStageData().GetStageDataHolderFromStagePhase(currentStagePhase).flashLimitCount;

    /// <summary>
    /// �t���b�V���񐔂̍Čv�Z
    /// �񐔏���̏㏑������
    /// ���̂܂܂��ƃX�e�[�W�̐��������擾����
    /// </summary>
    public void RefreshFlash(int overrideLimitCount = -1) {
        // �t���b�V���g�p�񐔏�����擾
        int limitCount = overrideLimitCount > 0 ? overrideLimitCount : GetFlashLimitCount();

        // ��Փx�ɂ���ăt���b�V���̒����A�񐔂�ύX
        float targetDuration = 0;
        int additionalCount = 0;

        switch (StageManager.Instance.difficulty) {
            case StageManager.StageDifficulty.Easy:
                targetDuration = exitDurationEasy;
                additionalCount = additionalFlashCountEasy;
                break;
            case StageManager.StageDifficulty.Normal:
                targetDuration = exitDurationNormal;
                additionalCount = additionalFlashCountNormal;
                break;
            case StageManager.StageDifficulty.Hard:
                targetDuration = exitDurationHard;
                additionalCount = additionalFlashCountHard;
                break;
        }

        exitDuration = targetDuration;
        flashLimitCount = Mathf.Max(limitCount + additionalCount, 1);
        currentFlashedCount = 0;

        UIManager.Instance.UpdateFlashCountStateText();
    }

    /// <summary>
    /// �t���b�V���p���C�g�𑦍��ɈÓ]����
    /// </summary>
    void CancelFlash() {
        currentLightIntensity = 0;
        ApplyGlobalLightIntensity();
    }

    void ApplyGlobalLightIntensity() {
        globalLight.intensity = currentLightIntensity;
    }

    void CreateStageContainerList() {
        stageContainerList = new List<StageContainer>();
        for (int i = 0; i < allStageTilemaps.Count; i++) {
            Transform b = allStageTilemaps[i].transform;
            var data = new StageContainer();
            var trns = b.transform;
            data.container = trns;
            trns.GetChild(0).TryGetComponent<StageDataHolder>(out data.stage1Data);
            trns.GetChild(1).TryGetComponent<StageDataHolder>(out data.stage2Data);
            trns.GetChild(2).TryGetComponent<StageDataHolder>(out data.stage3Data);
            stageContainerList.Add(data);

            // RespawnUpdater�ɃX�e�[�W�ړ��C���f�b�N�X�����蓖�Ă�
            void SetStageTransMode(StageDataHolder data) {
                // RespawnUpdater�̃��X�g
                var targets = data.transform.parent.GetComponentsInChildren<RespawnUpdater>().ToList();
                if (targets != null && targets.Count > 0) {
                    float centerX = targets[0].transform.position.x + targets[1].transform.position.x;
                    if (targets[0].transform.position.x < centerX) {
                        targets[0].SetStageTransMode(RespawnUpdater.StageTransMode.To2);
                        targets[1].SetStageTransMode(RespawnUpdater.StageTransMode.To3);
                    } else {
                        targets[1].SetStageTransMode(RespawnUpdater.StageTransMode.To2);
                        targets[0].SetStageTransMode(RespawnUpdater.StageTransMode.To3);
                    }
                } else {
                    Debug.Log("Respawn Updater �̎擾�Ɏ��s");
                }
            }

            SetStageTransMode(data.stage1Data);
            SetStageTransMode(data.stage2Data);
            SetStageTransMode(data.stage3Data);
        }
    }

    /// <summary>
    /// �X�e�[�W�������_���ɃA�N�e�B�x�[�g����B
    /// ����ȊO�͔�A�N�e�B�u�ɁB
    /// </summary>
    public void RandomizeStage() {
        GameObject activeObj = allStageTilemaps.Where(i => i.activeSelf == true).FirstOrDefault();
        int currentActiveIndex = activeObj ? allStageTilemaps.IndexOf(activeObj) : -1;
        int randomizedIndex = UnityEngine.Random.Range(0, allStageTilemaps.Count);

        // ���݃A�N�e�B�u�̃X�e�[�W�ƃC���f�b�N�X�������Ȃ�I�тȂ���
        while (allStageTilemaps != null && allStageTilemaps.Count > 1 && currentActiveIndex == randomizedIndex)
            randomizedIndex = UnityEngine.Random.Range(0, allStageTilemaps.Count);

        for (int i = 0; i < allStageTilemaps.Count; i++) {
            var target = allStageTilemaps[i];
            if (target == null) continue;

            if (i == randomizedIndex) {
                target.gameObject.SetActive(true);
                currentStageListIndex = i;
                continue;
            } else {
                if (target.gameObject.activeSelf) target.gameObject.SetActive(false);
            }
        }

        stageGlowEff.Stop();
        stageGlowEff.Play();
    }

    /// <summary>
    /// �X�e�[�W�J�ڎ�
    /// </summary>
    public void MoveStageToNext(int nextIndex) {
        // �X�e�[�W����ύX
        SetCurrentStagePhase(nextIndex);

        // �t���b�V�����Ó]
        CancelFlash();

        // �t���b�V���񐔂����Z�b�g
        RefreshFlash();

        // �X�e�[�W���\��
        UIManager.Instance.Invoke("DisplayCurrentStagePhase", 1f);
    }

    public override Vector2 GetSpawnPosition() {
        // �X�e�[�W�f�[�^
        var data = GetStageData().GetStageDataHolderFromStagePhase(currentStagePhase);
        return data.spawnPoint.position;
    }

    public override void OnPlayerRespawnStart() {
        // �t���b�V�����Ó]
        CancelFlash();
        cashedCinemachineBrainBlendStyle = cinemachineBrain.m_DefaultBlend.m_Style;
        cinemachineBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
    }

    public override void ResetStage() {
        // �X�e�[�W�������_����
        RandomizeStage();

        // �t���b�V�������Z�b�g
        RefreshFlash();

        UIManager.Instance.HideCurrentStagePhase();
    }

    public override void OnPlayerRespawnEnd() {
        cinemachineBrain.m_DefaultBlend.m_Style = cashedCinemachineBrainBlendStyle;

        // �X�e�[�W��\��
        UIManager.Instance.DisplayCurrentStagePhase();
    }

    public override async UniTask OnStageClear(CancellationToken ct) {
        await UniTask.Yield(ct);

        // SceneLoader.Instance.LoadScene("StageSelectScene");
        stageClearCanvas.Open();

        BGMManager.Instance.Stop();
        SEManager.Instance.Play(SEPath.GAMECLEAR);
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