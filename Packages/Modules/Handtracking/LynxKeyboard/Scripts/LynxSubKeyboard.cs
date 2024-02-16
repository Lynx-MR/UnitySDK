using System.Collections.Generic;
using UnityEngine;

namespace Lynx
{
    /// <summary>
    /// Manages a subKeyboard with hovered & timer based lifecycle
    /// </summary>
    public class LynxSubKeyboard : MonoBehaviour
    {
        //INSPECTOR
        public LynxKeyboardKey lynxKeyboardKeyOrigin;

        public List<RectTransform> subKeyboardRows = new List<RectTransform>();
        public List<LynxKeyboardKey> subKeyboardKeys = new List<LynxKeyboardKey>();

        //PRIVATE
        [HideInInspector] public float lifeTime = 2;
        [HideInInspector] public float timer = 0;



        private void Update()
        {
            timer += Time.deltaTime;
            if(SubKeyboardKeysHovered()) timer = 0;
            if (timer > lifeTime) DeactivateSubKeyboard();
        }



        /// <summary>
        /// Checks if any keys within the sub-keyboard are being hovered over
        /// </summary>
        /// <returns></returns>
        public bool SubKeyboardKeysHovered()
        {
            bool subKeyboardKeysHovered = false;

            foreach (var subKeyboardKey in subKeyboardKeys)
            {
                if (subKeyboardKey.pointerHoveringKey)
                {
                    subKeyboardKeysHovered = true;
                    return subKeyboardKeysHovered;
                }
            }
            return subKeyboardKeysHovered;
        }
        /// <summary>
        /// Deactivates the sub-keyboard, resetting the timer and clearing the hovered state of keys
        /// </summary>
        public void DeactivateSubKeyboard()
        {
            timer = 0;
            foreach (var subKeyboardKey in subKeyboardKeys)
            {
                subKeyboardKey.pointerHoveringKey = false;
            }
            gameObject.SetActive(false);
        }

    }
}