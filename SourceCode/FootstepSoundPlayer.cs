using KanKikuchi.AudioManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepSoundPlayer : MonoBehaviour {
    void PlayFootstepSound() {
        int range = 2;
        int pitchShift = Random.Range(-range, range + 1);
        float pitch = Mathf.Pow(2, pitchShift / 12.0f);

        string path = "";
        System.Func<string> getPath = null;
        if (StageManager.Instance.stageType == StageManager.StageType.FlashStage) {
            getPath = () => {
                int i = Random.Range(0, 5);
                return i switch {
                    0 => SEPath.FS_FLA_01,
                    1 => SEPath.FS_FLA_02,
                    2 => SEPath.FS_FLA_03,
                    3 => SEPath.FS_FLA_04,
                    4 => SEPath.FS_FLA_05,
                    _ => ""
                };
            };
        } else if (StageManager.Instance.stageType == StageManager.StageType.RepeatStage) {
            getPath = () => {
                int i = Random.Range(0, 5);
                return i switch {
                    0 => SEPath.FS_REP_01,
                    1 => SEPath.FS_REP_02,
                    2 => SEPath.FS_REP_03,
                    3 => SEPath.FS_REP_04,
                    4 => SEPath.FS_REP_05,
                    _ => ""
                };
            };
        }

        path = getPath?.Invoke();

        if (path != "") {
            SEManager.Instance.Play(path, pitch: pitch);
        }
    }
}