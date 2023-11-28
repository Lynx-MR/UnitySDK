//   ==============================================================================
//   | Lynx Interfaces (2023)                                                     |
//   |======================================                                      |
//   | Button Animation Class                                                     |
//   | This class contains some parameters for button animation methods.          |
//   ==============================================================================

using System;
using UnityEngine;

namespace Lynx.UI
{
    [Serializable]
    public class ButtonAnimation
    {
        [Tooltip("Object that will be moved when the button is pressed")]
        public Transform moveRoot;

        [HideInInspector]
        public Vector3 moveBasePos;

        [Tooltip("Distance at which the button will be pushed in")]
        public Vector3 moveDelta;

        [Tooltip("Animation time when button is pressed")]
        public float moveDuration;

        [Tooltip("Compensates moveDelta for local root size")]
        public bool isUsingScale; 

        public ButtonAnimation()
        {
            this.moveRoot = null;
            this.moveBasePos = Vector3.zero;
            this.moveDelta = Vector3.forward;
            this.moveDuration = 0.5f;
            this.isUsingScale = false;
        }

        public ButtonAnimation(Transform moveRoot, Vector3 moveDelta, float moveDuration, bool isUsingScale)
        {
            this.moveRoot = moveRoot;
            this.moveBasePos = moveRoot.position;
            this.moveDelta = moveDelta;
            this.moveDuration = moveDuration;
            this.isUsingScale = isUsingScale;
        }
    }
}
