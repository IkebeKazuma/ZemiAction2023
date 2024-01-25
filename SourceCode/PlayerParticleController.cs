using PMP.UnityLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParticleController : MonoBehaviour {

    [SerializeField] ParticleSystem landEff;
    [SerializeField] ParticleSystem runEff;

    public void PlayLandEff(float scale) {
        float correctedScale = Mathf.InverseLerp(-12f, -18f, scale).RoundDownToNDecimalPoint(2);
        //Debug.Log(correctedScale);

        correctedScale = Mathf.Clamp(correctedScale, 0.1f, 0.8f);

        var main = landEff.main;
        main.startSize = new ParticleSystem.MinMaxCurve(correctedScale, correctedScale);

        landEff.Play();
    }

    public void SetPlayStateRun(bool newPlayState, float velocityX) {
        var main = runEff.main;
        if (newPlayState) {
            main.emitterVelocity = new Vector3(velocityX, 0.0f, 0.0f);
        } else {
            main.emitterVelocity = new Vector3(0.0f, 0.0f, 0.0f);
        }
    }

}