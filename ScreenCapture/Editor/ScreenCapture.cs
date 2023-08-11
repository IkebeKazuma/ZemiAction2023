using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PMP.ScreenCapture {
    public class ScreenCapture : Editor {

        [MenuItem("Tools/PM Presents/Screenshot %F3", false)]
        static void OnScreenCapture() {
            ScreenCaptureHandler handler = new GameObject("Temp Screenshot Handler").AddComponent<ScreenCaptureHandler>();
            handler.Begin();
        }
    }

    public class ScreenCaptureHandler : MonoBehaviour {

        IEnumerator t;

        bool isCapturing = false;

        public void Begin() {
            isCapturing = false;
            t = PrintScreenIE();

            EditorApplication.update += Process;
        }

        void Process() => t.MoveNext();

        IEnumerator PrintScreenIE() {
            if (isCapturing) yield break;

            isCapturing = true;

            yield return null;

            var path = Application.dataPath + "/Screenshots/";

            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }

            string date = DateTime.Now.ToString("yy-MM-dd_HH-mm-ss");
            string fileName = path + "capture" + "_" + date + ".png";

            UnityEngine.ScreenCapture.CaptureScreenshot(fileName);

            yield return new WaitForSeconds(0.1f);

            yield return new WaitUntil(() => File.Exists(fileName));

            EditorUtility.DisplayDialog("ScreenCapture", "スクリーンショットをAssets/Screenshotsフォルダに保存しました。\n反映までに時間がかかる場合があります。", "閉じる");

            yield return new WaitForSeconds(0.1f);

            // アセットをリフレッシュ
            AssetDatabase.Refresh();

            DestroyImmediate(gameObject);

            yield break;
        }
    }
}