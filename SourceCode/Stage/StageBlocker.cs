using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(BoxCollider2D))]
public class StageBlocker : MonoBehaviour {

    [SerializeField, Header("���ύX���Ȃ�")] BoxCollider2D collider2d;

    private void OnDrawGizmos() {

        if (collider2d == null) return;

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
            new Color(1.00f, 0.14f, 0.14f, 0.20f),
            new Color(1.00f, 1.00f, 1.00f, 1.00f));
#endif

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
