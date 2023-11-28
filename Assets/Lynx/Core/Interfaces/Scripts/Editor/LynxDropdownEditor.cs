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
    [CustomEditor(typeof(Lynx.UI.LynxDropdown))]
    public class LynxDropdownEditor : DropdownEditor
    {
        public override void OnInspectorGUI()
        {
            GUIStyle bold = new GUIStyle();
            bold = EditorStyles.boldLabel;

            serializedObject.Update();

            base.OnInspectorGUI();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_disableSelectState"), EditorGUIUtility.TrTextContent("Disable Select State", "If checked, the select state of the button is disable."));
            SerializedProperty m_TransitionProperty = serializedObject.FindProperty("m_Transition");
            if ((m_TransitionProperty.enumValueIndex == ((int)UnityEngine.UI.Selectable.Transition.ColorTint)))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_secondaryTargetGraphic"), EditorGUIUtility.TrTextContent("Secondary target graphic", "If checked, the select state of the button is disable."));
            }
            EditorGUILayout.Space(10);
            serializedObject.ApplyModifiedProperties();
        }
    }
}

