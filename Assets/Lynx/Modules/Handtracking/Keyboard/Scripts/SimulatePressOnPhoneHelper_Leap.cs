using UnityEngine;

namespace Lynx
{
    public class SimulatePressOnPhoneHelper_Leap : MonoBehaviour
    {
        //public UnityEvent OnClick = new UnityEvent();

        void OnMouseDown()
        {
            //OnClick.Invoke();
            KeyboardValueKey_Leap keyboardValueKey_Leap = GetComponentInChildren<KeyboardValueKey_Leap>();

            if (keyboardValueKey_Leap != null)
            {
                keyboardValueKey_Leap.FireAppendValue();
            }
            else
            {
                KeyboardKeyFunc_Leap keyboardKeyFunc_Leap = GetComponentInChildren<KeyboardKeyFunc_Leap>();

                if (keyboardKeyFunc_Leap != null)
                {
                    keyboardKeyFunc_Leap.FireFunctionKey();
                }
            }
        }
    }
}