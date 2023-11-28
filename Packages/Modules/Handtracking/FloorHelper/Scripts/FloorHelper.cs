/**
* @file FloorHelper.cs
* 
* @author Geoffrey Marhuenda
* 
* @brief Helper used to define floor level. Floor level can then be retrieved by Level property.
*/
using Leap.Unity;
using System.Collections;
using UnityEngine;

namespace Lynx
{
    public class FloorHelper : MonoBehaviour
    {
        #region INSPECTOR VARIABLES
        [SerializeField] private Chirality m_chirality = Chirality.Right;
        [SerializeField] private GameObject m_floorHelperPrefab = null;
        [SerializeField] private GameObject m_ARFloorInstance = null;
        [SerializeField] private bool m_helperAtStart = false;
        #endregion

        #region PRIVATE VARIABLES
        private bool m_isRunning = false; // Define if floor level editing is enabled
        private GameObject m_floorHelperInstance = null;
        const float PALM_DIRECTION_THRESHOLD = 0.4f;
        #endregion

        #region PROPERTIES
        // Give current floor level
        public float Level { get; private set; } = 0.0f;
        #endregion

        #region UNITY API
        private void OnEnable()
        {
            if (m_helperAtStart)
                StartFloorLevel();
        }
        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Enable or disable floor level editor.
        /// </summary>
        public void ToggleFloorHelper()
        {
            if (m_isRunning)
                EndFloorLevel();
            else
                StartFloorLevel();
        }

        /// <summary>
        /// Create the floor helper and start updating the level based on palm position (when palm direction is pointing down).
        /// </summary>
        public void StartFloorLevel()
        {
            if (!m_isRunning)
            {
                m_isRunning = true;

                if (m_ARFloorInstance)
                    m_ARFloorInstance.SetActive(false);

                StartCoroutine(UpdateFloorLevelCoroutine());
            }
        }

        /// <summary>
        /// Remove floor helper and set AR floor location at floor helper position.
        /// </summary>
        public void EndFloorLevel()
        {
            Destroy(this.gameObject.GetComponent<PalmDirectionDetector>());

            m_isRunning = false;

            if (m_ARFloorInstance)
            {
                m_ARFloorInstance.SetActive(true);
                m_ARFloorInstance.transform.position = m_floorHelperInstance.transform.position;
            }

            if (m_floorHelperInstance)
                DestroyImmediate(m_floorHelperInstance);

        }

        /// <summary>
        /// Coroutine to update the level based on hand position.
        /// </summary>
        /// <returns></returns>
        public IEnumerator UpdateFloorLevelCoroutine()
        {

            if (m_floorHelperInstance)
            {
                DestroyImmediate(m_floorHelperInstance);
                DestroyImmediate(m_ARFloorInstance);
            }

            Leap.Hand hand = Hands.Provider.GetHand(m_chirality);
            while(hand == null)
            {
                yield return new WaitForEndOfFrame();
                hand = Hands.Provider.GetHand(m_chirality);
            }

            if (hand != null)
            {

                Vector3 handPosition = hand.PalmPosition;
                m_floorHelperInstance = Instantiate(m_floorHelperPrefab);
                m_floorHelperInstance.transform.position = handPosition;

                m_ARFloorInstance.transform.position = handPosition;

                while (m_isRunning)
                {
                    hand = Hands.Provider.GetHand(m_chirality);
                    if (hand != null && (hand.PalmNormal - Vector3.down).magnitude < PALM_DIRECTION_THRESHOLD)
                    {
                        handPosition = hand.PalmPosition;
                        if (handPosition.y - 0.05f < m_floorHelperInstance.transform.position.y)
                        {
                            Vector3 pos = handPosition;
                            pos.y -= 0.05f;
                            m_floorHelperInstance.transform.position = pos;
                            Level = pos.y;
                        }
                    }

                    yield return new WaitForEndOfFrame();
                }
            }
        }
        #endregion

        #region SINGLETON
        protected FloorHelper() { }
        public static FloorHelper Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
        }
        #endregion
    }
}