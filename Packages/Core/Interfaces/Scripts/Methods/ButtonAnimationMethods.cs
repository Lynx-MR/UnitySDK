//   ==============================================================================
//   | Lynx Interfaces (2023)                                                     |
//   |======================================                                      |
//   | Button Animation Methods                                                   |
//   | This script contains some generic button animation methods.                |
//   ==============================================================================

using System;
using System.Collections;
using UnityEngine;

namespace Lynx.UI
{
    public class ButtonAnimationMethods
    {
        /// <summary>
        /// Start this coroutine to activate the push-button animation.
        /// </summary>
        /// <param name="buttonAnimation">Button animation values.</param>
        /// <param name="transform">Transform of the button.</param>
        /// <param name="callback">The callback when the coroutine is ended.</param>
        /// <returns></returns>
        public static IEnumerator PressingAnimationCoroutine(ButtonAnimation buttonAnimation, Transform transform, Action<bool> callback)
        {
            float elapsedTime = 0.0f;

            Vector3 moveBasePos = !buttonAnimation.isUsingScale ? buttonAnimation.moveBasePos : Vector3.Scale(buttonAnimation.moveBasePos, LynxMath.InverseVector(transform.localScale));
            Vector3 moveDelta = !buttonAnimation.isUsingScale ? buttonAnimation.moveDelta : Vector3.Scale(buttonAnimation.moveDelta, LynxMath.InverseVector(transform.localScale));
            Vector3 pressedPose = moveBasePos + moveDelta;

            while (elapsedTime < buttonAnimation.moveDuration)
            {
                buttonAnimation.moveRoot.localPosition = Vector3.Lerp(moveBasePos, pressedPose, elapsedTime / buttonAnimation.moveDuration);
                yield return new WaitForEndOfFrame();
                elapsedTime += Time.deltaTime;
            }

            buttonAnimation.moveRoot.localPosition = pressedPose;
            callback(true);
        }

        /// <summary>
        /// Start this coroutine to activate the release-button animation.
        /// </summary>
        /// <param name="buttonAnimation">Button animation values.</param>
        /// <param name="transform">Transform of the button.</param>
        /// <param name="callback">The callback when the coroutine is ended.</param>
        /// <returns></returns>
        public static IEnumerator UnpressingAnimationCoroutine(ButtonAnimation buttonAnimation, Transform transform, Action<bool> callback)
        {
            float elapsedTime = 0.0f;

            Vector3 pressedPose = buttonAnimation.moveRoot.localPosition;
            Vector3 moveBasePos = !buttonAnimation.isUsingScale ? buttonAnimation.moveBasePos : Vector3.Scale(buttonAnimation.moveBasePos, LynxMath.InverseVector(transform.localScale));

            while (elapsedTime < buttonAnimation.moveDuration)
            {
                buttonAnimation.moveRoot.localPosition = Vector3.Lerp(pressedPose, moveBasePos, elapsedTime / buttonAnimation.moveDuration);
                yield return new WaitForEndOfFrame();
                elapsedTime += Time.deltaTime;
            }

            buttonAnimation.moveRoot.localPosition = moveBasePos;
            callback(false);
        }

        /// <summary>
        /// Call this functionto reset the animation.
        /// </summary>
        /// <param name="buttonAnimation">Button animation values.</param>
        /// <param name="transform">Transform of the button.</param>
        public static void ResetAnimation(ButtonAnimation buttonAnimation, Transform transform)
        {
           Vector3 initialPos = !buttonAnimation.isUsingScale ? buttonAnimation.moveBasePos : Vector3.Scale(buttonAnimation.moveBasePos, LynxMath.InverseVector(transform.localScale));
            buttonAnimation.moveRoot.localPosition = initialPos;
        }

    }

}
