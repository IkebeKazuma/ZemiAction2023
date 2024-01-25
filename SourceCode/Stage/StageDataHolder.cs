using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StageDataHolder : MonoBehaviour {

    Tilemap _tilemap;
    public Tilemap tilemap {
        get {
            if (!_tilemap) {
                TryGetComponent(out _tilemap);
            }
            return _tilemap;
        }
    }

    [SerializeField, Range(1, 10)] int _flashLimitCount = 1;
    public int flashLimitCount => _flashLimitCount;

    [SerializeField] Transform _spawnPoint;
    public Transform spawnPoint => _spawnPoint;

    public void SetSpawnPoint(Transform newPoint) { _spawnPoint = newPoint; }
    public Vector2 GetSpawnPosition() { return _spawnPoint.position; }

    void Start() {

    }

    private void OnDrawGizmos() {
        if (spawnPoint) {
            Gizmos.color = new Color(0.40f, 0.90f, 0.53f, 0.50f);
            Gizmos.DrawSphere(spawnPoint.position, 0.25f);
        }
    }
}