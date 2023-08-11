using UnityEditor;
using UnityEngine;

namespace PMP {
    public static class EditorRestarter {
        [MenuItem("File/Restart")]
        private static void Restart() {
            // �v���W�F�N�g�p�X
            string projectPath = Application.dataPath.Remove(Application.dataPath.Length - "/Assets".Length, "/Assets".Length);
            // Unity�v���W�F�N�g���J��
            EditorApplication.OpenProject(projectPath);
        }
    }
}