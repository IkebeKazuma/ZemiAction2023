using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RepeatStage : StageHandlerBase {
    [SerializeField] Tilemap invisibleTileMap;

    private void Start() {
        var col = invisibleTileMap.color;
        col.a = 0;
        invisibleTileMap.color = col;
    }
}