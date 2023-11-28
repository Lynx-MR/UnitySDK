using System.Collections.Generic;
using UnityEngine;

namespace Lynx.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer))]
    public class LynxThemedMaterial : LynxThemed
    {
        [Header("LynxThemedMaterial")]
        public int materialIndex = 0;
        public LynxThemeColorSetSO.ThemeInfo colorType;

        public Color materialComponentBaseColor;
        public List<Color> lynxThemedMaterialColorList;

        private Material material;
        private MeshRenderer meshRenderer;

        private bool materialBaseColorSaved = false;
        private bool materialBaseColorReset = false;



        protected override void Awake()
        {
            base.Awake();
            meshRenderer = GetComponent<MeshRenderer>();
            material = meshRenderer.sharedMaterial;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            //LynxThemeManager.Instance.ThemeUpdateEvent -= UpdateTheme;
        }

        private void OnValidate()
        {
            //Debug.Log("LynxThemedImage.OnValidate()");
            meshRenderer = GetComponent<MeshRenderer>();
            material = meshRenderer.sharedMaterial;
            UpdateLynxThemedColorsList();

            if (useLynxTheme)
            {
                if (materialBaseColorReset) materialBaseColorReset = false;
                if (!materialBaseColorSaved) SaveMaterialBaseColor();
                SetMaterialColorToCurrentThemeColors();
            }
            else
            {
                if (materialBaseColorSaved) materialBaseColorSaved = false;
                if (!materialBaseColorReset) ResetMaterialBaseColor();
                //SetImageColorsToSavedBaseColors();
            }

        }



        public override void UpdateTheme()
        {
            if (useLynxTheme) SetMaterialColorToCurrentThemeColors();
        }
        public override void UpdateLynxThemedColorsList()
        {
            lynxThemedMaterialColorList = new List<Color>();
            CheckLynxThemeManagerInstance();
            if (LynxThemeManager.Instance == null || LynxThemeManager.Instance.ThemeSets.Count == 0)
            {
                useLynxTheme = false;
                return;
            }

            for (int i = 0; i < LynxThemeManager.Instance.ThemeSets.Count; i++)
            {
                LynxThemeColorSetSO lynxThemeColorSet = LynxThemeManager.Instance.ThemeSets[i];
                Color imageColor = lynxThemeColorSet.GetColor(colorType);

                lynxThemedMaterialColorList.Add(imageColor);
            }
        }


        public void SetMaterialColor(Color imageColor)
        {
            if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
            Material newMaterial = meshRenderer.sharedMaterials[materialIndex];
            newMaterial.color = imageColor;
            meshRenderer.material = newMaterial;
        }
        public void SetMaterialColorsToSavedBaseColors()
        {
            //Debug.Log("LynxThemedImage.SetImageColorsToSavedBaseColors()");
            SetMaterialColor(materialComponentBaseColor);
        }
        public void SetMaterialColorToCurrentThemeColors()
        {
            //Debug.Log("LynxThemedImage.SetImageColorsToCurrentThemeColors()");
            CheckLynxThemeManagerInstance();
            SetMaterialColor(lynxThemedMaterialColorList[LynxThemeManager.Instance.currentIndex]);
        }

        public void SaveMaterialBaseColor()
        {
            materialComponentBaseColor = material.color;

            materialBaseColorSaved = true;
        }
        public void ResetMaterialBaseColor()
        {
            SetMaterialColorsToSavedBaseColors();
            materialBaseColorReset = true;
        }
    }
}