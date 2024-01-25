using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class SceneChangeTrigger : MonoBehaviour {

    bool called = false;

    [SerializeField] private string sceneName = "StageSelectScene";

    void Start() {
        called = false;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (called) return;

        if (collision.gameObject.CompareTag("Player")) {
            called = true;
            SceneLoader.Instance.LoadScene(sceneName);
        }
    }
}