using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class GoalTrigger : MonoBehaviour {

    BoxCollider2D boxCollider;

    // Start is called before the first frame update
    void Start() {
        if(TryGetComponent(out boxCollider)) {
            boxCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            StageManager.Instance.OnGoalEvent?.Invoke();
        }
    }
}