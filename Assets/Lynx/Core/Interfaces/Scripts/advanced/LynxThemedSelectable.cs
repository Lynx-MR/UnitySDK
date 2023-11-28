using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Lynx.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Selectable))]
    public class LynxThemedSelectable : LynxThemed
    {
        [Header("LynxThemedSelectable")]
        public LynxThemeColorSetSO.ThemeInfo normalColorType;
        public LynxThemeColorSetSO.ThemeInfo highlightedColorType;
        public LynxThemeColorSetSO.ThemeInfo pressedColorType;
        public LynxThemeColorSetSO.ThemeInfo selectedColorType;
        public LynxThemeColorSetSO.ThemeInfo disabledColorType;

        [System.Serializable]
        public class LynxSelectableColors
        {
            public Color ColorNormal;
            public Color ColorHighlighted;
            public Color ColorPressed;
            public Color ColorSelected;
            public Color ColorDisabled;
        }
        public LynxSelectableColors selectableComponentBaseColors;
        public List<LynxSelectableColors> lynxThemedSelectableColorsList;

        private Selectable selectable;

        private bool selectableBaseColorsSaved = false;
        private bool selectableBaseColorsReset = false;



        protected override void Awake()
        {
            base.Awake();
            selectable = GetComponent<Selectable>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            //LynxThemeManager.Instance.ThemeUpdateEvent -= UpdateTheme;
        }

        private void OnValidate()
        {
            //Debug.Log("LynxThemedSelectable.OnValidate()");
            selectable = GetComponent<Selectable>();
            UpdateLynxThemedColorsList();

            if (useLynxTheme)
            {
                if (selectableBaseColorsReset) selectableBaseColorsReset = false;
                if (!selectableBaseColorsSaved) SaveSelectableBaseColors();
                SetSelectableColorsToCurrentThemeColors();
            }
            else
            {
                if (selectableBaseColorsSaved) selectableBaseColorsSaved = false;
                if (!selectableBaseColorsReset) ResetSelectableBaseColors();
                //SetSelectableColorsToSavedBaseColors();
            }


        }



        public void SetSelectableColors(LynxSelectableColors lynxSelectableColors)
        {
            ColorBlock colorBlock = new ColorBlock();
            colorBlock.normalColor = lynxSelectableColors.ColorNormal;
            colorBlock.highlightedColor = lynxSelectableColors.ColorHighlighted;
            colorBlock.pressedColor = lynxSelectableColors.ColorPressed;
            colorBlock.selectedColor = lynxSelectableColors.ColorSelected;
            colorBlock.disabledColor = lynxSelectableColors.ColorDisabled;

            if (selectable == null) selectable = GetComponent<Selectable>();
            colorBlock.colorMultiplier = selectable.colors.colorMultiplier;
            colorBlock.fadeDuration = selectable.colors.fadeDuration;

            selectable.colors = colorBlock;
        }
        public void SetSelectableColorsToSavedBaseColors()
        {
            //Debug.Log("LynxThemedSelectable.SetSelectableColorsToSavedBaseColors()");
            SetSelectableColors(selectableComponentBaseColors);
        }
        public void SetSelectableColorsToCurrentThemeColors()
        {
            //Debug.Log("LynxThemedSelectable.SetSelectableColorsToCurrentThemeColors()");
            CheckLynxThemeManagerInstance();
            SetSelectableColors(lynxThemedSelectableColorsList[LynxThemeManager.Instance.currentIndex]);
        }

        public override void UpdateTheme()
        {
            if (useLynxTheme) SetSelectableColorsToCurrentThemeColors();
        }
        public override void UpdateLynxThemedColorsList()
        {
            lynxThemedSelectableColorsList = new List<LynxSelectableColors>();
            CheckLynxThemeManagerInstance();
            if (LynxThemeManager.Instance == null || LynxThemeManager.Instance.ThemeSets.Count == 0)
            {
                useLynxTheme = false;
                return;
            }


            for (int i = 0; i < LynxThemeManager.Instance.ThemeSets.Count; i++)
            {
                LynxThemeColorSetSO lynxThemeColorSet = LynxThemeManager.Instance.ThemeSets[i];
                LynxSelectableColors lynxThemedSelectableColors = new LynxSelectableColors();
                lynxThemedSelectableColors.ColorNormal = lynxThemeColorSet.GetColor(normalColorType);
                lynxThemedSelectableColors.ColorHighlighted = lynxThemeColorSet.GetColor(highlightedColorType);
                lynxThemedSelectableColors.ColorPressed = lynxThemeColorSet.GetColor(pressedColorType);
                lynxThemedSelectableColors.ColorSelected = lynxThemeColorSet.GetColor(selectedColorType);
                lynxThemedSelectableColors.ColorDisabled = lynxThemeColorSet.GetColor(disabledColorType);
                lynxThemedSelectableColorsList.Add(lynxThemedSelectableColors);
            }
        }

        public void SaveSelectableBaseColors()
        {
            selectableComponentBaseColors.ColorNormal = selectable.colors.normalColor;
            selectableComponentBaseColors.ColorHighlighted = selectable.colors.highlightedColor;
            selectableComponentBaseColors.ColorPressed = selectable.colors.pressedColor;
            selectableComponentBaseColors.ColorSelected = selectable.colors.selectedColor;
            selectableComponentBaseColors.ColorDisabled = selectable.colors.disabledColor;
            selectableBaseColorsSaved = true;
        }
        public void ResetSelectableBaseColors()
        {
            SetSelectableColorsToSavedBaseColors();
            selectableBaseColorsReset = true;
        }
    }
}