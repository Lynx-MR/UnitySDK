using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lynx.UI
{
    [ExecuteInEditMode]
    public class LynxThemeManager : MonoBehaviour
    {
        //INSPECTOR
        [SerializeField] private List<LynxThemeColorSetSO> m_ThemeColorSets = null;
        [SerializeField] private List<LynxSoundsSO> m_Sounds = null;

        public List<LynxThemeColorSetSO> ThemeSets
        {
            get { return m_ThemeColorSets; }
            set
            {
                return;
            }
        }
        public List<LynxSoundsSO> SoundsSets
        {
            get { return m_Sounds; }
            set
            {
                return;
            }
        }

        [SerializeField] private int m_CurrentIndex;
        public int currentIndex
        {
            get { return m_CurrentIndex; }
            set
            {
                if (value >= ThemeSets.Count || value < 0) return;
                m_CurrentIndex = value;

                currentTheme = ThemeSets[value];
                m_CurrentIndex = value;
                ThemeUpdateEvent?.Invoke();
                UpdateAllLynxUIElements();
                UpdateLynxThemedComponents();
            }
        }

        [HideInInspector] public LynxThemeColorSetSO currentTheme 
        {
            get { return ThemeSets[m_CurrentIndex]; } 
            set { return; }
        }
        [HideInInspector] public LynxSoundsSO currentSounds
        {
            get { return SoundsSets[m_CurrentIndex]; }
            set { return; }
        }

        public delegate void LynxThemeUpdated();
        public LynxThemeUpdated ThemeUpdateEvent;

        //Singleton
        public static LynxThemeManager Instance { get; private set; }

        /// <summary>
        /// Add Listener to ThemeManager update event, check if singelton is init first
        /// </summary>
        /// <param name="listener">event to bind to theme manager update</param>
        /// <returns></returns>
        public static IEnumerator SetupCoroutine(LynxThemeUpdated listener)
        {
            float elapsedTime = 0.0f;
            const float MAX_DURATION = 1.0f;
            while(Instance == null && elapsedTime < MAX_DURATION)
            {
                yield return new WaitForEndOfFrame();
                elapsedTime += Time.deltaTime;
            }

            if (Instance != null)
                Instance.ThemeUpdateEvent += listener;
        }


        private void Awake()
        {
            SetupSingleton();
        }

        private void OnValidate()
        {
#if UNITY_6000_0_OR_NEWER
            if (Instance == null || FindAnyObjectByType<LynxThemeManager>() == null) SetupSingleton();
#else
            if (Instance == null || FindObjectOfType<LynxThemeManager>() == null) SetupSingleton();
#endif
            if(ThemeSets == null)
                m_CurrentIndex = 0;
            else if (ThemeSets.Count == 0)
            {
                m_CurrentIndex = 0;
            }
            else if (m_CurrentIndex >= ThemeSets.Count)
            {
                m_CurrentIndex = ThemeSets.Count-1;
            }
            else if(m_CurrentIndex < 0)
            {
                m_CurrentIndex = 0;
            }


            currentIndex = m_CurrentIndex;
        }


        public void RefreshThemes()
        {
            if (ThemeUpdateEvent == null)
                Debug.LogError("Theme is null");
            else
                currentIndex = currentIndex;
        }

        public void SetupSingleton()
        {
#if UNITY_6000_0_OR_NEWER
            if (Instance != null && Instance != this && FindAnyObjectByType<LynxThemeManager>() != null && FindObjectsByType<LynxThemeManager>(FindObjectsSortMode.None).Length > 1)
#else
            if (Instance != null && Instance != this && FindObjectOfType<LynxThemeManager>() != null && FindObjectsOfType<LynxThemeManager>().Length > 1)
#endif

            {
                Debug.LogWarning("Theme Manager already exist in current scene.");
                DestroyImmediate(this);
            }
            else
                Instance = this;
        }

        public static void CheckLynxThemeManagerInstance()
        {
            if (Instance != null) return;

#if UNITY_6000_0_OR_NEWER
            FindAnyObjectByType<LynxThemeManager>()?.SetupSingleton();
#else
            FindObjectOfType<LynxThemeManager>()?.SetupSingleton();
#endif
        }

        public void UpdateAllLynxUIElements()
        {
            foreach (LynxImage elt in FindObjectsByType<LynxImage>(FindObjectsSortMode.None))
                if (elt.IsUsingTheme()) elt.SetThemeColors();

            foreach (LynxSimpleButton elt in FindObjectsByType<LynxSimpleButton>(FindObjectsSortMode.None))
                if (elt.IsUsingTheme()) elt.SetThemeColors();

            foreach (LynxSlider elt in FindObjectsByType<LynxSlider>(FindObjectsSortMode.None))
                if (elt.IsUsingTheme()) elt.SetThemeColors();

            foreach (LynxSwitchButton elt in FindObjectsByType<LynxSwitchButton>(FindObjectsSortMode.None))
                if (elt.IsUsingTheme()) elt.SetThemeColors();

            foreach (LynxTimerButton elt in FindObjectsByType<LynxTimerButton>(FindObjectsSortMode.None))
                if (elt.IsUsingTheme()) elt.SetThemeColors();

            foreach (LynxToggleButton elt in FindObjectsByType<LynxToggleButton>(FindObjectsSortMode.None))
                if (elt.IsUsingTheme()) elt.SetThemeColors();
        }

        public void UpdateLynxThemedComponents()
        {
#if UNITY_6000_0_OR_NEWER
            LynxThemed[] list = FindObjectsByType<LynxThemed>(FindObjectsSortMode.None) as LynxThemed[];
#else
            LynxThemed[] list = FindObjectsOfType(typeof(LynxThemed)) as LynxThemed[];
#endif

            foreach (LynxThemed lynxThemeComponent in list)
            {
                lynxThemeComponent.UpdateLynxThemedColorsList();
                lynxThemeComponent.UpdateTheme();
            }
        }

#if UNITY_EDITOR
        public static void ResetInstance() { Instance = null; }
#endif
    }
}