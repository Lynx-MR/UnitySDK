using UnityEditor;
using UnityEngine;

namespace Lynx.UI
{
    [CustomEditor (typeof(LynxThemeColorSetSO))]
    public class LynxThemeColorSet_Editor : Editor
    {
        public override void OnInspectorGUI()
        {

            GUIStyle bold = new GUIStyle();
            bold = EditorStyles.boldLabel;


            serializedObject.Update();


            EditorGUILayout.LabelField("Button parameters", bold);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("normal"), EditorGUIUtility.TrTextContent("Normal Color"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("highlithed"), EditorGUIUtility.TrTextContent("Highlithed Color"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pressed"), EditorGUIUtility.TrTextContent("Pressed Color"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("selected"), EditorGUIUtility.TrTextContent("Selected Color"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("disabled"), EditorGUIUtility.TrTextContent("Disabled Color"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("colorMultiplier"), EditorGUIUtility.TrTextContent("Color Multiplier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fadeDuration"), EditorGUIUtility.TrTextContent("Fade Duration"));

            EditorGUILayout.Space(0);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("accent"), EditorGUIUtility.TrTextContent("Accent Color"));


            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("background Colors", bold);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("main"), EditorGUIUtility.TrTextContent("Main Color"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dark"), EditorGUIUtility.TrTextContent("Dark Color"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("light"), EditorGUIUtility.TrTextContent("Light Color"));

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Others Colors", bold);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("text"), EditorGUIUtility.TrTextContent("Text Color"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"), EditorGUIUtility.TrTextContent("Icon Color"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}