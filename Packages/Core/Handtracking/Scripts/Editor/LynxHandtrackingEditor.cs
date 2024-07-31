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
using UnityEngine.XR.Interaction.Toolkit.UI;
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
        private const string STR_LYNX_LINE_HANDS_VISUALIZER = "Lynx Line Hands Visualizer.prefab";


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

        [MenuItem("GameObject/Lynx/UI/Hand Menu", false, 250)]
        public static void AddHandMenu()
        {
            GameObject handMenu = LynxBuildSettings.InstantiateGameObjectByPath(LynxBuildSettings.LYNX_MODULES_PATH, STR_LYNX_HAND_MENU, Camera.main.transform.parent);
            Undo.RegisterCreatedObjectUndo(handMenu, "Hand Menu");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        public static void AddAudioVolumeIndicator()
        {
            //Add AudioVolumeIndicator if not present in scene
            if (LynxBuildSettings.FindObjectsOfTypeAll<AudioVolumeIndicator>().Count == 0)
                LynxBuildSettings.InstantiateGameObjectByPath(LynxBuildSettings.LYNX_CORE_PATH, STR_AUDIOVOLUMEINDICATOR, null);
        }

        public static void GenerateHand(Transform parent, bool isUnityEvent, bool isDirectInteraction, bool isRaycast, bool isLeftHand = false)
        {
            Transform handObj = isLeftHand ? parent.Find("Lynx Left Hand") : parent.Find("Lynx Right Hand");
            if (!handObj)
                handObj = LynxBuildSettings.InstantiateGameObjectByPath(LynxBuildSettings.LYNX_MODULES_PATH, STR_LYNX_HAND, parent).transform;

            handObj.name = isLeftHand ? "Lynx Left Hand" : "Lynx Right Hand";

            if (isUnityEvent)
            {
                if (!handObj.GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactors.XRPokeInteractor>())
                {
                    GameObject poke = LynxBuildSettings.InstantiateGameObjectByPath(LynxBuildSettings.LYNX_MODULES_PATH, STR_LYNX_POKE, handObj.transform);
                    AssetDatabase.LoadAssetAtPath<Preset>(Directory.GetFiles(LynxBuildSettings.LYNX_CORE_PATH, isLeftHand ? STR_LYNX_LEFT_POKE_PRESET : STR_LYNX_RIGHT_POKE_PRESET, SearchOption.AllDirectories)[0]).ApplyTo(poke.GetComponent<TrackedPoseDriver>());

                    // Stabilizer
                    GameObject pokeStabilizer = LynxBuildSettings.InstantiateGameObjectByPath(LynxBuildSettings.LYNX_MODULES_PATH, STR_LYNX_POKE_STABILIZER, handObj.transform);
                    poke.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>().attachTransform = pokeStabilizer.transform;
                    pokeStabilizer.GetComponent<XRTransformStabilizer>().targetTransform = poke.transform;

                    handObj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRInteractionGroup>().startingGroupMembers.Add(poke.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>());
                }
            }

            if (isDirectInteraction)
            {
                if (!handObj.GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor>())
                {
                    GameObject directInteractor = LynxBuildSettings.InstantiateGameObjectByPath(LynxBuildSettings.LYNX_MODULES_PATH, STR_LYNX_DIRECT_INTERACTOR, handObj.transform);
                    AssetDatabase.LoadAssetAtPath<Preset>(Directory.GetFiles(LynxBuildSettings.LYNX_CORE_PATH, isLeftHand ? STR_LYNX_LEFT_DIRECT_INTERACTOR_PRESET : STR_LYNX_RIGHT_DIRECT_INTERACTOR_PRESET, SearchOption.AllDirectories)[0]).ApplyTo(directInteractor.GetComponent<ActionBasedController>());

                    // Stabilizer
                    GameObject directInteractorStabilizer = LynxBuildSettings.InstantiateGameObjectByPath(LynxBuildSettings.LYNX_MODULES_PATH, STR_LYNX_DIRECT_INTERACTOR_STABILIZER, handObj.transform);
                    directInteractor.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>().attachTransform = directInteractorStabilizer.transform;
                    directInteractorStabilizer.GetComponent<XRTransformStabilizer>().targetTransform = directInteractor.transform;

                    handObj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRInteractionGroup>().startingGroupMembers.Add(directInteractor.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>());
                }
            }

            if (isRaycast)
            {
                if (!handObj.GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>())
                {
                    GameObject ray = LynxBuildSettings.InstantiateGameObjectByPath(LynxBuildSettings.LYNX_MODULES_PATH, STR_LYNX_RAY, handObj.transform);
                    AssetDatabase.LoadAssetAtPath<Preset>(Directory.GetFiles(LynxBuildSettings.LYNX_CORE_PATH, isLeftHand ? STR_LYNX_LEFT_RAY_PRESET : STR_LYNX_RIGHT_RAY_PRESET, SearchOption.AllDirectories)[0]).ApplyTo(ray.GetComponent<ActionBasedController>());

                    // Stabilizer
                    GameObject rayStabilizer = LynxBuildSettings.InstantiateGameObjectByPath(LynxBuildSettings.LYNX_MODULES_PATH, STR_LYNX_RAY_STABILIZER, handObj.transform);
                    ray.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>().attachTransform = rayStabilizer.GetComponentsInChildren<Transform>()[1];
                    ray.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>().rayOriginTransform = rayStabilizer.transform;
                    rayStabilizer.GetComponent<XRTransformStabilizer>().targetTransform = ray.transform;

                    handObj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRInteractionGroup>().startingGroupMembers.Add(ray.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>());
                }
            }

            Undo.RegisterCreatedObjectUndo(handObj.gameObject, "Lynx hand");

        }

        public static void AddHandtrackingPrefabs(bool isUnityEvent, bool isDirectInteraction, bool isRaycast, bool cbHandsVisualizer, bool ghostHandsVisualizer, bool lineHandsVisualizer, bool interfacePointer)
        {
            //AddAndroidComManager();
            //AddAudioVolumeIndicator();

            // Get camera parent
            Transform parent = Camera.main.transform.parent;
            if (!parent)
            {
                // Create gameobject as a parent if missing
                parent = new GameObject().transform;

                parent.gameObject.name = "Lynx Hands";
                parent.position = Camera.main.transform.position;
                parent.rotation = Camera.main.transform.rotation;
            }

            // Add event system
            GameObject eventSystem = null;
            XRUIInputModule uiInputModule = parent.GetComponentInChildren<XRUIInputModule>();
            if (!uiInputModule)
                eventSystem = LynxBuildSettings.InstantiateGameObjectByPath(LynxBuildSettings.LYNX_MODULES_PATH, STR_LYNX_EVENT_SYSTEM, parent);
            else
                eventSystem = uiInputModule.gameObject;
            Undo.RegisterCreatedObjectUndo(eventSystem, "XR Event System");


            // Add hands
            GenerateHand(parent, isUnityEvent, isDirectInteraction, isRaycast, true);
            GenerateHand(parent, isUnityEvent, isDirectInteraction, isRaycast, false);

            if (cbHandsVisualizer)
            {
                if (ghostHandsVisualizer)
                {
                    GameObject handsVisualizerLeftObj = LynxBuildSettings.InstantiateGameObjectByPath(LynxBuildSettings.LYNX_MODULES_PATH, STR_LYNX_HAND_LEFT_VISUALIZER, parent);
                    Undo.RegisterCreatedObjectUndo(handsVisualizerLeftObj, "Ghost hand visualizer Left");

                    GameObject handsVisualizerRightObj = LynxBuildSettings.InstantiateGameObjectByPath(LynxBuildSettings.LYNX_MODULES_PATH, STR_LYNX_HAND_RIGHT_VISUALIZER, parent);
                    Undo.RegisterCreatedObjectUndo(handsVisualizerRightObj, "Right hand visualizer Right");
                }

                if (lineHandsVisualizer)
                {
                    GameObject lineHandsObj = LynxBuildSettings.InstantiateGameObjectByPath(LynxBuildSettings.LYNX_MODULES_PATH, STR_LYNX_LINE_HANDS_VISUALIZER, parent);
                    Undo.RegisterCreatedObjectUndo(lineHandsObj, "Line hands visualizer");
                }
            }

            if (interfacePointer)
            {
                if(!eventSystem.GetComponent<LynxUIPointerManager>())
                    eventSystem.AddComponent<LynxUIPointerManager>();
            }

            Debug.Log($"Hands added under {parent.name}");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(parent, true);
        }


        /// <summary>
        /// Window asking the user what features to use with the handtracking.
        /// </summary>
        public class HandtrackingAddWindow : EditorWindow
        {
            bool cbUnityEvents = true;
            bool cbDirectInteractor = true;
            bool cbRaycasting = true;
            bool cbHandsVisualizer = true;
            bool cbGhostHandsVisualizer = true;
            bool cbLineHandsVisualizer = false;
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
                if(cbHandsVisualizer)
                {
                    cbGhostHandsVisualizer = EditorGUILayout.Toggle("\t- Ghost Hands", cbGhostHandsVisualizer);
                    cbLineHandsVisualizer = EditorGUILayout.Toggle("\t- Line Hands", cbLineHandsVisualizer);
                }

                if (cbDirectInteractor)
                    cbInterfacePointer = EditorGUILayout.Toggle("UI pointer", cbInterfacePointer) && cbDirectInteractor;
#endif

                GUILayout.Space(20);
                if (GUILayout.Button("Add"))
                {

                    AddHandtrackingPrefabs(cbUnityEvents, cbDirectInteractor, cbRaycasting, cbHandsVisualizer, cbGhostHandsVisualizer, cbLineHandsVisualizer, cbInterfacePointer);
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