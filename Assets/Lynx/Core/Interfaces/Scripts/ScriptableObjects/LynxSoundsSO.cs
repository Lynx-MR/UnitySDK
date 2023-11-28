using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lynx.UI
{
    [CreateAssetMenu(fileName = "LynxSounds", menuName = "ScriptableObjects/LynxSounds", order = 1)]
    [Serializable]
    public class LynxSoundsSO : ScriptableObject
    {
        public string themeName;
        public enum ThemeInfo
        {
            OnPress,
            OnUnpress
        }

        public enum AudioTick
        {
            None,
            OnPress,
            OnUnpress,
            Both
        }

        [SerializeField] private AudioTick tick;
        [SerializeField] private AudioClip onPress;
        [SerializeField] private AudioClip onUnpress;

        private void OnValidate()
        {
            LynxThemeManager.CheckLynxThemeManagerInstance();
            if (LynxThemeManager.Instance) LynxThemeManager.Instance.UpdateLynxThemedComponents();
        }

        public AudioClip CallOnPress()
        {
            return onPress;
        }


        public bool CallAudioOnPress(out AudioClip clip)
        {
            clip = onPress;
            return (tick == AudioTick.OnPress || tick == AudioTick.Both);

        }
        public bool CallOnAudioUnpress(out AudioClip clip)
        {
            clip = onUnpress;
            return (tick == AudioTick.OnUnpress || tick == AudioTick.Both);
        }
    }
}
