using PMP.UnityLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnTrigger : MonoBehaviour {

    [SerializeField] bool useRandomActive;
    [SerializeField, Range(0, 100)] float activeProbability = 50;

    public bool UseRandomActive => useRandomActive;

    private void Start() {
        RandomizeActive();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            GameManager.Instance.PlayerRespawn();
        }
    }

    public void RandomizeActive() {
        if (!UseRandomActive) return;
        gameObject.SetActive(RandomUtils.GetFlagFromPercent(activeProbability));
    }

}