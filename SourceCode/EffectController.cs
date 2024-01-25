using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectController : SingletonMonoBehaviour<EffectController> {

    [SerializeField] ParticleSystem playerDieEff;

    public void PlayPlayerDieEff(Vector2 pos) {
        playerDieEff.transform.position = pos;
        playerDieEff.Play();
    }

}