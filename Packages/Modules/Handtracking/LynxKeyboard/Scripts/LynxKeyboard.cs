using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Lynx
{
    /// <summary>
    /// Manages a LynxKeyboard with it's different layers, focused InputField & input handling
    /// </summary>
    public class LynxKeyboard : MonoBehaviour
    {
        //INSPECTOR
        public RectTransform keyboardRect;
        public RectTransform keyboardBackgroundRect;
        public RectTransform inputTextBackgroundRect;
        [Space]
        public TMP_InputField inputPreviewField;
        [Space]
        public List<LynxKeyboardLayer> keyboardLayers = new List<LynxKeyboardLayer>();

        //PUBLIC
        [HideInInspector] public LynxSubKeyboard currentActiveLynxSubKeyboard;

        [HideInInspector] public LynxKeyboardInputField focusedLynxKeyboardInputField;
        [HideInInspector] public bool previewInputFieldLastFocused;

        //PRIVATE
        private UnityAction<string> selectAction;


        private void Start()
        {
            DisableAllSubKeyboards();
            DisableKeyboard();
        }

        private void OnEnable()
        {
            selectAction = (string arg0) => { previewInputFieldLastFocused = true; };
            inputPreviewField.onSelect.AddListener(selectAction);
        }
        private void OnDisable()
        {
            inputPreviewField.onSelect.RemoveListener(selectAction);
        }


        /// <summary>
        /// Activates the keyboard gameobject with its first layer and sets its target input field
        /// </summary>
        /// <param name="targetInputField"></param>
        public void EnableKeyboard(LynxKeyboardInputField targetInputField)
        {
            if (gameObject.activeSelf == false)
                gameObject.SetActive(true);

            focusedLynxKeyboardInputField = targetInputField;
            SwitchKeyboardLayer(0);

            UpdateInputPreviewFieldFromInputField(targetInputField.inputField);
        }
        /// <summary>
        /// Deactivates the keyboard gameobject
        /// </summary>
        public void DisableKeyboard()
        {
            focusedLynxKeyboardInputField = null;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Switches the active layer of the keyboard
        /// </summary>
        /// <param name="layerIndex"></param>
        public void SwitchKeyboardLayer(int layerIndex)
        {
            DisableAllSubKeyboards();

            foreach (LynxKeyboardLayer keyboardLayer in keyboardLayers)
            {
                keyboardLayer.keyboardLayerTransform.gameObject.SetActive(false);
            }
            keyboardLayers[layerIndex].keyboardLayerTransform.gameObject.SetActive(true);
        }
        /// <summary>
        /// Activates a specific subKeyboard
        /// </summary>
        /// <param name="lynxSubKeyboard"></param>
        public void EnableSubKeyboard(LynxSubKeyboard lynxSubKeyboard)
        {
            DisableAllSubKeyboards();

            lynxSubKeyboard.gameObject.SetActive(true);
            currentActiveLynxSubKeyboard = lynxSubKeyboard;
        }
        /// <summary>
        /// Deactivates all the keyboard's subKeyboards
        /// </summary>
        public void DisableAllSubKeyboards()
        {
            foreach (LynxKeyboardLayer keyboardLayer in keyboardLayers)
            {
                DisableLayerSubKeyboards(keyboardLayer);
            }
            currentActiveLynxSubKeyboard = null;
        }
        /// <summary>
        /// Deactivates a specific subKeyboard
        /// </summary>
        /// <param name="lynxKeyboardLayer"></param>
        public void DisableLayerSubKeyboards(LynxKeyboardLayer lynxKeyboardLayer)
        {
            foreach (LynxSubKeyboard lynxSubKeyboard in lynxKeyboardLayer.lynxSubKeyboards)
            {
                //lynxSubKeyboard.gameObject.SetActive(false);
                lynxSubKeyboard.DeactivateSubKeyboard();
            }
        }

        /// <summary>
        /// Handles input events from keyboard keys, either appending text or triggering keyfunction depending on the key input text
        /// </summary>
        /// <param name="keyStringInput"></param>
        public void HandleInput(string keyStringInput)
        {
            if (keyStringInput == null) return;
            else if (keyStringInput.Equals("")) return;
            //layer shifts
            else if (keyStringInput.Equals("layer_base")) SwitchKeyboardLayer(0);
            else if (keyStringInput.Equals("layer_caps")) SwitchKeyboardLayer(1);
            else if (keyStringInput.Equals("layer_symbols")) SwitchKeyboardLayer(2);
            //actions
            else if (keyStringInput.Equals("code_space")) AppendText(" ");
            else if (keyStringInput.Equals("code_tab")) AppendText("\t");
            else if (keyStringInput.Equals("code_enter")) KeyEnter();
            else if (keyStringInput.Equals("code_backspace")) KeyBackspace();
            else if (keyStringInput.Equals("code_delete")) AppendText("");

            else if (keyStringInput.Equals("arrow_up")) AppendText("");
            else if (keyStringInput.Equals("arrow_down")) AppendText("");
            else if (keyStringInput.Equals("arrow_right")) AppendText("");
            else if (keyStringInput.Equals("arrow_left")) AppendText("");
            //normal key character
            else AppendText(keyStringInput);
        }
        /// <summary>
        /// Appends text to both the keyboard focused input and the keyboard preview input field, keeping both synced text-wise
        /// </summary>
        /// <param name="keyStringInput"></param>
        public void AppendText(string keyStringInput)
        {
            if (previewInputFieldLastFocused)
            {
                AppendTextToInputField(keyStringInput, inputPreviewField);
                focusedLynxKeyboardInputField.UpdateInputFieldFromInputField(inputPreviewField);
            }
            else
            {
                AppendTextToInputField(keyStringInput, focusedLynxKeyboardInputField.inputField);
                UpdateInputPreviewFieldFromInputField(focusedLynxKeyboardInputField.inputField);
            }
        }
        /// <summary>
        /// Appends text to an input field, taking into account caret position and current text selection
        /// </summary>
        /// <param name="textToAppend"></param>
        /// <param name="inputField"></param>
        public void AppendTextToInputField(string textToAppend, TMP_InputField inputField)
        {
            string currentText = inputField.text;
            int caretPosition = inputField.caretPosition;

            int selectionStart = inputField.selectionAnchorPosition;
            int selectionEnd = inputField.selectionFocusPosition;

            if (selectionStart != selectionEnd)
            {
                currentText = currentText.Remove(selectionStart, selectionEnd - selectionStart);
                caretPosition = selectionStart;
            }
            currentText = currentText.Insert(caretPosition, textToAppend);

            inputField.text = currentText;
            inputField.caretPosition = caretPosition + textToAppend.Length;
        }
        /// <summary>
        /// Either appends a new line to the focused input field text if its line type allows, or invokes input field onEndEdit and onKeyEnter events
        /// </summary>
        public void KeyEnter()
        {
            bool submitOnEnter = false;
            if (focusedLynxKeyboardInputField.inputField.lineType != TMP_InputField.LineType.MultiLineNewline) submitOnEnter = true;
            if (focusedLynxKeyboardInputField.useOnKeyEnter) submitOnEnter = true;

            if (submitOnEnter)
            {
                focusedLynxKeyboardInputField.inputField.onEndEdit?.Invoke(focusedLynxKeyboardInputField.inputField.text);
                focusedLynxKeyboardInputField.onKeyEnter?.Invoke(focusedLynxKeyboardInputField.inputField.text);
            }
            else
            {
                AppendText("\n");
            }
        }
        /// <summary>
        /// Backspaces in both the keyboard focused input and the keyboard preview input field, keeping both synced text-wise
        /// </summary>
        public void KeyBackspace()
        {
            if (previewInputFieldLastFocused)
            {
                BackspaceTextToInputField(inputPreviewField);
                focusedLynxKeyboardInputField.UpdateInputFieldFromInputField(inputPreviewField);
            }
            else
            {
                BackspaceTextToInputField(focusedLynxKeyboardInputField.inputField);
                UpdateInputPreviewFieldFromInputField(focusedLynxKeyboardInputField.inputField);
            }
        }
        /// <summary>
        /// Removes one character in an input field, taking into account caret position and current text selection
        /// </summary>
        /// <param name="inputField"></param>
        public void BackspaceTextToInputField(TMP_InputField inputField)
        {
            string currentText = inputField.text;
            int caretPosition = inputField.caretPosition;

            int selectionStart = inputField.selectionAnchorPosition;
            int selectionEnd = inputField.selectionFocusPosition;

            if (selectionStart != selectionEnd)
            {
                // Remove the selected text
                int lengthToRemove = Mathf.Abs(selectionEnd - selectionStart);
                int startIndexToRemove = Mathf.Min(selectionStart, selectionEnd);
                currentText = currentText.Remove(startIndexToRemove, lengthToRemove);

                // Set the caret position to the start of the selection
                caretPosition = startIndexToRemove;
            }
            else
            {
                // No selection, so remove one character before the caret if possible
                if (caretPosition > 0)
                {
                    currentText = currentText.Remove(caretPosition - 1, 1);
                    caretPosition--; // Move caret back one position
                }
            }

            inputField.text = currentText;
            inputField.caretPosition = caretPosition;
        }

        /// <summary>
        /// Matches the preview input field state (text, caret, selection) to another input field
        /// </summary>
        /// <param name="inputField"></param>
        public void UpdateInputPreviewFieldFromInputField(TMP_InputField inputField)
        {
            inputPreviewField.text = inputField.text;
            inputPreviewField.caretPosition = inputField.caretPosition;
            inputPreviewField.selectionAnchorPosition = inputField.selectionAnchorPosition;
            inputPreviewField.selectionFocusPosition = inputField.selectionFocusPosition;
        }

    }
}