using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraArea : MonoBehaviour {

    [SerializeField] CameraAreaHandler cameraAreaHandler;

    [SerializeField] int index;

    private void OnTriggerEnter2D(Collider2D collision) {
        cameraAreaHandler.SetCameraIndex(index);
    }

    private void OnTriggerExit2D(Collider2D collision) {

    }
}