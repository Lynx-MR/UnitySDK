/**
 * @file LynxHandtrackingEditor.cs
 *
 * @author Geoffrey Marhuenda
 *
 * @brief Add lynx handtracking feature into Unity Editor menu to help configuration and integration in the scene.
 */
using System.IO;
using UnityEditor;
using UnityEditor.Presets;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem.XR;

#if LYNX_XRI
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
#endif

namespace Lynx
{
    public class LynxHandtrackingEditor
    {
        private const string STR_ANDROIDCOMMNG = "AndroidComManager.prefab";
        private const string STR_AUDIOVOLUMEINDICATOR = "AudioVolumeIndicator.prefab";

        private const string STR_LYNX_HAND = "Lynx Hand.prefab";
        private const string STR_LYNX_EVENT_SYSTEM = "Lynx Event System.prefab";

        private const string STR_LYNX_POKE = "Lynx Poke Interactor.prefab";
        private const string STR_LYNX_POKE_STABILIZER = "Lynx Poke Stabilized Attach.prefab";
        private const string STR_LYNX_LEFT_POKE_PRESET = "Lynx Left Poke.preset";
        private const string STR_LYNX_RIGHT_POKE_PRESET = "Lynx Right Poke.preset";


        private const string STR_LYNX_RAY = "Lynx Ray Interactor.prefab";
        private const string STR_LYNX_RAY_STABILIZER = "Lynx Ray Stabilized.prefab";
        private const string STR_LYNX_LEFT_RAY_PRESET = "Lynx Left Raycast.preset";
        private const string STR_LYNX_RIGHT_RAY_PRESET = "Lynx Right Raycast.preset";


        private const string STR_LYNX_DIRECT_INTERACTOR = "Lynx Direct Interactor.prefab";
        private const string STR_LYNX_DIRECT_INTERACTOR_STABILIZER = "Lynx Direct Stabilized Attach.prefab";
        private const string STR_LYNX_LEFT_DIRECT_INTERACTOR_PRESET = "Lynx Left Direct Interactor.preset";
        private const string STR_LYNX_RIGHT_DIRECT_INTERACTOR_PRESET = "Lynx Right Direct Interactor.preset";


        private const string STR_LYNX_HAND_LEFT_VISUALIZER = "Lynx Left Hand Visualizer.prefab";
        private const string STR_LYNX_HAND_RIGHT_VISUALIZER = "Lynx Right Hand Visualizer.prefab";

        private const string STR_LYNX_MENU = "LynxMenu.prefab";

        private const string STR_LYNX_HAND_MENU = "Lynx Hand Menu.prefab";


        [MenuItem("Lynx/Inputs/Add handtracking", false, 200)]
        public static void AddHandtracking()
        {
            EditorWindow window = EditorWindow.GetWindow(typeof(HandtrackingAddWindow));
            Rect r = window.position;
            r.position = new Vector2(150.0f, 150.0f);
            r.width = 400.0f;
            r.height = 250.0f;
            window.position = r;
        }


        [MenuItem("GameObject/Lynx/Inputs/Add handtracking", false, 200)]
        public static void AddHandtrackingContextMenu()
        {
            AddHandtracking();
        }

        [MenuItem("Lynx/Add Lynx Menu", false, 300)]
        public static void AddLynxMenu()
        {
            GameObject lynxMenu = LynxBuildSettings.InstantiateGameObjectByPath(STR_LYNX_MENU, null);
            Undo.RegisterCreatedObjectUndo(lynxMenu, "Hands visualizer Right");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        [MenuItem("GameObject/Lynx/Lynx Menu", false, 200)]
        public static void AddLynxMenuContextMenu()
        {
            AddLynxMenu();
        }

        [MenuItem("GameObject/Lynx/UI/Hand Menu", false, 250)]
        public static void AddHandMenu()
        {
            GameObject handMenu = LynxBuildSettings.InstantiateGameObjectByPath(STR_LYNX_HAND_MENU, Camera.main.transform.parent);
            Undo.RegisterCreatedObjectUndo(handMenu, "Hand Menu");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

    public static void AddAndroidComManager()
        {
            //Add AndroidComMng if not present in scene
            if (AndroidComMng.IsInstanceValid())
            {
                string str_androidComMng = Directory.GetFiles(Application.dataPath, STR_ANDROIDCOMMNG, SearchOption.AllDirectories)[0].Replace(Application.dataPath, "Assets/");
                PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<Object>(str_androidComMng), null);
            }
        }
        public static void AddAudioVolumeIndicator()
        {
            //Add AudioVolumeIndicator if not present in scene
            if (LynxBuildSettings.FindObjectsOfTypeAll<AudioVolumeIndicator>().Count == 0)

            {
                string str_audioVolumeIndicator = Directory.GetFiles(Application.dataPath, STR_AUDIOVOLUMEINDICATOR, SearchOption.AllDirectories)[0].Replace(Application.dataPath, "Assets/");
                PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<Object>(str_audioVolumeIndicator), null);
            }
        }

