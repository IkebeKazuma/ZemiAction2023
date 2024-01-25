using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class RespawnTrigger : MonoBehaviour {

    [SerializeField] SpriteRenderer spriteRenderer;
    public SpriteRenderer SpriteRenderer { get { return spriteRenderer; } }

    BoxCollider2D boxCollider;

    private void Start() {

    }

    BoxCollider2D CheckBoxCollider() {
        if (!boxCollider) TryGetComponent(out boxCollider);
        return boxCollider;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            GameManager.Instance.PlayerRespawn();
        }
    }

    private void OnDrawGizmos() {
#if UNITY_EDITOR
        bool isPrefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject) != null && PrefabUtility.GetPrefabInstanceHandle(gameObject) != null;
        if (isPrefab && CheckBoxCollider() != null) {
            if (boxCollider.offset != Vector2.zero || boxCollider.size != Vector2.one) {
                SerializedObject so = new SerializedObject(boxCollider);
                SerializedProperty spSize = so.FindProperty("m_Size");
                SerializedProperty spOffset = so.FindProperty("m_Offset");
                //PrefabUtility.RevertObjectOverride(boxCollider, InteractionMode.AutomatedAction);
                PrefabUtility.RevertPropertyOverride(spSize, InteractionMode.AutomatedAction);
                PrefabUtility.RevertPropertyOverride(spOffset, InteractionMode.AutomatedAction);
                Debug.LogError("Size, Offset ÇÕïœçXÇ≈Ç´Ç‹ÇπÇÒÅB");
            }
        }
#endif
    }

}