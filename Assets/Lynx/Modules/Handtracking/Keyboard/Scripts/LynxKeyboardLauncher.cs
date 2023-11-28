using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Lynx
{
    /// <summary>
    /// This component links the NonNativeKeyboard to a TMP_InputField
    /// Put it on the TMP_InputField and assign the LynxVirtualKeyboard.prefab
    /// </summary>
    [RequireComponent(typeof(TMP_InputField))]
    public class LynxKeyboardLauncher : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private LynxVirtualKeyboard_Leap keyboard = null;

        public void LaunchKeyboard()
        {
            //Debug.Log("LaunchKeyboard() called");
            keyboard.PresentKeyboard();
            keyboard.OnTextUpdated += UpdateText;
            keyboard.OnClosed += DisableKeyboard;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            keyboard.PresentKeyboard();

            keyboard.OnClosed += DisableKeyboard;
            keyboard.OnTextSubmitted += DisableKeyboard;
            keyboard.OnTextUpdated += UpdateText;
        }

        private void UpdateText(GameObject go, string text)
        {
            TMP_InputField tmp_InputField = GetComponent<TMP_InputField>();
            GetComponent<TMP_InputField>().text = text;
        }

        private void DisableKeyboard(object sender, EventArgs e)
        {
            keyboard.OnTextUpdated -= UpdateText;
            keyboard.OnClosed -= DisableKeyboard;
            keyboard.OnTextSubmitted -= DisableKeyboard;
            keyboard.Close();
        }
    }
}