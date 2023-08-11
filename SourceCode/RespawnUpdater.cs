using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RespawnUpdater : MonoBehaviour {

    [SerializeField] Transform positionOverrider;

    private void Start() {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            var pos = positionOverrider != null ? positionOverrider.position : transform.position;
            GameManager.Instance.OverrideRespawnPoint(pos);
        }
    }

    private void OnDrawGizmos() {
        var pos = positionOverrider != null ? positionOverrider.position : transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(pos, 0.2f);
    }

}