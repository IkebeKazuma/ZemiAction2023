using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.UI;

namespace PMP.BetterButton {
    [CustomEditor(typeof(BetterButton))]
    [CanEditMultipleObjects]
    public class BetterButtonEditor : UnityEditor.UI.SelectableEditor {

        [MenuItem("GameObject/UI/PM Presents/Better Button", false, 20)]
        static void CreateBetterButton() {
            GameObject selectGameObject = Selection.activeGameObject;
            if (selectGameObject == null || !selectGameObject.GetComponent<RectTransform>()) {
                Debug.LogError("[PM Presents/Better Button] 選択されたオブジェクトがなかったか、RectTransformが割り当てられていないため作成をキャンセルしました。");
                return;
            }

            string assetPath = "Assets/PM Presents/BetterButton/BetterButton.prefab";

            var asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));

            GameObject go = Instantiate(asset) as GameObject;
            go.name = "BetterButton";
            RectTransform rectTrns = go.GetComponent<RectTransform>();
            rectTrns.SetParent(selectGameObject.transform);

            // Reset
            rectTrns.localPosition = Vector3.zero;
            rectTrns.localRotation = Quaternion.identity;
            rectTrns.localScale = Vector3.one;

            Selection.activeGameObject = go;

            Debug.Log("[PM Presents/Better Button] Better Button を作成しました。");
        }

        AnimBool spriteTrasitionAnimBool = new AnimBool();

        // Navigation
        GUIContent visualizeNavigation = EditorGUIUtility.TrTextContent("Visualize", "Show navigation flows between selectable UI elements.");
        private static bool showNavigation = false;
        private static string showNavigationKey = "SelectableEditor.ShowNavigation";

        protected override void OnEnable() {
            base.OnEnable();

            spriteTrasitionAnimBool.value = true;
            spriteTrasitionAnimBool.valueChanged.AddListener(Repaint);

            showNavigation = EditorPrefs.GetBool(showNavigationKey);
        }

        protected override void OnDisable() {
            base.OnDisable();

            spriteTrasitionAnimBool.valueChanged.RemoveListener(Repaint);
        }

        private void PropertyField(string property, string label) {
            if (serializedObject != null) EditorGUILayout.PropertyField(serializedObject.FindProperty(property), new GUIContent(label));
        }

        public override void OnInspectorGUI() {

            BetterButton handler = (BetterButton)target;

            if (handler != null) {
                serializedObject.Update();

                EditorGUI.BeginChangeCheck();

                PropertyField("m_Interactable", "Interactable");
                PropertyField("interactOnlyOnce", "Only once");
                if (handler.interactOnlyOnce) {
                    EditorGUI.indentLevel++;
                    PropertyField("ignoreExceptTarget", "Ignore except target");
                    if (handler.ignoreExceptTarget) {
                        PropertyField("targetInputBtn", "Target input");
                        string ts = "";
                        switch (handler.targetInputBtn) {
                            case UnityEngine.EventSystems.PointerEventData.InputButton.Left:
                                ts = "左";
                                break;
                            case UnityEngine.EventSystems.PointerEventData.InputButton.Right:
                                ts = "右";
                                break;
                            case UnityEngine.EventSystems.PointerEventData.InputButton.Middle:
                                ts = "中";
                                break;
                        }
                        EditorGUILayout.LabelField($"現在は{ts}クリックが対象になっています。");
                    } else {
                        EditorGUILayout.LabelField($"現在は左、右、中の全てのクリックが対象になっています。");
                    }
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Space();

                if (handler.transition != Selectable.Transition.SpriteSwap) handler.transition = Selectable.Transition.SpriteSwap;

                {
                    GUIStyle style = new();
                    Color defCol = GUI.skin.label.normal.textColor;
                    style.normal.textColor = new Color(defCol.r, defCol.g, defCol.b, 0.50f);
                    EditorGUILayout.LabelField($"Transition : {handler.transition} (Fixed)", style);
                }
                //PropertyField("m_Transition", "Transition");
                EditorGUI.indentLevel++;
                {
                    if (EditorGUILayout.BeginFadeGroup(spriteTrasitionAnimBool.faded)) {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SpriteState"));
                    }
                    EditorGUILayout.EndFadeGroup();
                }
                EditorGUI.indentLevel--;

                EditorGUILayout.Space();

                PropertyField("m_Navigation", "Navigation");
                Rect toggleRect = EditorGUILayout.GetControlRect();
                toggleRect.xMin += EditorGUIUtility.labelWidth;
                showNavigation = GUI.Toggle(toggleRect, showNavigation, visualizeNavigation, EditorStyles.miniButton);

                //PropertyField("_state", "State");

                EditorGUILayout.Space();

                EditorGUILayout.LabelField($"Is Pointer Inside : {handler.isPointerInside}");

                EditorGUILayout.Space();

                EditorGUI.indentLevel++;
                PropertyField("_pointerHandler", "Pointer Handler");
                EditorGUI.indentLevel--;

                if (EditorGUI.EndChangeCheck()) {
                    EditorPrefs.SetBool(showNavigationKey, showNavigation);
                    SceneView.RepaintAll();
                }

                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}