        public static void GenerateHand(Transform parent, bool isUnityEvent, bool isDirectInteraction, bool isRaycast, bool isLeftHand = false)
        {
            GameObject handObj = LynxBuildSettings.InstantiateGameObjectByPath(STR_LYNX_HAND, parent);
            handObj.name = isLeftHand ? "Lynx Left Hand" : "Lynx Right Hand";

            if (isUnityEvent)
            {
                GameObject poke = LynxBuildSettings.InstantiateGameObjectByPath(STR_LYNX_POKE, handObj.transform);
                AssetDatabase.LoadAssetAtPath<Preset>(Directory.GetFiles(Application.dataPath, isLeftHand ? STR_LYNX_LEFT_POKE_PRESET : STR_LYNX_RIGHT_POKE_PRESET, SearchOption.AllDirectories)[0].Replace(Application.dataPath, "Assets/")).ApplyTo(poke.GetComponent<TrackedPoseDriver>());

                // Stabilizer
                GameObject pokeStabilizer = LynxBuildSettings.InstantiateGameObjectByPath(STR_LYNX_POKE_STABILIZER, handObj.transform);
                poke.GetComponent<XRBaseInteractor>().attachTransform = pokeStabilizer.transform;
                pokeStabilizer.GetComponent<XRTransformStabilizer>().targetTransform = poke.transform;

                handObj.GetComponent<XRInteractionGroup>().startingGroupMembers.Add(poke.GetComponent<XRBaseInteractor>());
            }

            if (isDirectInteraction)
            {
                GameObject directInteractor = LynxBuildSettings.InstantiateGameObjectByPath(STR_LYNX_DIRECT_INTERACTOR, handObj.transform);
                AssetDatabase.LoadAssetAtPath<Preset>(Directory.GetFiles(Application.dataPath, isLeftHand ? STR_LYNX_LEFT_DIRECT_INTERACTOR_PRESET : STR_LYNX_RIGHT_DIRECT_INTERACTOR_PRESET, SearchOption.AllDirectories)[0].Replace(Application.dataPath, "Assets/")).ApplyTo(directInteractor.GetComponent<ActionBasedController>());

                // Stabilizer
                GameObject directInteractorStabilizer = LynxBuildSettings.InstantiateGameObjectByPath(STR_LYNX_DIRECT_INTERACTOR_STABILIZER, handObj.transform);
                directInteractor.GetComponent<XRBaseInteractor>().attachTransform = directInteractorStabilizer.transform;
                directInteractorStabilizer.GetComponent<XRTransformStabilizer>().targetTransform = directInteractor.transform;

                handObj.GetComponent<XRInteractionGroup>().startingGroupMembers.Add(directInteractor.GetComponent<XRBaseInteractor>());
            }

            if (isRaycast)
            {
                GameObject ray = LynxBuildSettings.InstantiateGameObjectByPath(STR_LYNX_RAY, handObj.transform);
                AssetDatabase.LoadAssetAtPath<Preset>(Directory.GetFiles(Application.dataPath, isLeftHand ? STR_LYNX_LEFT_RAY_PRESET : STR_LYNX_RIGHT_RAY_PRESET, SearchOption.AllDirectories)[0].Replace(Application.dataPath, "Assets/")).ApplyTo(ray.GetComponent<ActionBasedController>());

                // Stabilizer
                GameObject rayStabilizer = LynxBuildSettings.InstantiateGameObjectByPath(STR_LYNX_RAY_STABILIZER, handObj.transform);
                ray.GetComponent<XRBaseInteractor>().attachTransform = rayStabilizer.GetComponentsInChildren<Transform>()[1];
                ray.GetComponent<XRRayInteractor>().rayOriginTransform = rayStabilizer.transform;
                rayStabilizer.GetComponent<XRTransformStabilizer>().targetTransform = ray.transform;

                handObj.GetComponent<XRInteractionGroup>().startingGroupMembers.Add(ray.GetComponent<XRBaseInteractor>());

            }

            Undo.RegisterCreatedObjectUndo(handObj, "Lynx hand");

        }

