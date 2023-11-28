
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


    /// <summary>
    /// Represents a key on the keyboard that has a function.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class KeyboardKeyFunc : MonoBehaviour
    {
        /// <summary>
        /// Possible functionality for a button.
        /// </summary>
        public enum Function
        {
            // Commands
            Enter,
            Tab,
            ABC,
            Symbol,
            Previous,
            Next,
            Close,
            Dictate,

            // Editing
            Shift,
            CapsLock,
            Space,
            Backspace,

            UNDEFINED,
        }

        /// <summary>
        /// Designer specified functionality of a keyboard button.
        /// </summary>
        [SerializeField, FormerlySerializedAs("m_ButtonFunction")] private Function buttonFunction = Function.UNDEFINED;

        public Function ButtonFunction => buttonFunction;

        /// <summary>
        /// Subscribe to the onClick event.
        /// </summary>
        private void Start()
        {
            Button          button    = GetComponent<Button>();
            //PressableButton pressable = GetComponent<PressableButton>();

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(FireFunctionKey);

            // cedric 09/03/2020 : add action onPress : 
            //pressable.ButtonPressed.RemoveAllListeners();
            //pressable.ButtonPressed.AddListener(FireFunctionKey);
        }

        /// <summary>
        /// Method injected into the button's onClick listener.
        /// </summary>
        private void FireFunctionKey()
        {
            LynxVirtualKeyboard.Instance.FunctionKey(this);
        }
    }

