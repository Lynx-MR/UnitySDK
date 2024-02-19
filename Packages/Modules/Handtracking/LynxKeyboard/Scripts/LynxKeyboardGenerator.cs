using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Lynx
{
    /// <summary>
    /// Utility class used to dynamically generate a keyboard layout based on provided data
    /// </summary>
    public class LynxKeyboardGenerator : MonoBehaviour
    {
        [Header("Target Keyboard")]
        public LynxKeyboard lynxKeyboard;
        [Header("Prefabs")]
        public LynxKeyboardKey lynxKeyboardKeyPrefab;
        public GameObject subKeyboardBackgroundPrefab;
        [Header("Layout")]
        public LynxKeyboardLayoutSO lynxKeyboardLayoutSO;
        public float baseSize = 1f;
        public float padding = 0.1f;
        public float spacing = 0.1f;


        /// <summary>
        /// Clears the existing keyboard and generates a new one based on
        /// the <see cref="lynxKeyboardKeyPrefab"/> and <see cref="subKeyboardBackgroundPrefab"/> prefabs,  
        /// the specified layout in <see cref="lynxKeyboardLayoutSO"/>, and 
        /// the <see cref="baseSize"/>,  <see cref="padding"/> and <see cref="spacing"/> values
        /// Warning, will unpack the keyboard prefab object in the scene
        /// </summary>
        [ContextMenu("GenerateKeyboard()")]
        public void GenerateKeyboard()
        {
#if UNITY_EDITOR
            if (PrefabUtility.IsPartOfPrefabInstance(lynxKeyboard.gameObject))
            {
                PrefabUtility.UnpackPrefabInstance(lynxKeyboard.gameObject, PrefabUnpackMode.Completely, InteractionMode.UserAction);
            }
#endif
            ClearKeyboard(lynxKeyboard);

            GenerateKeyboardLayout(lynxKeyboardLayoutSO.keyboardLayout, lynxKeyboard, padding);

            FitKeyboardRectAndLayersToLayout(lynxKeyboard, padding);
            lynxKeyboard.keyboardBackgroundRect.sizeDelta = lynxKeyboard.keyboardRect.sizeDelta;
            lynxKeyboard.inputTextBackgroundRect.sizeDelta = new Vector2(lynxKeyboard.keyboardRect.sizeDelta.x, lynxKeyboard.inputTextBackgroundRect.sizeDelta.y);

            lynxKeyboard.SwitchKeyboardLayer(0);
        }

        /// <summary>
        /// Clears the keyboard by destroying its existing layers
        /// </summary>
        /// <param name="lynxKeyboard"></param>
        private void ClearKeyboard(LynxKeyboard lynxKeyboard)
        {
            if (lynxKeyboard.keyboardLayers != null)
            {
                foreach (LynxKeyboardLayer keyboardLayer in lynxKeyboard.keyboardLayers)
                {
                    if (keyboardLayer.keyboardLayerTransform) StartCoroutine(DestroyCoroutine(keyboardLayer.keyboardLayerTransform.gameObject));
                }
                lynxKeyboard.keyboardLayers.Clear();
            }
        }
        IEnumerator DestroyCoroutine(Object target)
        {
            yield return new WaitForSeconds(0);
            DestroyImmediate(target);
        }

        /// <summary>
        /// Generates keyboard layout layer by layer, based on provided layout data.
        /// </summary>
        /// <param name="keyboardLayout"></param>
        /// <param name="lynxKeyboard"></param>
        /// <param name="padding"></param>
        public void GenerateKeyboardLayout(KeyboardLayout keyboardLayout, LynxKeyboard lynxKeyboard, float padding = 0f)
        {
            lynxKeyboard.keyboardLayers = new List<LynxKeyboardLayer>();

            for (int i = 0; i < keyboardLayout.keyboardLayers.Count; i++)
            {
                GameObject keyboardLayer = new GameObject("Layer : " + keyboardLayout.keyboardLayers[i].name);
                keyboardLayer.transform.parent = lynxKeyboard.keyboardRect;
                keyboardLayer.transform.SetSiblingIndex(i);
                keyboardLayer.transform.localPosition = Vector3.zero;

                RectTransform keyboardLayerRectTransform = keyboardLayer.AddComponent<RectTransform>();
                keyboardLayerRectTransform.pivot = new Vector2(0f, 1f);
                keyboardLayerRectTransform.anchorMin = new Vector2(0, 1);
                keyboardLayerRectTransform.anchorMax = new Vector2(0, 1);
                keyboardLayerRectTransform.sizeDelta = new Vector2(lynxKeyboard.keyboardRect.rect.width, lynxKeyboard.keyboardRect.rect.height);
                keyboardLayerRectTransform.anchoredPosition = Vector2.zero;

                LynxKeyboardLayer lynxKeyboardLayer = new LynxKeyboardLayer();
                lynxKeyboardLayer.keyboardLayerTransform = keyboardLayerRectTransform;
                lynxKeyboardLayer.keyboardRowTransforms = new List<RectTransform>();
                lynxKeyboardLayer.lynxKeyboardKeys = new List<LynxKeyboardKey>();
                lynxKeyboardLayer.lynxSubKeyboards = new List<LynxSubKeyboard>();

                lynxKeyboard.keyboardLayers.Add(lynxKeyboardLayer);

                GenerateKeyboardLayer(keyboardLayout.keyboardLayers[i], lynxKeyboardLayer, padding);

                keyboardLayer.transform.localScale = Vector3.one;
            }


        }
        /// <summary>
        /// Generates keyboard layer with its rows of keys and subkeyboards, based on provided layer data.
        /// </summary>
        /// <param name="keyboardLayer"></param>
        /// <param name="lynxKeyboardLayer"></param>
        /// <param name="padding"></param>
        public void GenerateKeyboardLayer(KeyboardLayer keyboardLayer, LynxKeyboardLayer lynxKeyboardLayer, float padding = 0f)
        {
            //Create keyboard layer rows
            for (int i = 0; i < keyboardLayer.keyboardRows.Count; i++)
            {
                GenerateNextKeyboardRow(keyboardLayer.keyboardRows[i], lynxKeyboardLayer.keyboardLayerTransform, lynxKeyboardLayer.keyboardRowTransforms, padding);

                //Create keyboard layer row keys
                for (int j = 0; j < keyboardLayer.keyboardRows[i].keyboardRowKeys.Count; j++)
                {
                    GenerateNextKeyboardKey(keyboardLayer.keyboardRows[i].keyboardRowKeys[j], lynxKeyboardLayer.keyboardRowTransforms[i], lynxKeyboardLayer.lynxKeyboardKeys, padding); 
                }
            }

            //Create SubKeyboard parent
            GameObject subKeyboardsParent = new GameObject("SubKeyboards");
            subKeyboardsParent.transform.parent = lynxKeyboardLayer.keyboardLayerTransform;
            subKeyboardsParent.transform.localPosition = Vector3.zero;

            RectTransform subKeyboardsParentRectTransform = subKeyboardsParent.AddComponent<RectTransform>();
            subKeyboardsParentRectTransform.pivot = new Vector2(0f, 1f);
            subKeyboardsParentRectTransform.anchorMin = new Vector2(0, 1);
            subKeyboardsParentRectTransform.anchorMax = new Vector2(0, 1);
            subKeyboardsParentRectTransform.sizeDelta = new Vector2(lynxKeyboardLayer.keyboardLayerTransform.rect.width, lynxKeyboardLayer.keyboardLayerTransform.rect.height);
            subKeyboardsParentRectTransform.anchoredPosition = Vector2.zero;

            //Create subkeyboards
            foreach (LynxKeyboardKey lynxKeyboardKey in lynxKeyboardLayer.lynxKeyboardKeys)
            {
                if (lynxKeyboardKey.subKeyboardLayout.keyboardRows.Count > 0)
                {
                    GenerateSubKeyboard(lynxKeyboardKey.subKeyboardLayout, subKeyboardsParent.transform, lynxKeyboardKey, lynxKeyboardLayer.lynxSubKeyboards);
                }
            }
        }
        /// <summary>
        /// Generates the next keyboard row inside a keyboard layer based on provided row data, and positiones it based on currently instanced rows
        /// </summary>
        /// <param name="rowData"></param>
        /// <param name="targetLayerRectTransform"></param>
        /// <param name="keyboardRowTransforms"></param>
        /// <param name="padding"></param>
        public void GenerateNextKeyboardRow(Row rowData, RectTransform targetLayerRectTransform, List<RectTransform> keyboardRowTransforms, float padding = 0f)
        {
            GameObject keyboardRow = new GameObject();
            if (rowData is KeyboardRow keyboardRowData) keyboardRow.name = "Row : " + keyboardRowData.name;
            else keyboardRow.name = "keys";
            keyboardRow.transform.parent = targetLayerRectTransform;
            keyboardRow.transform.SetSiblingIndex(keyboardRowTransforms.Count);
            keyboardRow.transform.localPosition = Vector3.zero;

            RectTransform keyboardRowRectTransform = keyboardRow.AddComponent<RectTransform>();
            keyboardRowRectTransform.pivot = new Vector2(0f, 1f);
            keyboardRowRectTransform.anchorMin = new Vector2(0, 1);
            keyboardRowRectTransform.anchorMax = new Vector2(0, 1);
            keyboardRowRectTransform.offsetMax = new Vector2(targetLayerRectTransform.GetComponent<RectTransform>().rect.width,
                                                             keyboardRowRectTransform.offsetMax.y);

            float rowHeight;
            float rowRelHeight = rowData.relHeight;
            if (rowRelHeight < 1) rowHeight = rowRelHeight * baseSize;
            else rowHeight = rowRelHeight * baseSize + (rowRelHeight - 1) * spacing;
            keyboardRowRectTransform.sizeDelta = new Vector2(keyboardRowRectTransform.sizeDelta.x, rowHeight);

            float rowPosY;
            int rowCount = keyboardRowTransforms.Count;
            if (rowCount == 0) rowPosY = -padding;
            else rowPosY = keyboardRowTransforms[keyboardRowTransforms.Count -1].anchoredPosition.y
                           - keyboardRowTransforms[keyboardRowTransforms.Count - 1].sizeDelta.y
                           - spacing;
            keyboardRowRectTransform.anchoredPosition = new Vector2(0, rowPosY);

            keyboardRowTransforms.Add(keyboardRowRectTransform);
        }
        /// <summary>
        /// Generates the next keyboard key inside a keyboard row based on provided key data, and positiones it based on currently instanced keys in the row
        /// </summary>
        /// <param name="keyData"></param>
        /// <param name="targetRowRectTransform"></param>
        /// <param name="lynxKeyboardKeys"></param>
        /// <param name="padding"></param>
        public void GenerateNextKeyboardKey(Key keyData, RectTransform targetRowRectTransform, List<LynxKeyboardKey> lynxKeyboardKeys, float padding = 0f)
        {
            LynxKeyboardKey keyboardKey = Instantiate(lynxKeyboardKeyPrefab, targetRowRectTransform);
            keyboardKey.transform.localPosition = Vector3.zero;
            if (keyData.inputString == "")
            {
                keyboardKey.gameObject.SetActive(false);
                keyboardKey.gameObject.name = "empty space";
            }
            else
            {
                keyboardKey.gameObject.SetActive(true);
                keyboardKey.UpdateKeyStrings(keyData.inputString, keyData.displayString);
                keyboardKey.gameObject.name = keyData.inputString;
                keyboardKey.lynxKeyboard = lynxKeyboard;
                if (keyData is KeyboardKey keyboardKeyData) keyboardKey.subKeyboardLayout = keyboardKeyData.subKeyboard;
            }

            RectTransform keyboardKeyRectTransform = keyboardKey.GetComponent<RectTransform>();
            keyboardKeyRectTransform.pivot = new Vector2(0, 0.5f);
            keyboardKeyRectTransform.anchorMin = new Vector2(0, 0);
            keyboardKeyRectTransform.anchorMax = new Vector2(0, 1);
            keyboardKeyRectTransform.offsetMin = new Vector2(keyboardKeyRectTransform.offsetMin.x, 0);//set bottom 0
            keyboardKeyRectTransform.offsetMax = new Vector2(keyboardKeyRectTransform.offsetMax.x, -0);//set top 0
            keyboardKeyRectTransform.sizeDelta = new Vector2(baseSize, keyboardKeyRectTransform.sizeDelta.y);

            float keyWidth;
            float keyRelWidth = keyData.relWidth;
            if (keyRelWidth < 1) keyWidth = keyRelWidth * baseSize;
            else keyWidth = keyRelWidth * baseSize + (keyRelWidth - 1) * spacing;
            keyboardKeyRectTransform.sizeDelta = new Vector2(keyWidth, keyboardKeyRectTransform.sizeDelta.y);

            float keyPosX;
            int rowKeyCount = targetRowRectTransform.childCount;
            if (rowKeyCount == 1) keyPosX = padding;
            else keyPosX = targetRowRectTransform.GetChild(rowKeyCount - 2).GetComponent<RectTransform>().anchoredPosition.x
                           + targetRowRectTransform.GetChild(rowKeyCount - 2).GetComponent<RectTransform>().sizeDelta.x
                           + spacing;
            keyboardKeyRectTransform.anchoredPosition = new Vector2(keyPosX, 0);

            lynxKeyboardKeys.Add(keyboardKey);

        }
        /// <summary>
        /// Generates a subKeyboard layout, based on provided layout data, and positiones it based on the subKeyboard trigger key position & layout mode
        /// </summary>
        /// <param name="subKeyboardLayout"></param>
        /// <param name="subKeyboardsParent"></param>
        /// <param name="lynxKeyboardKey"></param>
        /// <param name="targetSubKeyboards"></param>
        public void GenerateSubKeyboard(SubKeyboardLayout subKeyboardLayout, Transform subKeyboardsParent, LynxKeyboardKey lynxKeyboardKey, List<LynxSubKeyboard> targetSubKeyboards)
        {
            //Create subKeyboard in scene
            GameObject subKeyboard = new GameObject($"{lynxKeyboardKey.keyStringDisplay} subKeyboard ");
            subKeyboard.transform.parent = subKeyboardsParent;
            subKeyboard.transform.localPosition = Vector3.zero;

            RectTransform subKeyboardRectTransform = subKeyboard.AddComponent<RectTransform>();
            subKeyboardRectTransform.pivot = new Vector2(0f, 1f);
            subKeyboardRectTransform.anchorMin = new Vector2(0, 1);
            subKeyboardRectTransform.anchorMax = new Vector2(0, 1);
            subKeyboardRectTransform.anchoredPosition = subKeyboardsParent.GetComponent<RectTransform>().anchoredPosition;
            subKeyboardRectTransform.sizeDelta = subKeyboardsParent.GetComponent<RectTransform>().sizeDelta;

            LynxSubKeyboard lynxSubKeyboard = subKeyboard.AddComponent<LynxSubKeyboard>();
            lynxSubKeyboard.lynxKeyboardKeyOrigin = lynxKeyboardKey;
            lynxKeyboardKey.lynxSubKeyboard = lynxSubKeyboard;

            targetSubKeyboards.Add(lynxSubKeyboard);


            //Create sub keyboard layer rows
            for (int i = 0; i < subKeyboardLayout.keyboardRows.Count; i++)
            {
                GenerateNextKeyboardRow(subKeyboardLayout.keyboardRows[i], subKeyboardRectTransform, lynxSubKeyboard.subKeyboardRows);

                //Create keyboard layer row keys
                for (int j = 0; j < subKeyboardLayout.keyboardRows[i].keyboardRowKeys.Count; j++)
                {
                    GenerateNextKeyboardKey(subKeyboardLayout.keyboardRows[i].keyboardRowKeys[j], lynxSubKeyboard.subKeyboardRows[i], lynxSubKeyboard.subKeyboardKeys);
                }
            }

            FitKeyboardRectAndRowsToKeyboardLayout(subKeyboardRectTransform, lynxSubKeyboard.subKeyboardRows, lynxSubKeyboard.subKeyboardKeys);

            //place subKeyboard using keyboardKey pos & placement enum
            if (subKeyboardLayout.positioning == SubKeyboardPositioning.TopFromBottomLeftCorner)
            {
                subKeyboardRectTransform.anchoredPosition = lynxKeyboardKey.GetComponent<RectTransform>().anchoredPosition
                                                          + lynxKeyboardKey.GetComponent<RectTransform>().parent.GetComponent<RectTransform>().anchoredPosition;
                subKeyboardRectTransform.anchoredPosition += new Vector2(0, subKeyboardRectTransform.rect.height + spacing);
            }
            else if (subKeyboardLayout.positioning == SubKeyboardPositioning.TopFromBottomRightCorner)
            {
                subKeyboardRectTransform.anchoredPosition = lynxKeyboardKey.GetComponent<RectTransform>().anchoredPosition
                                                          + lynxKeyboardKey.GetComponent<RectTransform>().parent.GetComponent<RectTransform>().anchoredPosition;
                subKeyboardRectTransform.anchoredPosition += new Vector2(0, subKeyboardRectTransform.rect.height + spacing);
                subKeyboardRectTransform.anchoredPosition += new Vector2(-subKeyboardRectTransform.rect.width + lynxKeyboardKey.GetComponent<RectTransform>().rect.width, 0);
            }

            //create & add background with outside padding
            GameObject subKeyboardBackground = Instantiate(subKeyboardBackgroundPrefab, subKeyboard.transform);
            subKeyboardBackground.name = "background";
            subKeyboardBackground.transform.SetSiblingIndex(0);

            RectTransform subKeyboardBackgroundRectTransform = subKeyboardBackground.GetComponent<RectTransform>();
            subKeyboardBackgroundRectTransform.pivot = new Vector2(0.5f, 0.5f);
            subKeyboardBackgroundRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            subKeyboardBackgroundRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            subKeyboardBackgroundRectTransform.anchoredPosition = Vector2.zero;
            subKeyboardBackgroundRectTransform.sizeDelta = new Vector2(subKeyboardRectTransform.rect.width + 2.5f * padding, subKeyboardRectTransform.rect.height + 2 * padding);

        }

        /// <summary>
        /// Fits a keyboard rectTransform to its rows & keys rectTransforms, then fits the row rect transforms to the keyboard rect transform
        /// </summary>
        /// <param name="keyboardRect"></param>
        /// <param name="lynxKeyboardRows"></param>
        /// <param name="lynxKeyboardKeys"></param>
        /// <param name="padding"></param>
        public void FitKeyboardRectAndRowsToKeyboardLayout(RectTransform keyboardRect, List<RectTransform> lynxKeyboardRows, List<LynxKeyboardKey> lynxKeyboardKeys, float padding = 0f)
        {
            //Fit keyboard rect to last row lower bound
            RectTransform lastRowRect = lynxKeyboardRows[lynxKeyboardRows.Count - 1];
            float lowerBound = - lastRowRect.anchoredPosition.y + lastRowRect.sizeDelta.y;
            keyboardRect.sizeDelta = new Vector2(keyboardRect.sizeDelta.x, lowerBound + padding);

            //Fit keyboard rect to furthest right key bound
            float rightBound = 0;
            foreach (LynxKeyboardKey key in lynxKeyboardKeys)
            {
                RectTransform keyRectTransform = key.gameObject.GetComponent<RectTransform>();
                float keyRightBound = keyRectTransform.anchoredPosition.x + keyRectTransform.sizeDelta.x;
                if (keyRightBound > rightBound) rightBound = keyRightBound;
            }
            keyboardRect.sizeDelta = new Vector2(rightBound + padding, keyboardRect.sizeDelta.y);

            //Fit keyboard rows to furthest right key bound
            foreach (RectTransform rectTransform in lynxKeyboardRows)
            {
                rectTransform.sizeDelta = new Vector2(rightBound + padding, rectTransform.sizeDelta.y);
            }
        }
        /// <summary>
        /// Fits the keyboard rectTransform to its rows & keys rectTransforms, then fits the layers rect transforms to the keyboard rect transform
        /// </summary>
        /// <param name="lynxKeyboard"></param>
        /// <param name="padding"></param>
        public void FitKeyboardRectAndLayersToLayout(LynxKeyboard lynxKeyboard, float padding = 0f)
        {
            //Fit keyboard rect to to furthest right & low bounds in keyboard layers
            float lowerBound = 0;
            float rightBound = 0;
            foreach (LynxKeyboardLayer lynxKeyboardLayer in lynxKeyboard.keyboardLayers)
            {
                RectTransform lastRowRect = lynxKeyboardLayer.keyboardRowTransforms[lynxKeyboardLayer.keyboardRowTransforms.Count - 1];
                float rowLowBound = -lastRowRect.anchoredPosition.y + lastRowRect.sizeDelta.y;
                if (rowLowBound > lowerBound) lowerBound = rowLowBound;

                foreach (LynxKeyboardKey key in lynxKeyboardLayer.lynxKeyboardKeys)
                {
                    RectTransform keyRectTransform = key.gameObject.GetComponent<RectTransform>();
                    float keyRightBound = keyRectTransform.anchoredPosition.x + keyRectTransform.sizeDelta.x;
                    if (keyRightBound > rightBound) rightBound = keyRightBound;
                }
            }

            lynxKeyboard.keyboardRect.sizeDelta = new Vector2(rightBound + padding, lowerBound + padding);

            //Fit keyboard layers & rows to resulting right & low bounds
            foreach (LynxKeyboardLayer lynxKeyboardLayer in lynxKeyboard.keyboardLayers)
            {
                lynxKeyboardLayer.keyboardLayerTransform.sizeDelta = new Vector2(rightBound + padding, lowerBound + padding);

                foreach (RectTransform rowRectTransform in lynxKeyboardLayer.keyboardRowTransforms)
                {
                    rowRectTransform.sizeDelta = new Vector2(rightBound + padding, rowRectTransform.sizeDelta.y);
                }
            }
        }

    }
}