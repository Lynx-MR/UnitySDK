using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Events;

namespace Lynx
{
    /// <summary>
    /// Enables an input field to trigger an associated Lynx Keyboard and recive input from it
    /// </summary>
    public class LynxKeyboardInputField : MonoBehaviour, IPointerUpHandler
    {

        public TMP_InputField inputField;
        public LynxKeyboard lynxKeyboard;
        [Space]
        public bool useOnKeyEnter;
        public UnityEvent<string> onKeyEnter;



        private void Awake()
        {
            inputField = GetComponent<TMP_InputField>();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            CheckAndActivateKeyboard();
        }


        /// <summary>
        /// Checks if the LynxKeyboard is active and enables it if necessary, focusing on the current input field
        /// </summary>
        public void CheckAndActivateKeyboard()
        {            
            if (lynxKeyboard.isActiveAndEnabled == false || lynxKeyboard.focusedLynxKeyboardInputField != this)
            {
                lynxKeyboard.EnableKeyboard(this);
            }
            lynxKeyboard.previewInputFieldLastFocused = false;
        }
        /// <summary>
        /// Deactivates the keyboard unless another input field using it just got selected
        /// </summary>
        public void CheckAndDeactivateKeyboard()
        {
            GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
            bool anotherLynxKeyboardInputFieldGotSelected = (selectedObject != null
                                                          && selectedObject.GetComponent<LynxKeyboardInputField>()
                                                          && selectedObject.GetComponent<LynxKeyboardInputField>() != this);

            if (!anotherLynxKeyboardInputFieldGotSelected)
            {
                lynxKeyboard.DisableKeyboard();
            }
        }

        /// <summary>
        /// Clears the text content of the input field
        /// </summary>
        public void ClearInputField()
        {
            inputField.text = "";
            inputField.caretPosition = 0;
        }
        /// <summary>
        /// Updates the text content of the input field
        /// </summary>
        /// <param name="text"></param>
        public void UpdateInputFieldText(string text)
        {
            inputField.text = text;
        }
        /// <summary>
        /// Updates the input field based on another TMP_InputField's content
        /// </summary>
        /// <param name="targetInputField"></param>
        public void UpdateInputFieldFromInputField(TMP_InputField targetInputField)
        {
            inputField.text = targetInputField.text;
            inputField.caretPosition = targetInputField.caretPosition;
            inputField.selectionAnchorPosition = targetInputField.selectionAnchorPosition;
            inputField.selectionFocusPosition = targetInputField.selectionFocusPosition;
        }

    }
}