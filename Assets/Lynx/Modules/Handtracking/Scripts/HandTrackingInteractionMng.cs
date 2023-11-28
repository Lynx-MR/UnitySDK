using UnityEngine;
using UnityEngine.Events;

namespace Lynx
{
    public class HandTrackingInteractionMng : MonoBehaviour
    {
        [SerializeField] public UnityEvent OnLux1Clicked = null;
        [SerializeField] public UnityEvent OnLux2Clicked = null;
        [SerializeField] public UnityEvent OnLux3Clicked = null;
        [SerializeField] public UnityEvent OnFoodClicked = null;
        [SerializeField] public UnityEvent OnWhistleClicked = null;
        
        [SerializeField] public Material m_luxBodyMaterial = null;
        [SerializeField] public Material m_luxEyesMaterial = null;

        /*/
        private void Start()
        {
            //OnLux1Clicked.Invoke();
            //OnLux2Clicked.Invoke();
            //OnLux3Clicked.Invoke();
        }
        //*/

        public void LuxTexture1ButtonClicked()
        {
            OnLux1Clicked.Invoke();
        }

        public void LuxTexture2ButtonClicked()
        {
            OnLux2Clicked.Invoke();
        }

        public void LuxTexture3ButtonClicked()
        {
            OnLux3Clicked.Invoke();
        }

        public void FoodClicked()
        {
            OnFoodClicked.Invoke();
        }

        public void WhistleClicked()
        {
            OnWhistleClicked.Invoke();
        }

        public void SetBodyTexture(Texture2D tex)
        {
            m_luxBodyMaterial.SetTexture("_MainTex", tex);
        }

        public void SetEyesTexture(Texture2D tex)
        {
            m_luxEyesMaterial.SetTexture("_MainTex", tex);
        }
    }
}