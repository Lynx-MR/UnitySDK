using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Lynx
{
    public class LynxGazeInteractorEditor
    {
        private const string STR_GazeInteractor = "LynxGazeInteractor.prefab";

        [MenuItem("GameObject/Lynx/Inputs/Add gaze interactor", false, 200)]
        public static void AddGazeInteractorContextMenu()
        {
            AddGazeInteractor();
        }

        [MenuItem("Lynx/Inputs/Add gaze interactor", false, 200)]
        public static void AddGazeInteractorMenu()
        {
            AddGazeInteractor();
        }

        public static void AddGazeInteractor()
        {
            // Get camera parent
            Transform parent = Camera.main.transform.parent;

            // Add Gaze Interactor
            GameObject gazeInteractor = LynxBuildSettings.InstantiateGameObjectByPath(STR_GazeInteractor, parent);
            Debug.Log($"Gaze Interactor added under {parent.name}");

            // Update scene state
            Undo.RegisterCreatedObjectUndo(gazeInteractor, "Gaze Interactor");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}