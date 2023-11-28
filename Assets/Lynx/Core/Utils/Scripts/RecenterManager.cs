/**
 * @file RecenterManager.cs
 *
 * @author Geoffrey Marhuenda
 *
 * @brief Helper to recenter the user at the start of the scene.
 */
using UnityEngine;

namespace Lynx
{
    public class RecenterManager : MonoBehaviour
    {
        private Vector3 m_startPosition = Vector3.zero;

        void Start()
        {
            Transform t = Camera.main.transform;
            m_startPosition = t.position;

            // Create parent if needed
            if (Camera.main.transform.parent == null)
            {
                GameObject goParent = new GameObject();
                goParent.transform.position = Vector3.zero;
                goParent.transform.rotation = Quaternion.identity;
                Camera.main.transform.parent = goParent.transform;
            }
        }

        /// <summary>
        /// Recenter the user view at start position.
        /// Note: as the camera itself cannot be moved (overrided by tracking), it uses cameras parent
        /// </summary>
        public void ResetToStart()
        {
            Transform t = Camera.main.transform;
            Vector3 deltaPos = t.position - m_startPosition;

            Quaternion rot = Quaternion.AngleAxis(-t.eulerAngles.y, Vector3.up);
            Camera.main.transform.parent.position = rot * (Camera.main.transform.parent.position - t.position) + t.position - deltaPos;
            Camera.main.transform.parent.rotation = rot * Camera.main.transform.parent.rotation;
        }
    }
}