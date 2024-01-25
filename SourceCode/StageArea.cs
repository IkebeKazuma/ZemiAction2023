using KanKikuchi.AudioManager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageArea : MonoBehaviour {

    [SerializeField] CameraAreaHandler cameraAreaHandler;

    [SerializeField] int index;

    bool firstTime = true;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            // �Q�[���J�n���͈�x�����������X���[����
            if (index == 1 && firstTime) {
                firstTime = false;
                return;
            }

            // �v���C���[���ꎞ�I�ɍs���s�\��
            if (cameraAreaHandler.currentCameraIndex != index) {
                GameManager.Instance.playerCtrl.FreezeMovementTemp(1f);
                SEManager.Instance.Play(SEPath.STAGE_CLEAR);
            }

            cameraAreaHandler.SetCameraIndex(index);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {

    }
}