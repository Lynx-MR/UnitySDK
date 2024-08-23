

using UnityEditor.SceneManagement;
using UnityEditor;


using UnityEngine;
/**
 * @file LynxMenuImporter.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief Used to integrate LynxMenu into the scene from Editor (should be removed once the Lynx menu managed by the system).
 */

namespace Lynx
{
    public class LynxMenuImporter
    {
        public const string STR_LYNX_MENU = "LynxMenu.prefab";


        [MenuItem("Lynx/Add Lynx Menu", false, 300)]
        public static void AddLynxMenu()
        {
            GameObject lynxMenu = LynxBuildSettings.InstantiateGameObjectByPath(LynxBuildSettings.LYNX_CORE_PATH, STR_LYNX_MENU, null);
            Undo.RegisterCreatedObjectUndo(lynxMenu, "Lynx Menu");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        [MenuItem("GameObject/Lynx/Lynx Menu", false, 200)]
        public static void AddLynxMenuContextMenu()
        {
            AddLynxMenu();
        }
    }
}