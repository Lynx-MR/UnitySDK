using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Lynx.UI
{

#if UNITY_EDITOR
    [CustomEditor(typeof(LynxUIGrabPositioner))]
    public class LynxUIGrabPositioner_Editor : Editor
    {
        protected bool AdvancedSetttings = false;
        protected bool ShowOffsetTransform = false;
        protected bool ShowSCIParameters = false;

        public override void OnInspectorGUI()
        {


            LynxUIGrabPositioner grb = (LynxUIGrabPositioner)target;

            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_Panel"), false);
            if (grb.followObjectAtStart)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("objectToFollow"), false);
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("HandleGrabSize"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("followDistanceThreshold"), false);

           
            if (grb.followObjectAtStart)
            {
                if (GUILayout.Button("Set inverse"))
                {
                    grb.PosOffset = Quaternion.Inverse(grb.objectToFollow.rotation) * (grb.Panel.position - grb.objectToFollow.position);
                    grb.RotOffset = Vector3.zero - grb.objectToFollow.rotation.eulerAngles;
                }
                ShowOffsetTransform = EditorGUILayout.Foldout(ShowOffsetTransform, "Offset Settings");
                if (ShowOffsetTransform)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("PosOffset"), false);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("RotOffset"), false);
                }
            }

            AdvancedSetttings = EditorGUILayout.Foldout(AdvancedSetttings, "Advanced settings");
            if (AdvancedSetttings)
            {

                EditorGUILayout.PropertyField(serializedObject.FindProperty("MakeHandleMesh"), false);
                if (grb.MakeHandleMesh)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("handleMaterial"), false);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("handleColor"), false);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("handleGrabbedColor"), false);
                }

                EditorGUILayout.Separator();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("followObjectAtStart"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("stopFollowOnGrab"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_SelectExtendeDistance"), false);


            }
            if (serializedObject.ApplyModifiedProperties())
            {

            }
        }
    }

#endif
}