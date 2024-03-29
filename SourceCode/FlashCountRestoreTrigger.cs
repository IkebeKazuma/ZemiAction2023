using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashCountRestoreTrigger : MonoBehaviour {

    [Header("フラッシュ数上書き")]
    [SerializeField] int count = 2;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            StageManager.Instance.flashStageController.RefreshFlash(count);
        }
    }
}