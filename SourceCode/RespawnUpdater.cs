using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(BoxCollider2D))]
public class RespawnUpdater : MonoBehaviour {

    [SerializeField] Transform target;

    [SerializeField, Header("���ύX���Ȃ�")] BoxCollider2D collider2d;

    public enum StageTransMode {
        None = -1,
        To2,
        To3
    }

    StageTransMode stageTransMode = 0;

    [Header("��x�̂ݔ���")]
    [SerializeField] bool once = true;
    bool reacted = false;

    public void SetStageTransMode(StageTransMode stageTransMode) {
        this.stageTransMode = stageTransMode;
    }

    private void Start() {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (once && reacted) return;

        if (collision.gameObject.CompareTag("Player")) {
            int newStageIndex = -1;
            // �X�e�[�W�̈ړ����J�E���g
            switch (stageTransMode) {
                case StageTransMode.To2:
                    newStageIndex = 2;
                    break;
                case StageTransMode.To3:
                    newStageIndex = 3;
                    break;
            }
            if (newStageIndex == -1) return;

            // �X�e�[�W�ړ�
            StageManager.Instance.flashStageController.MoveStageToNext(newStageIndex);

            if (once) reacted = true;
        }
    }

    private void OnDrawGizmos() {
        if (!target) {
            Debug.LogError($"RespawnUpdater�Ƀ^�[�Q�b�g���ݒ肳��Ă��܂���B\n���X�|�[���n�_�̍X�V�͍s���܂���B\n�I�u�W�F�N�g���F{gameObject.name}");
            return;
        } else {
            Gizmos.color = Color.red;

            if (!collider2d) {
                Debug.LogError($"BoxCollider2D���ݒ肳��Ă��܂���B\n�I�u�W�F�N�g���F{gameObject.name}");
                return;
            }

#if UNITY_EDITOR
            var origin = (Vector2)transform.position;
            var range = (Vector2)collider2d.size / 2;
            Handles.DrawSolidRectangleWithOutline(
                new Vector3[] {
                    new Vector3(origin.x - range.x, origin.y - range.y, 0),
                    new Vector3(origin.x - range.x, origin.y + range.y, 0),
                    new Vector3(origin.x + range.x, origin.y + range.y, 0),
                    new Vector3(origin.x + range.x, origin.y - range.y, 0),
                },
                new Color(0.40f, 0.90f, 0.53f, 0.20f),
                new Color(1.00f, 1.00f, 1.00f, 1.00f));
#endif
            Gizmos.DrawLine(transform.position, target.position);
        }

        if (collider2d.offset != Vector2.zero) {
            Debug.LogError($"Offset�͕ύX�ł��܂���BPosition��ύX���Ă��������B");
            collider2d.offset = Vector2.zero;
        }

        if (transform.localScale != Vector3.one) {
            Debug.LogError($"Scale�͕ύX�ł��܂���BSize��ύX���Ă��������B");
            transform.localScale = Vector3.one;
        }
    }

}