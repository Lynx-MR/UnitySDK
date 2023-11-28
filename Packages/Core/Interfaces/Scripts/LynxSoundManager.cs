using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Lynx.UI
{
    [ExecuteInEditMode]
    public class LynxSoundManager : MonoBehaviour
    {
        //INSPECTOR
        [SerializeField] private List<LynxSoundsSO> m_Sounds = null;
        [SerializeField] private int m_CurrentIndex = 0;

        [HideInInspector]
        public LynxSoundsSO currentSounds
        {
            get { return m_Sounds[m_CurrentIndex]; }
            set { return; }
        }

        #region SINGLETON
        public static LynxSoundManager Instance { get; private set; } = null;

        private void Awake()
        {
            Instance = this;
        }

        protected LynxSoundManager() { }
        #endregion
    }
}