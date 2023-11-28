using UnityEngine;

namespace Lynx
{
    public class KeyboardAudioMng : MonoBehaviour
    {
        [SerializeField]
        private AudioSource mAudioSource;

        [SerializeField]
        private AudioClip[] mAudioClips;


        private static KeyboardAudioMng keyboardAudioMngInst = null; //Reference to the OSComMngInst, to make sure it's been included

        public static KeyboardAudioMng Instance()
        {
            if (!keyboardAudioMngInst)
            {
                keyboardAudioMngInst = FindObjectOfType(typeof(KeyboardAudioMng)) as KeyboardAudioMng;
                if (!keyboardAudioMngInst)
                {
                    Debug.LogError("There needs to be one active KeyboardAudioMng script on a GameObject in your scene.");
                }
            }
            return keyboardAudioMngInst;
        }

        public void playClickButton()
        {
            mAudioSource.PlayOneShot(mAudioClips[0]);
        }

        public void playBackButton()
        {
            mAudioSource.PlayOneShot(mAudioClips[1]);
        }
    }
}