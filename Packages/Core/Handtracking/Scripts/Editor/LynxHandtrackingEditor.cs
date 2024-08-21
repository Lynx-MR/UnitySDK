/**
 * @file LynxHandtrackingEditor.cs
 *
 * @author Geoffrey Marhuenda
 *
 * @brief Add lynx handtracking feature into Unity Editor menu to help configuration and integration in the scene.
 */
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

#if LYNX_XRI
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
#endif

namespace Lynx
{
    public class LynxHandtrackingEditor
    {
        private const string STR_LYNX_LINE_HANDS_VISUALIZER = "Lynx Line Hands Visualizer.prefab";
        private const string STR_TELEPORT_INTERACTOR = "Teleport Interactor.prefab";
        private const string STR_LYNX_HAND_MENU = "Lynx Hand Menu.prefab";

        private static InputActionAsset m_actionAsset = null;
        public static InputActionAsset ActionAsset {
            get {
                if (m_actionAsset == null)
                {
                    string[] actionAssets = Directory.GetFiles(Application.dataPath, "XRI Default Input Actions.inputactions", SearchOption.AllDirectories);
                    if(actionAssets.Length > 0)
                        m_actionAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets" + actionAssets[0].Substring(Application.dataPath.Length));
                    else
                        Debug.LogError("Failed to load \"XRI Default Input Actions.inputactions\". Missing samples: XR Interaction Toolkit - Starter Assets");
                }

                return m_actionAsset;

            }
        }

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

            string[] paths = Directory.GetFiles(Application.dataPath, "Menu Hands Follow Preset.asset", SearchOption.AllDirectories);

            // File does not exists (probably due to missing required dependencies)
            if (paths.Length == 0)
            {
                Debug.LogError("Failed to add Hand Menu. Missing sample: XR Interaction Toolkit - Hands Interaction Demo");
                return;
            }

