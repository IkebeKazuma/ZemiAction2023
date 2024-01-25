using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class ScreenToRawImage : MonoBehaviour {

    Camera currentCamera;

    [SerializeField] RawImage target;

    void OnEnable() {
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    void OnDisable() {
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

    void OnEndCameraRendering(ScriptableRenderContext context, Camera camera) {
        currentCamera = camera;

        if ((currentCamera == null) || (currentCamera.cullingMask & (1 << gameObject.layer)) == 0) {
            return;
        }

        Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false, true);
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);
        texture.Apply(false);

        target.texture = texture;

        if (texture) Object.Destroy(texture);
    }
}