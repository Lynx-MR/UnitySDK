using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lynx.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Slider))]
    public class LynxThemedSlider : LynxThemed
    {
        [Header("LynxThemedSlider")]
        public LynxThemeColorSetSO.ThemeInfo normalColorType;
        public LynxThemeColorSetSO.ThemeInfo highlightedColorType;
        public LynxThemeColorSetSO.ThemeInfo pressedColorType;
        public LynxThemeColorSetSO.ThemeInfo selectedColorType;
        public LynxThemeColorSetSO.ThemeInfo disabledColorType;

        [System.Serializable]
        public class LynxSliderColors
        {
            public Color ColorNormal;
            public Color ColorHighlighted;
            public Color ColorPressed;
            public Color ColorSelected;
            public Color ColorDisabled;
        }
        public LynxSliderColors sliderComponentBaseColors;
        public List<LynxSliderColors> lynxThemedSliderColorsList;

        private Slider slider;

        private bool sliderBaseColorsSaved = false;
        private bool sliderBaseColorsReset = false;



        protected override void Awake()
        {
            base.Awake();
            slider = GetComponent<Slider>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            //LynxThemeManager.Instance.ThemeUpdateEvent -= UpdateTheme;
        }

        private void OnValidate()
        {
            //Debug.Log("LynxThemedSlider.OnValidate()");
            slider = GetComponent<Slider>();
            UpdateLynxThemedColorsList();

            if (useLynxTheme)
            {
                if (sliderBaseColorsReset) sliderBaseColorsReset = false;
                if (!sliderBaseColorsSaved) SaveSliderBaseColors();
                SetSliderColorsToCurrentThemeColors();
            }
            else
            {
                if (sliderBaseColorsSaved) sliderBaseColorsSaved = false;
                if (!sliderBaseColorsReset) ResetSliderBaseColors();
                //SetSliderColorsToSavedBaseColors();
            }


        }



        public void SetSliderColors(LynxSliderColors lynxSliderColors)
        {
            ColorBlock colorBlock = new ColorBlock();
            colorBlock.normalColor = lynxSliderColors.ColorNormal;
            colorBlock.highlightedColor = lynxSliderColors.ColorHighlighted;
            colorBlock.pressedColor = lynxSliderColors.ColorPressed;
            colorBlock.selectedColor = lynxSliderColors.ColorSelected;
            colorBlock.disabledColor = lynxSliderColors.ColorDisabled;

            if (slider == null) slider = GetComponent<Slider>();
            colorBlock.colorMultiplier = slider.colors.colorMultiplier;
            colorBlock.fadeDuration = slider.colors.fadeDuration;
            slider.colors = colorBlock;
        }
        public void SetSliderColorsToSavedBaseColors()
        {
            //Debug.Log("LynxThemedSlider.SetSliderColorsToSavedBaseColors()");
            SetSliderColors(sliderComponentBaseColors);
        }
        public void SetSliderColorsToCurrentThemeColors()
        {
            //Debug.Log("LynxThemedSlider.SetSliderColorsToCurrentThemeColors()");
            CheckLynxThemeManagerInstance();
            SetSliderColors(lynxThemedSliderColorsList[LynxThemeManager.Instance.currentIndex]);
        }

        public override void UpdateTheme()
        {
            if (useLynxTheme) SetSliderColorsToCurrentThemeColors();
        }
        public override void UpdateLynxThemedColorsList()
        {
            lynxThemedSliderColorsList = new List<LynxSliderColors>();
            CheckLynxThemeManagerInstance();
            if (LynxThemeManager.Instance == null || LynxThemeManager.Instance.ThemeSets.Count == 0)
            {
                useLynxTheme = false;
                return;
            }


            for (int i = 0; i < LynxThemeManager.Instance.ThemeSets.Count; i++)
            {
                LynxThemeColorSetSO lynxThemeColorSet = LynxThemeManager.Instance.ThemeSets[i];
                LynxSliderColors lynxThemedSliderColors = new LynxSliderColors();
                lynxThemedSliderColors.ColorNormal = lynxThemeColorSet.GetColor(normalColorType);
                lynxThemedSliderColors.ColorHighlighted = lynxThemeColorSet.GetColor(highlightedColorType);
                lynxThemedSliderColors.ColorPressed = lynxThemeColorSet.GetColor(pressedColorType);
                lynxThemedSliderColors.ColorSelected = lynxThemeColorSet.GetColor(selectedColorType);
                lynxThemedSliderColors.ColorDisabled = lynxThemeColorSet.GetColor(disabledColorType);
                lynxThemedSliderColorsList.Add(lynxThemedSliderColors);
            }
        }

        public void SaveSliderBaseColors()
        {
            sliderComponentBaseColors.ColorNormal = slider.colors.normalColor;
            sliderComponentBaseColors.ColorHighlighted = slider.colors.highlightedColor;
            sliderComponentBaseColors.ColorPressed = slider.colors.pressedColor;
            sliderComponentBaseColors.ColorSelected = slider.colors.selectedColor;
            sliderComponentBaseColors.ColorDisabled = slider.colors.disabledColor;
            sliderBaseColorsSaved = true;
        }
        public void ResetSliderBaseColors()
        {
            SetSliderColorsToSavedBaseColors();
            sliderBaseColorsReset = true;
        }
    }
}