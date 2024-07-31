//   ==============================================================================
//   | Lynx Mixed Reality                                                         |
//   |======================================                                      |
//   | Lynx SDK                                                                   |
//   | Editor script for LineHands.cs parameters                                  |
//   ==============================================================================



#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Lynx
{
    [CustomEditor(typeof(LineHands))]
    public class LineHands_Editor : Editor
    {

        private bool advancedColorOption = false;

        public override void OnInspectorGUI()
        {
            LineHands script = (LineHands)target;

            serializedObject.Update();
            //General Parameters
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jointScale"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("lineScale"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("LineMaterial"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("JointMaterial"), false);

            EditorGUILayout.Space(10);


            EditorGUILayout.PropertyField(serializedObject.FindProperty("handsToRender"), true);

            EditorGUILayout.Space(10);

            advancedColorOption = EditorGUILayout.Foldout(advancedColorOption, "Advanced Color Option");
            if (advancedColorOption)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("colorMode"), true);

                if (script.colorMode == LineHands.ColorMode.Random)
                {
                    GUILayout.Label("A random shade of color is applied to each hand on 'Tracking found'.");
                }
                if (script.colorMode == LineHands.ColorMode.Color)
                {
                    GUILayout.Label("Set a colour for each hand");
                    EditorGUILayout.Space(5);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("leftHandColor"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("rightHandColor"), true);
                }
                if (script.colorMode == LineHands.ColorMode.Palette)
                {
                    GUILayout.Label("The hands will alternate between each colour when the tracking is found.");
                    EditorGUILayout.Space(5);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("leftHandsPalette"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("rightHandsPalette"), true);
                }
            }
            if (serializedObject.ApplyModifiedProperties())
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }

        }
    }
}
#endif