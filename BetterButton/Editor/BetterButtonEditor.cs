using System.Drawing.Drawing2D;
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

            // アセットパス
            string assetPath = "Assets/PM Presents/BetterButton/Better Button.prefab";

            // ロード
            var asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));

            // インスタンス生成
            GameObject go = Instantiate(asset) as GameObject;

            // Nullチェック
            if (go == null) {
                Debug.LogError("[PM Presents/Better Button] Better Button の作成に失敗しました。");
                return;
            }

            go.name = "Better Button";
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

            if (handler == null) return;

            serializedObject.Update();

            GUIStyle descStyle = new();
            Color defCol = GUI.skin.label.normal.textColor;
            descStyle.normal.textColor = new Color(defCol.r, defCol.g, defCol.b, 0.50f);

            LayoutUtility.TitleAndCredit("Better Button", "© 2023 PinoMatcha");

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                LayoutUtility.HeaderField("通常設定", Color.gray, Color.white);

                EditorGUI.indentLevel++;

                PropertyField("m_Interactable", "Interactable");

                if (handler.transition != Selectable.Transition.SpriteSwap) handler.transition = Selectable.Transition.SpriteSwap;

                {
                    EditorGUILayout.LabelField($"Transition : {handler.transition} (Fixed)", descStyle);
                }
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

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                LayoutUtility.HeaderField("拡張設定", Color.gray, Color.white);

                EditorGUI.indentLevel++;

                PropertyField("uiLabel", "ラベル");

                PropertyField("allowOnlyOnceInput", "一度のみ反応");

                PropertyField("ignoreExceptTargetInput", "クリック入力をマスキングする");
                if (handler.ignoreExceptTargetInput) {
                    EditorGUI.indentLevel++;
                    PropertyField("targetInputBtn", "ターゲット");
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
                    EditorGUI.indentLevel--;
                    EditorGUILayout.LabelField($"現在は [{ts}クリック] でのみOnClick()を実行します。", descStyle);
                } else {
                    EditorGUILayout.LabelField($"現在は [左/右/中] 全てのクリックでOnClick()を実行します。", descStyle);
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUI.indentLevel++;

                LayoutUtility.HeaderField("ポインターのホバー状態（Read only）", Color.gray, Color.white);
                EditorGUILayout.ToggleLeft(handler.isPointerInside.ToString(), handler.isPointerInside);

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUI.indentLevel++;

                LayoutUtility.HeaderField("ポインターステート", Color.gray, Color.white);
                for (int i = 0; i < 3; i++) {
                    string label_jp = i switch {
                        0 => "左",
                        1 => "右",
                        2 => "中",
                        _ => "null"
                    };
                    BetterButton.PointerHandlerUnit pointerHandlerUnit = i switch {
                        0 => handler.pointerHandler.left,
                        1 => handler.pointerHandler.right,
                        2 => handler.pointerHandler.middle,
                        _ => null
                    };

                    EditorGUILayout.LabelField($"{label_jp}ポインター");
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField($"- 押下中 : {pointerHandlerUnit.isPointerDown}");
                    EditorGUILayout.LabelField($"- ホールド時間 : {pointerHandlerUnit.holdTime}");
                    EditorGUILayout.LabelField($"- ドラッグ中 : {pointerHandlerUnit.isDragging}");
                    EditorGUILayout.LabelField($"- クリック回数 : {pointerHandlerUnit.clickCount}");
                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            Repaint();

            if (EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
                EditorPrefs.SetBool(showNavigationKey, showNavigation);
                SceneView.RepaintAll();
            }
        }
    }
}