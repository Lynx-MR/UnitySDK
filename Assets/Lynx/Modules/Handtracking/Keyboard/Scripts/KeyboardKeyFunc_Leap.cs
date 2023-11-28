using Leap.Unity.Interaction;
using UnityEngine;
using UnityEngine.Serialization;
using Lynx;

/// <summary>
/// Represents a key on the keyboard that has a function.
/// </summary>
//[RequireComponent(typeof(Button))]
public class KeyboardKeyFunc_Leap : MonoBehaviour
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


        //Button          button    = GetComponent<Button>();
        //PressableButton pressable = GetComponent<PressableButton>();

        InteractionToggle interactionToggle = GetComponent<InteractionToggle>();

        if (interactionToggle != null)
        {
            // let for while toogle behaviour :
            interactionToggle.OnToggle += FireFunctionKey;
            interactionToggle.OnUntoggle += FireFunctionKey;
        }
        else
        {
            
            InteractionButton interactionButton = GetComponent<InteractionButton>();
            interactionButton.OnUnpress += FireFunctionKey;
            

            //ProximityDetector proximityDetector = GetComponent<ProximityDetector>();
            //proximityDetector.OnProximity.AddListener(FireFunctionKeyForProximity); 
        }

        

        //button.onClick.RemoveAllListeners();
        //button.onClick.AddListener(FireFunctionKey);

    }

    /// <summary>
    /// Method injected into the button's onClick listener.
    /// </summary>
    public void FireFunctionKey()
    {
        if (KeyboardAudioMng.Instance() != null)
            KeyboardAudioMng.Instance().playBackButton();

        LynxVirtualKeyboard_Leap.Instance.FunctionKey(this);
        Debug.Log("********* FireFunctionKey ");
    }

    /// <summary>
    /// Method injected into the button's onClick listener.
    /// </summary>
    public void FireFunctionKeyForProximity(GameObject obj)
    {         
        Debug.Log("----------FireFunctionKeyForProximity " + obj);
        LynxVirtualKeyboard_Leap.Instance.FunctionKey(this);
    }
}

