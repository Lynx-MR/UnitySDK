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
    [CustomEditor(typeof(Lynx.UI.LynxSimpleButton))]
    public class LynxSimpleButtonEditor : ButtonEditor
    {
        public override void OnInspectorGUI()
        {
            LynxSimpleButton script = (LynxSimpleButton)target;
            GUIStyle bold = new GUIStyle();
            bold = EditorStyles.boldLabel;

            serializedObject.Update();

            EditorGUILayout.LabelField("Button Parameters", bold);
            EditorGUILayout.Space(10);
            base.OnInspectorGUI();
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnPress"), EditorGUIUtility.TrTextContent("OnPress", "This event is called when the button is pressed."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnUnpress"), EditorGUIUtility.TrTextContent("OnUnpress", "This event is called when the button is released."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_disableSelectState"), EditorGUIUtility.TrTextContent("Disable Select State", "If checked, the select state of the button is disable."));
            EditorGUILayout.Space(10);

            #region THEME MANAGMENT
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_useSound"), EditorGUIUtility.TrTextContent("Use Sounds", "Check to automatically play sound per events."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_useTheme"), EditorGUIUtility.TrTextContent("Use Theme", "Automatically assigns colors to match those of the active theme."));
            SerializedProperty isUsingTheme = serializedObject.FindProperty("m_useTheme");

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
            else if(LynxThemeManager.Instance)
                LynxThemeManager.Instance.ThemeUpdateEvent -= script.SetThemeColors;
            #endregion

            EditorGUILayout.Space(10);
            SerializedProperty m_TransitionProperty = serializedObject.FindProperty("m_Transition");
            if ((m_TransitionProperty.enumValueIndex == ((int)UnityEngine.UI.Selectable.Transition.ColorTint))){
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_secondaryTargetGraphic"), EditorGUIUtility.TrTextContent("Secondary target graphic", "If checked, the select state of the button is disable."));
            }
            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_animation"), EditorGUIUtility.TrTextContent("Animation", "Pressing animation parameters."));
            EditorGUILayout.Space(20);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}

