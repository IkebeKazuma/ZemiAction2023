using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public abstract class StageControllerBase : MonoBehaviour {

    // �������ς݂�
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
    /// StageController�����������B
    /// StageManager����R�[�������
    /// </summary>
    public abstract UniTask Initialize(CancellationToken ct);

    /// <summary>
    /// �v���C���[�̃��X�|�[���ʒu��Ԃ�����
    /// </summary>
    public abstract Vector2 GetSpawnPosition();

    /// <summary>
    /// �v���C���[�����X�|�[������Ƃ��̏���
    /// </summary>
    public abstract void OnPlayerRespawnStart();

    /// <summary>
    /// �X�e�[�W���Z�b�g����
    /// </summary>
    public abstract void ResetStage();

    /// <summary>
    /// �X�e�[�W�`���[�g���A������
    /// </summary>
    public abstract UniTask OnStageTutorial(CancellationToken ct);

    /// <summary>
    /// �X�e�[�W���X�|�[���I�����̏���
    /// </summary>
    public abstract void OnPlayerRespawnEnd();

    /// <summary>
    /// �v���C���[�S�[�����̏���
    /// </summary>
    public abstract UniTask OnStageClear(CancellationToken ct);
}