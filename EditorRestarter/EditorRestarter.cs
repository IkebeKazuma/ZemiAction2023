using UnityEditor;
using UnityEngine;

namespace PMP {
    public static class EditorRestarter {
        [MenuItem("File/Restart")]
        private static void Restart() {
            // プロジェクトパス
            string projectPath = Application.dataPath.Remove(Application.dataPath.Length - "/Assets".Length, "/Assets".Length);
            // Unityプロジェクトを開く
            EditorApplication.OpenProject(projectPath);
        }
    }
}