            GameObject handMenu = LynxBuildSettings.InstantiateGameObjectByPath(LynxBuildSettings.LYNX_CORE_PATH, STR_LYNX_HAND_MENU, Camera.main.transform.parent);
            Undo.RegisterCreatedObjectUndo(handMenu, "Hand Menu");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        public static void GenerateHand(Transform parent, bool defaultHands, bool ghostHands, bool teleportInteractor, bool isLeftHand = false)
        {
            // Declare all paths based on handedness
            string handednessStr;
            InteractorHandedness eHandedness;
            if(isLeftHand)
            {
                handednessStr = "Left";
                eHandedness = InteractorHandedness.Left;
            }
            else
            {
                handednessStr = "Right";
                eHandedness = InteractorHandedness.Right;
            }
            
            string handVisualizerPath = $"{handednessStr} Hand Interaction Visual.prefab";
            string handNearFarInteractor = $"{handednessStr}_NearFarInteractor.prefab";
            string ghostHandStr = $"Lynx {handednessStr} Hand Visualizer.prefab";
            const string pinchPointName = "Pinch point";

            // Generate hand base
            string handName = $"{handednessStr} Hand";
            Transform handObj = parent.Find(handName);
            if (!handObj)
            {
                handObj = new GameObject(handName).transform;
                handObj.parent = parent;
                handObj.localPosition = Vector3.zero;
                handObj.localRotation = Quaternion.identity;
            }

            // UI interactor
            if (!handObj.GetComponentInChildren<XRPokeInteractor>())
            {
                XRPokeInteractor pokeInteractor = new GameObject($"{handednessStr} poke interactor").AddComponent<XRPokeInteractor>();
                pokeInteractor.transform.parent = handObj;
                pokeInteractor.transform.localPosition = Vector3.zero;
                pokeInteractor.transform.localRotation = Quaternion.identity;
                pokeInteractor.handedness = eHandedness;

                TrackedPoseDriver pokePose = pokeInteractor.gameObject.AddComponent<TrackedPoseDriver>();
                pokePose.positionInput = new InputActionProperty(InputActionReference.Create(ActionAsset.FindAction($"XRI {handednessStr}/Poke Position")));
                pokePose.rotationInput = new InputActionProperty(InputActionReference.Create(ActionAsset.FindAction($"XRI {handednessStr}/Poke Rotation")));
                pokePose.trackingStateInput = new InputActionProperty(InputActionReference.Create(ActionAsset.FindAction($"XRI {handednessStr}/Tracking State")));
            }

            Transform pinchPoint = handObj.Find(pinchPointName);
            if (!pinchPoint)
            {
                // Create pinch point for far interactor
                pinchPoint = new GameObject(pinchPointName).transform;
                pinchPoint.parent = handObj.transform;

                TrackedPoseDriver pinchPose = pinchPoint.gameObject.AddComponent<TrackedPoseDriver>();
                pinchPose.positionInput = new InputActionProperty(InputActionReference.Create(ActionAsset.FindAction($"XRI {handednessStr}/Aim Position")));
                pinchPose.rotationInput = new InputActionProperty(InputActionReference.Create(ActionAsset.FindAction($"XRI {handednessStr}/Aim Rotation")));
                pinchPose.trackingStateInput = new InputActionProperty(InputActionReference.Create(ActionAsset.FindAction($"XRI {handednessStr}/Tracking State")));
            }


            // Direct & Ray interactors
            if (!handObj.GetComponentInChildren<NearFarInteractor>())
            {

                GameObject nearFarInteractorPrefab = LynxBuildSettings.InstantiateGameObjectByPath(Application.dataPath, handNearFarInteractor, handObj.transform);
                if(nearFarInteractorPrefab)
                {
                    NearFarInteractor nearFarInteractor = nearFarInteractorPrefab.GetComponent<NearFarInteractor>();

                    TrackedPoseDriver grabTPD = nearFarInteractor.gameObject.AddComponent<TrackedPoseDriver>();
                    grabTPD.positionInput = new InputActionProperty(InputActionReference.Create(ActionAsset.FindAction($"XRI {handednessStr}/Pinch Position")));
                    grabTPD.rotationInput = new InputActionProperty(InputActionReference.Create(ActionAsset.FindAction($"XRI {handednessStr}/Rotation")));
                    grabTPD.trackingStateInput = new InputActionProperty(InputActionReference.Create(ActionAsset.FindAction($"XRI {handednessStr}/Tracking State")));

                    nearFarInteractor.GetComponent<CurveInteractionCaster>().castOrigin = pinchPoint;

                    CurveVisualController curveCtrl = nearFarInteractor.GetComponentInChildren<CurveVisualController>();
                    curveCtrl.overrideLineOrigin = true;
                }
                else
                {
                    Debug.LogError($"Failed to load \"{handNearFarInteractor}\". Missing sample: XR Interaction Toolkit - Starter Assets");
                }
            }

            if(teleportInteractor && !handObj.Find(STR_TELEPORT_INTERACTOR))
            {
                GameObject teleportInteractorObject = LynxBuildSettings.InstantiateGameObjectByPath(Application.dataPath, STR_TELEPORT_INTERACTOR, handObj);
                if (teleportInteractorObject)
                {
                    XRRayInteractor rayInteractor = teleportInteractorObject.GetComponent<XRRayInteractor>();
                    rayInteractor.handedness = eHandedness;
                    rayInteractor.rayOriginTransform = pinchPoint;
                    rayInteractor.selectInput.inputActionReferencePerformed = InputActionReference.Create(ActionAsset.FindAction($"XRI {handednessStr} Interaction/Select"));
                    rayInteractor.activateInput.inputActionReferencePerformed = InputActionReference.Create(ActionAsset.FindAction($"XRI {handednessStr} Interaction/Activate"));
                }
            }

            // Visualizers
            if (defaultHands)
            {
                if(LynxBuildSettings.InstantiateGameObjectByPath(Application.dataPath, handVisualizerPath, handObj) == null)
                    Debug.LogError($"Failed to load \"{handVisualizerPath}\". Default hand visualizers requires Unity sample: XR Hands - HandVisualizer");
            }

            if (ghostHands)
                LynxBuildSettings.InstantiateGameObjectByPath(LynxBuildSettings.LYNX_CORE_PATH, ghostHandStr, handObj);

            Undo.RegisterCreatedObjectUndo(handObj.gameObject, handName);
        }

