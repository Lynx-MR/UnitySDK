//   ==============================================================================
//   | LynxInterfaces (2023)                                                      |
//   |======================================                                      |
//   | LynxToggleButton Editor Script                                             |
//   | Script to modify the inspector GUI of the LynxToggleButton Script.         |
//   ==============================================================================

using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

namespace Lynx.UI
{
    [CustomEditor(typeof(Lynx.UI.LynxSlider))]
    public class LynxSliderEditor : SliderEditor
    {
        public override void OnInspectorGUI()
        {
            LynxSlider script = (LynxSlider)target;
            GUIStyle bold = new GUIStyle();
            bold = EditorStyles.boldLabel;

            serializedObject.Update();

            base.OnInspectorGUI();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_disableSelectState"), EditorGUIUtility.TrTextContent("Disable Select State", "If checked, the select state of the button is disable."));


            #region THEME MANAGMENT
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useSound"), EditorGUIUtility.TrTextContent("Use sound", "Check to automatically play sound per events."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useTheme"), EditorGUIUtility.TrTextContent("Use theme colors", "automatically assigns colors to match those of the active theme"));
            SerializedProperty isUsingTheme = serializedObject.FindProperty("useTheme");

            if (isUsingTheme.boolValue)
            {
                if (LynxThemeManager.Instance == null)
                {
                    if (GUILayout.Button("ADD THEME MANAGER TO SCENE"))
                        LynxThemeManagerEditor.InstantiateThemeManager();
                }
                else if (LynxThemeManager.Instance.ThemeSets != null && LynxThemeManager.Instance.ThemeSets.Count == 0)
                {
                    EditorGUILayout.HelpBox("Theme Manager does not have any theme set", MessageType.Warning);
                }
                else if (LynxThemeManager.Instance.ThemeSets != null && LynxThemeManager.Instance.ThemeSets.Count != 0)
                {
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

