using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(BoxCollider2D))]
public class RespawnUpdater : MonoBehaviour {

    [SerializeField] Transform target;

    [SerializeField, Header("↓変更しない")] BoxCollider2D collider2d;

    public enum StageTransMode {
        None = -1,
        To2,
        To3
    }

    StageTransMode stageTransMode = 0;

    [Header("一度のみ反応")]
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
            // ステージの移動をカウント
            switch (stageTransMode) {
                case StageTransMode.To2:
                    newStageIndex = 2;
                    break;
                case StageTransMode.To3:
                    newStageIndex = 3;
                    break;
            }
            if (newStageIndex == -1) return;

            // ステージ移動
            StageManager.Instance.flashStageController.MoveStageToNext(newStageIndex);

            if (once) reacted = true;
        }
    }

    private void OnDrawGizmos() {
        if (!target) {
            Debug.LogError($"RespawnUpdaterにターゲットが設定されていません。\nリスポーン地点の更新は行われません。\nオブジェクト名：{gameObject.name}");
            return;
        } else {
            Gizmos.color = Color.red;

            if (!collider2d) {
                Debug.LogError($"BoxCollider2Dが設定されていません。\nオブジェクト名：{gameObject.name}");
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
            Debug.LogError($"Offsetは変更できません。Positionを変更してください。");
            collider2d.offset = Vector2.zero;
        }

        if (transform.localScale != Vector3.one) {
            Debug.LogError($"Scaleは変更できません。Sizeを変更してください。");
            transform.localScale = Vector3.one;
        }
    }

}