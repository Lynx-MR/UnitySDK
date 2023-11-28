//   ==============================================================================
//   | LynxInterfaces (2023)                                                      |
//   |======================================                                      |
//   | LynxSimpleButton Editor Script                                             |
//   | Script to modify the inspector GUI of the LynxSimpleButton Script.         |
//   ==============================================================================

using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

namespace Lynx.UI
{
    [CustomEditor(typeof(Lynx.UI.LynxImage))]
    public class LynxImageEditor : ImageEditor
    {

        public override void OnInspectorGUI()
        {
            LynxImage script = (LynxImage)target;
            GUIStyle bold = new GUIStyle();
            bold = EditorStyles.boldLabel;

            serializedObject.Update();

            EditorGUILayout.LabelField("Image parameters", bold);
            EditorGUILayout.Space(10);
            base.OnInspectorGUI();

            EditorGUILayout.Space(10);

            #region THEME MANAGMENT
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useTheme"), EditorGUIUtility.TrTextContent("Use theme colors", "automatically assigns colors to match those of the active theme"));
            SerializedProperty isUsingTheme = serializedObject.FindProperty("useTheme");

            if (isUsingTheme.boolValue)
            {
                if (LynxThemeManager.Instance == null)
                {
                    if (GUILayout.Button("ADD THEME MANAGER TO SCENE"))
                        LynxThemeManagerEditor.InstantiateThemeManager();
                }
                else if(LynxThemeManager.Instance.ThemeSets != null && LynxThemeManager.Instance.ThemeSets.Count == 0)
                {
                    EditorGUILayout.HelpBox("Theme Manager does not have any theme set", MessageType.Warning);
                }
                else if(LynxThemeManager.Instance.ThemeSets != null || LynxThemeManager.Instance.ThemeSets.Count != 0)
                {
                    LynxThemeManager.Instance.StartCoroutine(LynxThemeManager.SetupCoroutine(script.SetThemeColors));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedColor"), EditorGUIUtility.TrTextContent("Color type", "automatically get colors to match those of the active theme"));
                    if (LynxThemeManager.Instance != null)
                        script.SetThemeColors();
                }
            }
            else if (LynxThemeManager.Instance)
                LynxThemeManager.Instance.ThemeUpdateEvent -= script.SetThemeColors;
            #endregion


            serializedObject.ApplyModifiedProperties();
        }
    }
}

