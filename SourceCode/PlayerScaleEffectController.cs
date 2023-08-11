using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class PlayerScaleEffectController : MonoBehaviour {

    [SerializeField]SpriteRenderer spriteRenderer;

    Transform _spriteRendererTransform;
    Transform spriteRendererTransform {
        get {
            if (_spriteRendererTransform == null) {
                _spriteRendererTransform = spriteRenderer.transform;
            }
            return _spriteRendererTransform;
        }
    }

    [Header("Jump")]
    [SerializeField] Vector2 jumpUpStartScale = Vector2.one;
    [SerializeField] float jumpUpDuration = 0.5f;
    Tweener jumpUpTw;

    [Header("Land")]
    [SerializeField] Vector2 justLandScale = Vector2.one;
    [SerializeField] float landDuration = 0.5f;
    Tweener landTw;

    private void Start() {
        jumpUpTw = spriteRendererTransform.DOScale(Vector3.one, jumpUpDuration).Pause().SetAutoKill(false).SetLink(gameObject);
        landTw = spriteRendererTransform.DOScale(Vector3.one, landDuration).Pause().SetAutoKill(false).SetLink(gameObject);
    }

    public void JumpUp() {
        if(landTw.IsPlaying()) landTw.Pause();
        spriteRendererTransform.localScale = jumpUpStartScale;
        jumpUpTw.Restart();
    }

    public void Land() {
        spriteRendererTransform.localScale = justLandScale;
        landTw.Restart();
    }
}