using UnityEngine;

namespace Lynx
{
    public class TestARVRCallback : MonoBehaviour
    {
        public GameObject m_cubeAR = null;
        public GameObject m_cubeVR = null;


        void Awake()
        {
            LynxAPI.onARVRChanged += ARVRChanged;
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Space))
                LynxAPI.ToggleAR();
        }

        public void ARVRChanged(bool isAR)
        {
            Debug.Log($"ARVR changed. Is AR? {isAR}");
            m_cubeAR.SetActive(isAR);
            m_cubeVR.SetActive(!isAR);
        }
    }
}