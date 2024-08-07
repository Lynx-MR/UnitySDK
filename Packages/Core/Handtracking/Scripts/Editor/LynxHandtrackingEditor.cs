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
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using UnityEngine.XR.Interaction.Toolkit.Samples.Hands;
#endif

namespace Lynx
{
    public class LynxHandtrackingEditor
    {
        private const string STR_LYNX_LINE_HANDS_VISUALIZER = "Lynx Line Hands Visualizer.prefab";


        private const string STR_LYNX_HAND_MENU = "Lynx Hand Menu.prefab";

        private static InputActionAsset m_actionAsset = null;
        public static InputActionAsset ActionAsset {
            get {
                if(m_actionAsset == null)
                    m_actionAsset =  AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets" + Directory.GetFiles(Application.dataPath, "XRI Default Input Actions.inputactions", SearchOption.AllDirectories)[0].Substring(Application.dataPath.Length));

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
            GameObject handMenu = LynxBuildSettings.InstantiateGameObjectByPath(LynxBuildSettings.LYNX_CORE_PATH, STR_LYNX_HAND_MENU, Camera.main.transform.parent);
            Undo.RegisterCreatedObjectUndo(handMenu, "Hand Menu");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        public static void GenerateHand(Transform parent, bool defaultHands, bool ghostHands, bool lineHands, bool isLeftHand = false)
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
            string pinchStabilizerStr = "Pinch Point Stabilized.prefab";
            string ghostHandStr = $"Lynx {handednessStr} Hand Visualizer.prefab";

            


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

                TrackedPoseDriver tpd = pokeInteractor.gameObject.AddComponent<TrackedPoseDriver>();
                tpd.positionInput = new InputActionProperty(InputActionReference.Create(ActionAsset.FindAction($"XRI {handednessStr}/Poke Position")));
                tpd.rotationInput = new InputActionProperty(InputActionReference.Create(ActionAsset.FindAction($"XRI {handednessStr}/Poke Rotation")));
                tpd.trackingStateInput = new InputActionProperty(InputActionReference.Create(ActionAsset.FindAction($"XRI {handednessStr}/Tracking State")));
            }

            // Direct & Ray interactors
            if (!handObj.GetComponentInChildren<NearFarInteractor>())
            {
                NearFarInteractor nearFarInteractor = LynxBuildSettings.InstantiateGameObjectByPath(Application.dataPath, handNearFarInteractor, handObj.transform).GetComponent<NearFarInteractor>();

                TrackedPoseDriver grabTPD = nearFarInteractor.gameObject.AddComponent<TrackedPoseDriver>();
                grabTPD.positionInput = new InputActionProperty(InputActionReference.Create(ActionAsset.FindAction($"XRI {handednessStr}/Pinch Position")));
                grabTPD.rotationInput = new InputActionProperty(InputActionReference.Create(ActionAsset.FindAction($"XRI {handednessStr}/Rotation")));
                grabTPD.trackingStateInput = new InputActionProperty(InputActionReference.Create(ActionAsset.FindAction($"XRI {handednessStr}/Tracking State")));

                //nearFarInteractor.GetComponent<InteractionAttachController>().


                PinchPointFollow pinchPoint = LynxBuildSettings.InstantiateGameObjectByPath(Application.dataPath, pinchStabilizerStr, handObj.transform).GetComponent<PinchPointFollow>();
                TrackedPoseDriver tpd = pinchPoint.gameObject.AddComponent<TrackedPoseDriver>();
                tpd.positionInput = new InputActionProperty(InputActionReference.Create(ActionAsset.FindAction($"XRI {handednessStr}/Aim Position")));
                tpd.rotationInput = new InputActionProperty(InputActionReference.Create(ActionAsset.FindAction($"XRI {handednessStr}/Aim Rotation")));
                tpd.trackingStateInput = new InputActionProperty(InputActionReference.Create(ActionAsset.FindAction($"XRI {handednessStr}/Tracking State")));

                nearFarInteractor.GetComponent<CurveInteractionCaster>().castOrigin = pinchPoint.transform;

                CurveVisualController curveCtrl = nearFarInteractor.GetComponentInChildren<CurveVisualController>();
                curveCtrl.overrideLineOrigin = true;
                curveCtrl.lineOriginTransform = pinchPoint.transform;
            }

            // Visualizers
            if(defaultHands)
                LynxBuildSettings.InstantiateGameObjectByPath(Application.dataPath, handVisualizerPath, handObj);

            if (ghostHands)
                LynxBuildSettings.InstantiateGameObjectByPath(LynxBuildSettings.LYNX_CORE_PATH, ghostHandStr, handObj);

            Undo.RegisterCreatedObjectUndo(handObj.gameObject, handName);
        }

        public static void AddHandtrackingPrefabs(bool defaultHandsVisualizer, bool ghostHandsVisualizer, bool lineHandsVisualizer, bool interfacePointer)
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

            GenerateHand(parent, defaultHandsVisualizer, ghostHandsVisualizer, lineHandsVisualizer, true);
            GenerateHand(parent, defaultHandsVisualizer, ghostHandsVisualizer, lineHandsVisualizer, false);

            // Add line hands visualizer (same gameobject for both hands)
            if (lineHandsVisualizer)
            {
                GameObject lineHands = LynxBuildSettings.InstantiateGameObjectByPath(LynxBuildSettings.LYNX_CORE_PATH, STR_LYNX_LINE_HANDS_VISUALIZER, parent);
                Undo.RegisterCreatedObjectUndo(lineHands, "Lynx line hands");
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

                cbInterfacePointer = EditorGUILayout.Toggle("UI pointer", cbInterfacePointer);
#endif

                GUILayout.Space(20);
                if (GUILayout.Button("Add"))
                {

                    AddHandtrackingPrefabs(cbDefaultHandsVisualizer, cbGhostHandsVisualizer, cbLineHandsVisualizer, cbInterfacePointer);
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