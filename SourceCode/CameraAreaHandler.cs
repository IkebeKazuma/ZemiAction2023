using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAreaHandler : MonoBehaviour {

    public int currentCameraIndex { get; private set; } = -1;
    int prevCameraIndex = -1;

    [SerializeField] List<GameObject> cameraList;

    public void SetCameraIndex(int newIndex) {
        currentCameraIndex = newIndex;
        if (prevCameraIndex != currentCameraIndex) {
            prevCameraIndex = currentCameraIndex;
            OnCameraAreaChanged();
        }
    }

    void OnCameraAreaChanged() {
        for (int i = 0; i < cameraList.Count; i++) {
            GameObject cam = cameraList[i];
            if (i == currentCameraIndex - 1) {
                cam.SetActive(true);
            } else {
                cam.SetActive(false);
            }
        }
    }

}