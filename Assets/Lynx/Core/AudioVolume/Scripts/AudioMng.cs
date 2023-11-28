using UnityEngine;

namespace Lynx
{
    public class AudioMng : MonoBehaviour
    {
        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private AudioClip[] audioClips;

        public void Play_Click()
        {
            audioSource.PlayOneShot(audioClips[0]);
        }

        public void Play_Click_Down()
        {
            audioSource.PlayOneShot(audioClips[0]);
        }

        public void Play_Click_Up()
        {
            audioSource.PlayOneShot(audioClips[1]);
        }

        public void Play_Back()
        {
            audioSource.PlayOneShot(audioClips[2]);
        }

        public void Play_Photo_Diaph()
        {
            audioSource.PlayOneShot(audioClips[3]);
        }

        public void Play_Bip()
        {
            audioSource.PlayOneShot(audioClips[4]);
        }

        public void playVolumeTest()
        {
            audioSource.PlayOneShot(audioClips[5]);
        }

        public void stop()
        {
            audioSource.Stop();
        }
    }
}