        public static void AddHandtracking(bool isUnityEvent, bool isDirectInteraction, bool isRaycast, bool handsVisualizer, bool interfacePointer)
        {
            //AddAndroidComManager();
            //AddAudioVolumeIndicator();

            // Get camera parent
            Transform parent = Camera.main.transform.parent;

            // Add event system
            GameObject eventSystem = LynxBuildSettings.InstantiateGameObjectByPath(STR_LYNX_EVENT_SYSTEM, parent);
            Undo.RegisterCreatedObjectUndo(eventSystem, "XR Event System");


            // Add hands
            GenerateHand(parent, isUnityEvent, isDirectInteraction, isRaycast, true);
            GenerateHand(parent, isUnityEvent, isDirectInteraction, isRaycast, false);

            if (handsVisualizer)
            {
                GameObject handsVisualizerLeftObj = LynxBuildSettings.InstantiateGameObjectByPath(STR_LYNX_HAND_LEFT_VISUALIZER, parent);
                Undo.RegisterCreatedObjectUndo(handsVisualizerLeftObj, "Hands visualizer Left");

                GameObject handsVisualizerRightObj = LynxBuildSettings.InstantiateGameObjectByPath(STR_LYNX_HAND_RIGHT_VISUALIZER, parent);
                Undo.RegisterCreatedObjectUndo(handsVisualizerRightObj, "Hands visualizer Right");
            }

            if (interfacePointer)
            {
                eventSystem.AddComponent<LynxUIPointerManager>();
            }

            Debug.Log($"Hands added under {parent.name}");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }


        /// <summary>
        /// Window asking the user what features to use with the handtracking.
        /// </summary>
        public class HandtrackingAddWindow : EditorWindow
        {
            bool cbUnityEvents = true;
            bool cbDirectInteractor = true;
            bool cbRaycasting = true;
            bool cbULSettings = true;
            bool cbHandsVisualizer = true;
            bool cbInterfacePointer = false;

            void OnGUI()
            {
                GUILayout.Space(20);
                GUILayout.Label("Add handtracking to the scene.\nPlease define which interaction types you want to use.", EditorStyles.label);

                GUILayout.Space(20);
                //cbUnityEvents = EditorGUILayout.Toggle("Unity events", cbUnityEvents);
                GUILayout.Label("Unity UI compatibility is enabled.", EditorStyles.label);
#if LYNX_XRI
                cbDirectInteractor = EditorGUILayout.Toggle("XR Direct interactors", cbDirectInteractor);
                cbRaycasting = EditorGUILayout.Toggle("XR raycast interactors", cbRaycasting);
                cbHandsVisualizer = EditorGUILayout.Toggle("Hands visualizer", cbHandsVisualizer);
                if (cbDirectInteractor)
                    cbInterfacePointer = EditorGUILayout.Toggle("UI pointer", cbInterfacePointer) && cbDirectInteractor;
                cbULSettings = EditorGUILayout.Toggle("Ultraleap configuration", cbULSettings);
#endif

                GUILayout.Space(20);
                if (GUILayout.Button("Add"))
                {
                    if (cbULSettings)
                    {
                        if (Leap.Unity.UltraleapSettings.Instance)
                            Leap.Unity.UltraleapSettings.Instance.updateMetaInputSystem = true;
                    }

                    AddHandtracking(cbUnityEvents, cbDirectInteractor, cbRaycasting, cbHandsVisualizer, cbInterfacePointer);
                    this.Close();
                }

                if (GUILayout.Button("Cancel"))
                {
                    Debug.Log("Cancelled");
                    this.Close();
                }
            }
        }


    }
}