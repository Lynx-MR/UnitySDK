//   ==============================================================================
//   | LynxInterfaces (2023)                                                      |
//   |======================================                                      |
//   | LynxUIPositioner Editor Script                                             |
//   | Script to modify the inspector GUI of the LynxUIPositioner Script.         |
//   ==============================================================================

using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

namespace Lynx.UI
{
    [CustomEditor(typeof(Lynx.UI.LynxUIPositioner))]
    public class LynxUIPositionerEditor : Editor
    {
        private enum PositionMode { locked, followCarthesian, followCylindrical };
        private bool showRotationParameters = false;

        public override void OnInspectorGUI()
        {
            LynxUIPositioner script = (LynxUIPositioner)target;
            GUIStyle bold = new GUIStyle();
            bold = EditorStyles.boldLabel;

            serializedObject.Update();

            EditorGUILayout.LabelField("Generic Parameters", bold);
            EditorGUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("uiTransform"), EditorGUIUtility.TrTextContent("UI Transform", "Transform of the GameObject to be positonned and rotated by the script."));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("positionMode"), EditorGUIUtility.TrTextContent("Position Mode", "Carthesian (XYZ offsets), Cylindrical (distance, angle and height offsets) and Locked (static)."));
            SerializedProperty currentPositionMode = serializedObject.FindProperty("positionMode");

            EditorGUILayout.PropertyField(serializedObject.FindProperty("uiOriginTarget"), EditorGUIUtility.TrTextContent("UI Origin Target", "Transform relative of which the UI Transform is positonned (typicaly the Camera)."));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("uiLookAtTarget"), EditorGUIUtility.TrTextContent("UI Look At Target", "Transform relative of which the UI Transform is rotated (typicaly the Camera)."));

            PositionMode mode = (PositionMode)currentPositionMode.enumValueIndex;

            if (mode == PositionMode.followCarthesian)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Carthesian Parameters", bold);
                EditorGUILayout.Space(10);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("originLocalOffset"), EditorGUIUtility.TrTextContent("Origin Local Offset", "Translation offset relative to UI origin target local space."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("originDistThreshold"), EditorGUIUtility.TrTextContent("Origin Distance Threshold", "Distance between UI Transform Position and offset position above which repositionning lerp is triggered."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("originLerpTime"), EditorGUIUtility.TrTextContent("Origin Lerp Time", "The repositioning lerp time in seconds. Set to 0 for instant repositionning."));
            }
            else if (mode == PositionMode.followCylindrical)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Cylindrical Parameters", bold);
                EditorGUILayout.Space(10);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("targetDistance"), EditorGUIUtility.TrTextContent("Target Distance", "Distance forward from UI origin target on world XZ plane."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("targetDistanceThreshold"), EditorGUIUtility.TrTextContent("Target Distance Threshold", "Distance between UI Transform distance forward from UI origin and distance forward target above which distance repositionning lerp is triggered."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("targetDistanceLerpTime"), EditorGUIUtility.TrTextContent("Target Distance Lerp Time", "The repositioning lerp time in seconds for distance ajustment. Set to 0 for instant repositionning."));

                EditorGUILayout.Space(10);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("targetAngle"), EditorGUIUtility.TrTextContent("Target Angle", "Direction angle from UI origin target forward on world XZ plane."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("targetAngleThreshold"), EditorGUIUtility.TrTextContent("Target Angle Threshold", "Distance between UI transform direction angle from UI origin forward and direction angle target above which angle repositionning lerp is triggered."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("targetAngleLerpTime"), EditorGUIUtility.TrTextContent("Target Angle Lerp Time", "The repositioning lerp time in seconds for angle ajustment. Set to 0 for instant repositionning."));

                EditorGUILayout.Space(10);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("targetHeight"), EditorGUIUtility.TrTextContent("Target Height", "Position difference from from UI origin target on world Y axis."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("targetHeightThreshold"), EditorGUIUtility.TrTextContent("Target Height Threshold", "Distance between UI transform height from UI origin & height target above which height repositionning lerp is triggered."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("targetHeightLerpTime"), EditorGUIUtility.TrTextContent("Target Height Lerp Time", "The repositioning lerp time in seconds for height ajustment. Set to 0 for instant repositionning."));
            }

            EditorGUILayout.Space(10);

            showRotationParameters = EditorGUILayout.Toggle("Modify rotation parameters", showRotationParameters);

            if (showRotationParameters)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Rotation Parameters", bold);
                EditorGUILayout.Space(10);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("yAxisRotUpdateOnlyOnPosLerp"), EditorGUIUtility.TrTextContent("Y Axis Rot Update Only On Pos Lerp", "Only update rotation when positon lerp is taking place."));

                EditorGUILayout.Space(10);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("xAxisMode"), EditorGUIUtility.TrTextContent("X Axis Mode", "Locked (X rotation stays static), Look at target X axis (face UI origin target) and Follow target X angle (follow UI origin target)."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("yAxisMode"), EditorGUIUtility.TrTextContent("Y Axis Mode", "Locked World (Y rotation stays static), Look at target Y axis (face UI origin target) and Follow target Y angle (follow UI origin target)."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("zAxisMode"), EditorGUIUtility.TrTextContent("Z Axis Mode", "Locked (Z rotation stays static) and Follow target Z angle (follow UI origin target)."));

                EditorGUILayout.Space(10);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("rotationLocalOffset"), EditorGUIUtility.TrTextContent("Rotation Local Offset", "Additional rotation angle offsets added to the rotation updates configured above."));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

