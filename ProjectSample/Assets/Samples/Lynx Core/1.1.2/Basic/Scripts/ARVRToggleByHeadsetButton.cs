using UnityEngine;

namespace Lynx
{
    public class ARVRToggleByHeadsetButton : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyUp(LynxHardwareButtonMng.LYNX_BUTTON))
                LynxAPI.ToggleAROnly();
        }
    }
}