using Leap.Unity.Interaction;
using TMPro;
using UnityEngine;


namespace Lynx
{
    /// <summary>
    /// Represents a key on the keyboard that has a string value for input.
    /// </summary>

    public class KeyboardValueKey_Leap : MonoBehaviour
    {

        /// <summary>
        /// The default string value for this key.
        /// </summary>
        public string Value;

        /// <summary>
        /// The shifted string value for this key.
        /// </summary>
        public string ShiftValue;

        /// <summary>
        /// Reference to child text element.
        /// </summary>
        //private Text m_Text;
        private TextMeshPro m_Text;

        /// <summary>
        /// Reference to the GameObject's button component.
        /// </summary>
        //private Button m_Button;

        private InteractionButton m_InteractionButton;

        //ProximityDetector m_ProximityDetector;

        /// <summary>
        /// Get the button component.
        /// </summary>
        private void Awake()
        {
            m_InteractionButton = GetComponent<InteractionButton>();

            //m_ProximityDetector = GetComponent<ProximityDetector>(); 
        }

        /// <summary>
        /// Initialize key text, subscribe to the onClick event, and subscribe to keyboard shift event.
        /// </summary>
        private void Start()
        {
            m_Text = gameObject.GetComponentInChildren<TextMeshPro>();
            m_Text.text = Value;


            // changement cedric 29/10/2021 :
            m_InteractionButton.OnPress += FireAppendValue;
            //m_InteractionButton.OnUnpress+=FireAppendValue;

            //m_ProximityDetector.OnProximity.AddListener(FireAppendValueForProximity); // attention ici. 

            LynxVirtualKeyboard_Leap.Instance.OnKeyboardShifted += Shift;
        }

        /// <summary>
        /// Method injected into the button's onClick listener.
        /// </summary>
        public void FireAppendValue()
        {
            LynxVirtualKeyboard_Leap.Instance.AppendValue(Value, ShiftValue);

            if (KeyboardAudioMng.Instance() != null)
                KeyboardAudioMng.Instance().playClickButton();
        }


        /// <summary>
        /// Method injected into the button's onClick listener.
        /// </summary>
        public void FireAppendValueForProximity(GameObject obj)
        {
            Debug.Log("FireAppendValueForProximity" + obj);
            LynxVirtualKeyboard_Leap.Instance.AppendValue(Value, ShiftValue);
        }


        /// <summary>
        /// Called by the Keyboard when the shift key is pressed. Updates the text for this key using the Value and ShiftValue fields.
        /// </summary>
        /// <param name="isShifted">Indicates the state of shift, the key needs to be changed to.</param>
        public void Shift(bool isShifted)
        {
            // Shift value should only be applied if a shift value is present.
            if (isShifted && !string.IsNullOrEmpty(ShiftValue))
            {
                m_Text.text = ShiftValue;
            }
            else
            {
                m_Text.text = Value;
            }
        }
    }
}