using UnityEngine;

namespace Lynx.UI
{
    public class LynxSoundsMethods
    {
        /// <summary>
        /// Will play OnPress sound at main camera position
        /// </summary>
        public static void OnPressSound()
        {
            if (LynxSoundManager.Instance != null)
            {
                LynxSoundManager.Instance.currentSounds.CallAudioOnPress(out AudioClip clip);
                AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
            }
            else
            {
                Debug.LogWarning("There is no Lynx Sound Manager in the scene.");
            }
        }
        /// <summary>
        /// Will play OnPress sound at specified position
        /// </summary>
        /// <param name="soundOrigin"></param>
        public static void OnPressSound(Vector3 soundOrigin)
        {
            if (LynxSoundManager.Instance != null)
            {
                LynxSoundManager.Instance.currentSounds.CallAudioOnPress(out AudioClip clip);
                AudioSource.PlayClipAtPoint(clip, soundOrigin);
            }
            else
            {
                Debug.LogWarning("There is no Lynx Sound Manager in the scene.");
            }
        }

        /// <summary>
        /// Will play OnUnpress sound at main camera position
        /// </summary>
        public static void OnUnpressSound()
        {
            if (LynxSoundManager.Instance != null)
            {
                LynxSoundManager.Instance.currentSounds.CallOnAudioUnpress(out AudioClip clip);
                AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
            }
            else
            {
                Debug.LogWarning("There is no Lynx Sound Manager in the scene.");
            }
        }
        /// <summary>
        /// Will play OnUnpress sound at specified position
        /// </summary>
        public static void OnUnpressSound(Vector3 soundOrigin)
        {
            if (LynxSoundManager.Instance != null)
            {
                LynxSoundManager.Instance.currentSounds.CallOnAudioUnpress(out AudioClip clip);
                AudioSource.PlayClipAtPoint(clip, soundOrigin);
            }
            else
            {
                Debug.LogWarning("There is no Lynx Sound Manager in the scene.");
            }
        }

    }
}
