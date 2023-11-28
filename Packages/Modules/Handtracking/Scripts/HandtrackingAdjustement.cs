/**
 * @file HandtrackingAdjustement.cs
 *
 * @author Geoffrey Marhuenda
 *
 * @brief Adjust handtracking offset based on config file on the device.
 */
using Leap.Unity;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace Lynx
{
    public class HandtrackingAdjustement : MonoBehaviour
    {
        [SerializeField] private LeapXRServiceProvider m_provider = null;
        private string m_fileName = "/etc/qvr/handtracking_offsets.txt";

        void Start()
        {
            if (File.Exists(m_fileName))
            {
                string[] config = File.ReadAllLines(m_fileName);
                if (config.Length > 3)
                {
                    Debug.Log("Use hantracking offsets on device.");
                    //m_provider.deviceOffsetXAxis = float.Parse(config[0], CultureInfo.InvariantCulture);
                    m_provider.deviceOffsetYAxis = float.Parse(config[1], CultureInfo.InvariantCulture);
                    m_provider.deviceOffsetZAxis = float.Parse(config[2], CultureInfo.InvariantCulture);
                    m_provider.deviceTiltXAxis   = float.Parse(config[3], CultureInfo.InvariantCulture);
                }
                else
                {
                    Debug.Log("Use default hantracking offsets.");
                }
            }
            else
            {
                Debug.Log("Use default hantracking offsets.");
            }
        }

        public void SetProvider(LeapXRServiceProvider provider)
        {
            m_provider = provider;
        }

        /* Remove this line if you want to manually adjust handtracking with scrcpy and keyboard
        private const float delta = 0.001f;
        private const float deltaR = 0.25f;
        private void Update()
        {
            bool hasChanged = false;
            Vector3 newPos = new Vector3(m_provider.deviceOffsetXAxis, m_provider.deviceOffsetYAxis, m_provider.deviceOffsetZAxis);
            Vector3 newEulerRotation = new Vector3(m_provider.deviceTiltXAxis, .0f,.0f);

            ///// Position /////
            // Up/down
            if(Input.GetKey(KeyCode.Z))
            {
                newPos.y += delta;
                hasChanged = true;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                newPos.y -= delta;
                hasChanged = true;
            }

            // Left/right
            if (Input.GetKey(KeyCode.D))
            {
                newPos.x += delta;
                hasChanged = true;
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                newPos.x -= delta;
                hasChanged = true;
            }

            // Depth
            else if (Input.GetKey(KeyCode.E))
            {
                newPos.z += delta;
                hasChanged = true;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                newPos.z -= delta;
                hasChanged = true;
            }

            ///// Rotation /////
            // Pitch
            if (Input.GetKey(KeyCode.O)) 
            {
                newEulerRotation.x -= deltaR;
                hasChanged = true;
            }
            else if (Input.GetKey(KeyCode.L))
            {
                newEulerRotation.x += deltaR;
                hasChanged = true;
            }

            // Roll
            if (Input.GetKey(KeyCode.K))
            {
                newEulerRotation.z -= deltaR;
                hasChanged = true;
            }
            else if (Input.GetKey(KeyCode.M))
            {
                newEulerRotation.z += deltaR;
                hasChanged = true;
            }

            // Yaw
            if (Input.GetKey(KeyCode.P))
            {
                newEulerRotation.y -= deltaR;
                hasChanged = true;
            }
            else if (Input.GetKey(KeyCode.I))
            {
                newEulerRotation.y += deltaR;
                hasChanged = true;
            }

            // If there is a change, apply it and display it in the console (to help user reuse the current values)
            if (hasChanged)
            {
                m_provider.deviceOffsetXAxis = newPos.x;
                m_provider.deviceOffsetYAxis = newPos.y;
                m_provider.deviceOffsetZAxis = newPos.z;
                m_provider.deviceTiltXAxis = newEulerRotation.x;
                Debug.Log($"{newPos.x}\n{newPos.y}\n{newPos.z}\n{newEulerRotation.x}");
            }
        }
        //*/
    }
}