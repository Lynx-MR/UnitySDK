using UnityEngine;

namespace Lynx.UI
{
    public class LynxSoundsMethods
    {
        public static void OnPressSound()
        {
            if (LynxSoundManager.Instance != null)
            {
                LynxSoundManager.Instance.currentSounds.CallAudioOnPress(out AudioClip clip);
                AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
            }
            else
            {
                //Debug.LogWarning("There is no Lynx Sound Manager in the scene.", button.gameObject);
            }
        }

        public static void OnUnpressSound()
        {
            if (LynxSoundManager.Instance != null)
            {
                LynxSoundManager.Instance.currentSounds.CallOnAudioUnpress(out AudioClip clip);
                AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
            }
            else
            {
                //Debug.LogWarning("There is no Lynx Sound Manager in the scene.", button.gameObject);
            }
        }

    }
}
