//#define DEBUG_PINCH

using UnityEngine;
#if DEBUG_PINCH
using TMPro;
using UnityEngine.UI;
#endif

namespace Lynx
{
    public class DebugPinch : MonoBehaviour
    {
#if DEBUG_PINCH
        public QuickMenuMng quickMenuMng;
        public DebugHandsPalmFacing debugHandsPalmFacing;
        [Space]
        public LynxSlider pinchThresholdEditSlider;
        public TextMeshProUGUI pinchThresholdEditSliderText;
        [Space]
        public float pinchDistGaugeMaxLength = 100;
        public RectTransform pinchDistGaugeFillRight;
        public RectTransform pinchDistGaugeFillLeft;
        public TextMeshProUGUI pinchDistGaugeFillRightText;
        public TextMeshProUGUI pinchDistGaugeFillLeftText;
        [Space]
        public LynxSlider palmThresholdEditSlider;
        public TextMeshProUGUI palmThresholdEditSliderText;
        [Space]
        public RawImage palmFacingRight;
        public RawImage palmFacingLeft;
        [Space]
        public RawImage handOpenRight;
        public RawImage handOpenLeft;



        private void Start()
        {
            pinchThresholdEditSlider.value = quickMenuMng.pinchActivateDist;
            palmThresholdEditSlider.value = quickMenuMng.palmFacingDotProdThreshold;
        }

        private void Update()
        {
            //lynxSlider.value = quickMenuMng.pinchRightDist;
            UpdateHandsOpen();
            UpdatePalmsFacing();
            UpdatePinchDistGauges();
        }


        //private methods
        private void UpdateHandsOpen()
        {
            handOpenRight.color = quickMenuMng.palmOpenRight ? Color.green : Color.red;
            handOpenLeft.color = quickMenuMng.palmOpenLeft ? Color.green : Color.red;
        }
        private void UpdatePalmsFacing()
        {
            palmFacingRight.color = quickMenuMng.palmFacingEyeRight ? Color.green : Color.red;
            palmFacingLeft.color = quickMenuMng.palmFacingEyeLeft ? Color.green : Color.red;
        }
        private void UpdatePinchDistGauges()
        {
            UpdatePinchDistGauge(quickMenuMng.pinchRightDist, quickMenuMng.pinchRight, pinchDistGaugeFillRight, pinchDistGaugeFillRightText);
            UpdatePinchDistGauge(quickMenuMng.pinchLeftDist, quickMenuMng.pinchLeft, pinchDistGaugeFillLeft, pinchDistGaugeFillLeftText);
        }
        private void UpdatePinchDistGauge(float pinchDist, bool pinchBool, RectTransform pinchDistGaugeFillRight, TextMeshProUGUI pinchDistGaugeFillRightText)
        {
            //length
            //float pinchDist = quickMenuMng.pinchRightDist;
            float distGaugeVal = Mathf.Lerp(0, pinchDistGaugeMaxLength, Mathf.InverseLerp(0, 100, pinchDist));
            pinchDistGaugeFillRight.sizeDelta = new Vector2(distGaugeVal, pinchDistGaugeFillRight.sizeDelta.y);
            pinchDistGaugeFillRightText.text = Mathf.Round(distGaugeVal).ToString();
            //color
            //bool pinchBool = quickMenuMng.pinchRight;
            pinchDistGaugeFillRight.GetComponent<RawImage>().color = pinchBool ? Color.green : Color.red;
        }

        //public methods
        public void UpdatePinchThresholdFromSlider()
        {
            UpdateSliderText();
            UpdatePinchActivateDist();
        }
        private void UpdateSliderText()
        {
            pinchThresholdEditSliderText.text = Mathf.Round(pinchThresholdEditSlider.value).ToString();
        }
        private void UpdatePinchActivateDist()
        {
            quickMenuMng.pinchActivateDist = Mathf.Round(pinchThresholdEditSlider.value);
            quickMenuMng.pinchDeactivateDist = quickMenuMng.pinchActivateDist + 5f;
        }

        public void UpdatePalmThresholdFromSlider()
        {
            float sliderValueFull = palmThresholdEditSlider.value;
            float sliderValueRounded01 = Mathf.Round(sliderValueFull * 100) / 100;
            palmThresholdEditSliderText.text = sliderValueRounded01.ToString();


            quickMenuMng.palmFacingDotProdThreshold = sliderValueRounded01;
            debugHandsPalmFacing.palmFacingDotThreshold = sliderValueRounded01;
        }
#endif
    }
}