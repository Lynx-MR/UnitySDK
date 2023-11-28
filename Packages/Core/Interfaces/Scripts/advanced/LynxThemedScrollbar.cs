using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lynx.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Scrollbar))]
    public class LynxThemedScrollbar : LynxThemed
    {
        [Header("LynxThemedScrollbar")]
        public LynxThemeColorSetSO.ThemeInfo normalColorType;
        public LynxThemeColorSetSO.ThemeInfo highlightedColorType;
        public LynxThemeColorSetSO.ThemeInfo pressedColorType;
        public LynxThemeColorSetSO.ThemeInfo selectedColorType;
        public LynxThemeColorSetSO.ThemeInfo disabledColorType;

        [System.Serializable]
        public class LynxScrollbarColors
        {
            public Color ColorNormal;
            public Color ColorHighlighted;
            public Color ColorPressed;
            public Color ColorSelected;
            public Color ColorDisabled;
        }
        public LynxScrollbarColors scrollbarComponentBaseColors;
        public List<LynxScrollbarColors> lynxThemedScrollbarColorsList;

        private Scrollbar scrollbar;

        private bool scrollbarBaseColorsSaved = false;
        private bool scrollbarBaseColorsReset = false;



        protected override void Awake()
        {
            base.Awake();
            scrollbar = GetComponent<Scrollbar>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            //LynxThemeManager.Instance.ThemeUpdateEvent -= UpdateTheme;
        }

        private void OnValidate()
        {
            //Debug.Log("LynxThemedScrollbar.OnValidate()");
            scrollbar = GetComponent<Scrollbar>();
            UpdateLynxThemedColorsList();

            if (useLynxTheme)
            {
                if (scrollbarBaseColorsReset) scrollbarBaseColorsReset = false;
                if (!scrollbarBaseColorsSaved) SaveScrollbarBaseColors();
                SetScrollbarColorsToCurrentThemeColors();
            }
            else
            {
                if (scrollbarBaseColorsSaved) scrollbarBaseColorsSaved = false;
                if (!scrollbarBaseColorsReset) ResetScrollbarBaseColors();
                //SetScrollbarColorsToSavedBaseColors();
            }


        }



        public void SetScrollbarColors(LynxScrollbarColors lynxScrollbarColors)
        {
            ColorBlock colorBlock = new ColorBlock();
            colorBlock.normalColor = lynxScrollbarColors.ColorNormal;
            colorBlock.highlightedColor = lynxScrollbarColors.ColorHighlighted;
            colorBlock.pressedColor = lynxScrollbarColors.ColorPressed;
            colorBlock.selectedColor = lynxScrollbarColors.ColorSelected;
            colorBlock.disabledColor = lynxScrollbarColors.ColorDisabled;

            if (scrollbar == null) scrollbar = GetComponent<Scrollbar>();
            colorBlock.colorMultiplier = scrollbar.colors.colorMultiplier;
            colorBlock.fadeDuration = scrollbar.colors.fadeDuration;
            scrollbar.colors = colorBlock;
        }
        public void SetScrollbarColorsToSavedBaseColors()
        {
            //Debug.Log("LynxThemedScrollbar.SetScrollbarColorsToSavedBaseColors()");
            SetScrollbarColors(scrollbarComponentBaseColors);
        }
        public void SetScrollbarColorsToCurrentThemeColors()
        {
            //Debug.Log("LynxThemedScrollbar.SetScrollbarColorsToCurrentThemeColors()");
            CheckLynxThemeManagerInstance();
            SetScrollbarColors(lynxThemedScrollbarColorsList[LynxThemeManager.Instance.currentIndex]);
        }

        public override void UpdateTheme()
        {
            if (useLynxTheme) SetScrollbarColorsToCurrentThemeColors();
        }
        public override void UpdateLynxThemedColorsList()
        {
            lynxThemedScrollbarColorsList = new List<LynxScrollbarColors>();
            CheckLynxThemeManagerInstance();
            if (LynxThemeManager.Instance == null || LynxThemeManager.Instance.ThemeSets.Count == 0)
            {
                useLynxTheme = false;
                return;
            }


            for (int i = 0; i < LynxThemeManager.Instance.ThemeSets.Count; i++)
            {
                LynxThemeColorSetSO lynxThemeColorSet = LynxThemeManager.Instance.ThemeSets[i];
                LynxScrollbarColors lynxThemedScrollbarColors = new LynxScrollbarColors();
                lynxThemedScrollbarColors.ColorNormal = lynxThemeColorSet.GetColor(normalColorType);
                lynxThemedScrollbarColors.ColorHighlighted = lynxThemeColorSet.GetColor(highlightedColorType);
                lynxThemedScrollbarColors.ColorPressed = lynxThemeColorSet.GetColor(pressedColorType);
                lynxThemedScrollbarColors.ColorSelected = lynxThemeColorSet.GetColor(selectedColorType);
                lynxThemedScrollbarColors.ColorDisabled = lynxThemeColorSet.GetColor(disabledColorType);
                lynxThemedScrollbarColorsList.Add(lynxThemedScrollbarColors);
            }
        }

        public void SaveScrollbarBaseColors()
        {
            scrollbarComponentBaseColors.ColorNormal = scrollbar.colors.normalColor;
            scrollbarComponentBaseColors.ColorHighlighted = scrollbar.colors.highlightedColor;
            scrollbarComponentBaseColors.ColorPressed = scrollbar.colors.pressedColor;
            scrollbarComponentBaseColors.ColorSelected = scrollbar.colors.selectedColor;
            scrollbarComponentBaseColors.ColorDisabled = scrollbar.colors.disabledColor;
            scrollbarBaseColorsSaved = true;
        }
        public void ResetScrollbarBaseColors()
        {
            SetScrollbarColorsToSavedBaseColors();
            scrollbarBaseColorsReset = true;
        }


    }
}