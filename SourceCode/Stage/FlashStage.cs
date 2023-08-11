using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class FlashStage : StageHandlerBase {

    [SerializeField] Light2D globalLight;

    private float _intensity = 1;

    [Header("Flash Settings")]
    [SerializeField, Range(0, 10)] float lightIntensity = 1;
    [SerializeField] float enterDuration = 0;
    [SerializeField] float exitDuration = 0.2f;

    Sequence flashSequence;
    Sequence tileMapVisualizeSequence;

    [Space]

    [SerializeField] Tilemap invisibleTileMap;

    private void Start() {
        flashSequence = DOTween.Sequence();
        tileMapVisualizeSequence = DOTween.Sequence();

        var col = invisibleTileMap.color;
        col.a = 0;
        invisibleTileMap.color = col;

        flashSequence
            .Join(DOTween.To(() => _intensity, (value) => _intensity = value, lightIntensity, enterDuration))
            .Join(DOTween.To(() => _intensity, (value) => _intensity = value, 0, exitDuration))
            .Pause()
            .SetAutoKill(false)
            .SetLink(gameObject)
            .OnUpdate(() => { globalLight.intensity = _intensity; });

        tileMapVisualizeSequence
            .Join(DOTween.ToAlpha(() => invisibleTileMap.color, (newCol) => invisibleTileMap.color = newCol, 1, enterDuration))
            .Join(DOTween.ToAlpha(() => invisibleTileMap.color, (newCol) => invisibleTileMap.color = newCol, 0, exitDuration))
            .Pause()
            .SetAutoKill(false)
            .SetLink(gameObject);

        manager.onPlayerAction.AddListener(Flash);
    }

    public void Flash() {
        flashSequence.Restart();
        tileMapVisualizeSequence.Restart();
    }
}