        public static void AddHandtrackingPrefabs(bool defaultHandsVisualizer, bool ghostHandsVisualizer, bool lineHandsVisualizer, bool teleportInteractors, bool interfacePointer)
        {
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

            if (!ActionAsset)
                return;

            GenerateHand(parent, defaultHandsVisualizer, ghostHandsVisualizer, teleportInteractors,true);
            GenerateHand(parent, defaultHandsVisualizer, ghostHandsVisualizer, teleportInteractors, false);

            // Add line hands visualizer (same gameobject for both hands)
            if (lineHandsVisualizer)
            {
                GameObject lineHands = LynxBuildSettings.InstantiateGameObjectByPath(LynxBuildSettings.LYNX_CORE_PATH, STR_LYNX_LINE_HANDS_VISUALIZER, parent);
                Undo.RegisterCreatedObjectUndo(lineHands, "Lynx line hands");
            }

            // Add teleport provider
            if(teleportInteractors)
            {
                if (LynxBuildSettings.FindObjectsOfTypeAll<TeleportationProvider>().Count == 0)
                {
                    GameObject teleportationProvider = new GameObject("Teleportation provider");
                    teleportationProvider.transform.parent = null;
                    teleportationProvider.transform.localPosition = Vector3.zero;
                    teleportationProvider.transform.localRotation = Quaternion.identity;

                    teleportationProvider.AddComponent<TeleportationProvider>().mediator = teleportationProvider.AddComponent<LocomotionMediator>();
                    Debug.Log("Teleportation provider was missing and added to the scene.");
                }
            }

            // Add the interface pointer to have a visual feedback
            if (interfacePointer)
            {
                if (!parent.GetComponentInChildren<LynxUIPointerManager>())
                {
                    GameObject obj = new GameObject("Lynx interface pointer", typeof(LynxUIPointerManager));
                    obj.transform.parent = parent;
                    obj.transform.localPosition = Vector3.zero;
                    obj.transform.localRotation = Quaternion.identity;

                    Undo.RegisterCreatedObjectUndo(obj, "Lynx interface pointer");
                }
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
            bool cbHandsVisualizer = true;
            bool cbDefaultHandsVisualizer = true;
            bool cbGhostHandsVisualizer = false;
            bool cbLineHandsVisualizer = false;
            bool cbTeleportInteractors = false;
            bool cbInterfacePointer = false;

            void OnGUI()
            {
                GUILayout.Space(20);
                GUILayout.Label("Add handtracking to the scene.\nPlease define which interaction types you want to use.", EditorStyles.label);

                GUILayout.Space(20);
                //cbUnityEvents = EditorGUILayout.Toggle("Unity events", cbUnityEvents);
                GUILayout.Label("Unity UI compatibility is enabled.", EditorStyles.label);
#if LYNX_XRI
                cbHandsVisualizer = EditorGUILayout.Toggle("Hands visualizer", cbHandsVisualizer);
                if(cbHandsVisualizer)
                {
                    cbDefaultHandsVisualizer = EditorGUILayout.Toggle("\t- Default Hands", cbDefaultHandsVisualizer);
                    cbGhostHandsVisualizer = EditorGUILayout.Toggle("\t- Ghost Hands", cbGhostHandsVisualizer);
                    cbLineHandsVisualizer = EditorGUILayout.Toggle("\t- Line Hands", cbLineHandsVisualizer);
                }
                else
                {
                    cbDefaultHandsVisualizer = false;
                    cbGhostHandsVisualizer = false;
                    cbLineHandsVisualizer = false;
                }

                cbTeleportInteractors = EditorGUILayout.Toggle("Teleport interactors", cbTeleportInteractors);

                cbInterfacePointer = EditorGUILayout.Toggle("UI pointer", cbInterfacePointer);
#endif

                GUILayout.Space(20);
                if (GUILayout.Button("Add"))
                {
                    AddHandtrackingPrefabs(cbDefaultHandsVisualizer, cbGhostHandsVisualizer, cbLineHandsVisualizer, cbTeleportInteractors, cbInterfacePointer);
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