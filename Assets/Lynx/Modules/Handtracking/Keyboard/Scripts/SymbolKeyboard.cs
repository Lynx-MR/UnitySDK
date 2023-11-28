using UnityEngine;

namespace Lynx
{

    /// <summary>
    /// This class switches back and forth between two symbol boards that otherwise do not fit on the keyboard entirely
    /// </summary>
    public class SymbolKeyboard : MonoBehaviour
    {
        [SerializeField]
        private UnityEngine.UI.Button m_PageBck = null;

        [SerializeField]
        private UnityEngine.UI.Button m_PageFwd = null;

        private void Update()
        {
            // Visual reflection of state.
            m_PageBck.interactable = LynxVirtualKeyboard.Instance.IsShifted;
            m_PageFwd.interactable = !LynxVirtualKeyboard.Instance.IsShifted;
        }
    }

}