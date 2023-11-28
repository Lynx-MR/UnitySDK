using System.Collections;
using System.IO;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Hands;

namespace Lynx
{
    public class LynxGuardian : MonoBehaviour
    {
        private const string HEIGHT_SETUP_PATH = "/sdcard/height_config.txt";

        #region PROPERTIES

        public float Height { get; private set; } = 0.0f;
        public float FloorPositionY { get => floorYPosition; }

        #endregion

        #region INSPECTOR VARIABLES

        [Header("Guardian References")]
        [SerializeField] private XROrigin xrOrigin = null;
        [SerializeField] private GameObject floorHelperPrefab = null;
        [Header("Guardian Options")]
        [SerializeField] private bool autoRecenter = false;

        #endregion

        #region PRIVATE VARIABLES

        private GameObject m_floorHelperInstance = null;

        private float floorYPosition = 0f;
        private float minValue = float.MaxValue;

        private bool m_isRunning = false;

        private float offsetFloor = 0.02f;

        #endregion

        #region UNITY API

        private void OnValidate()
        {
            if (xrOrigin == null)
            {
                xrOrigin = FindObjectOfType<XROrigin>();
            }
        }

        // Awake is called when the script instance is being loaded.
        private void Awake()
        {
            // Ajust the camera y offset with the height value, if true.
            if (autoRecenter)
            {
                LoadCameraHeight();
                ApplyCameraHeight();
            }
        }

        #endregion

        #region FLOOR METHODS

        /// <summary>
        /// Call this function to start placing the floor.
        /// </summary>
        public void SetupFloorPosition()
        {
            if (!m_isRunning)
            {
                // Set isRunning as true, to avoid double setup.
                m_isRunning = true;

                // Reset floor Y position value and min value.
                floorYPosition = 0f;
                minValue = float.MaxValue;

                // Instantiate floor helper prefab.
                if (!m_floorHelperInstance)
                {
                    Vector3 relativePos = Camera.main.transform.position;
                    relativePos.y -= 0.5f;
                    m_floorHelperInstance = Instantiate(floorHelperPrefab, relativePos, Quaternion.identity);
                }

                // Start the positioning of the floor helper instance.
                StartCoroutine(UpdateFloorPositionCoroutine());
            }
        }

        /// <summary>
        /// Call this function to end placing the floor.
        /// </summary>
        public void ConfirmFloorPosition()
        {
            if (m_isRunning)
            {
                // Set isRunning as true, to avoid double confirm.
                m_isRunning = false;

                // Stop the positioning of the floor helper instance.
                StopCoroutine(UpdateFloorPositionCoroutine());

                // Update the floor Y position and set the Height property value.
                floorYPosition = m_floorHelperInstance.transform.position.y;
                Height = Camera.main.transform.position.y - floorYPosition;

                // Destroy the no longer useful floor helper instance.
                if (m_floorHelperInstance)
                    Destroy(m_floorHelperInstance);
            }
        }

        /// <summary>
        /// Call this coroutine to update the floor position.
        /// </summary>
        public IEnumerator UpdateFloorPositionCoroutine()
        {
            // Set relative position for the floor helper instance and the min value.

            Vector3 relativePos = Camera.main.transform.position;
            relativePos.y -= 0.5f;
            minValue = relativePos.y;
            m_floorHelperInstance.transform.position = relativePos;

            // Lower the floor helper instance to the lowest position of the fingers of both hands.
            while (m_isRunning)
            {
                if (LynxHandtrackingAPI.LeftHand.isTracked)
                {
                    for (XRHandJointID i = XRHandJointID.ThumbTip; i < XRHandJointID.EndMarker; i += 5)
                    {
                        Pose p;
                        LynxHandtrackingAPI.LeftHand.GetJoint(i).TryGetPose(out p);
                        minValue = Mathf.Min(minValue, p.position.y);
                    }
                }

                if (LynxHandtrackingAPI.RightHand.isTracked)
                {
                    for (XRHandJointID i = XRHandJointID.ThumbTip; i < XRHandJointID.EndMarker; i += 5)
                    {
                        Pose p;
                        LynxHandtrackingAPI.RightHand.GetJoint(i).TryGetPose(out p);
                        minValue = Mathf.Min(minValue, p.position.y);
                    }
                }

                relativePos.y = Camera.main.transform.parent.position.y + minValue - offsetFloor; // 1cm under the min position to be able to fit perfectly with the ground.
                m_floorHelperInstance.transform.position = relativePos;

                yield return new WaitForEndOfFrame();
            }
        }

        #endregion

        #region CAMERA METHODS

        /// <summary>
        /// Call this function to save the distance between the floor and the camera.
        /// </summary>
        public void SaveCameraHeight()
        {
            File.WriteAllText(HEIGHT_SETUP_PATH, $"{Height}");
            Debug.Log($"LynxGuardian - Camera Y Offset saved, with Height: {Height}");
        }

        /// <summary>
        /// Call this function to save the current camera position as camera height.
        /// </summary>
        public void SaveCurrentCameraHeight()
        {
            if (autoRecenter)
            {
                float cameraHeight = Camera.main.transform.position.y - xrOrigin.transform.position.y;
                File.WriteAllText(HEIGHT_SETUP_PATH, $"{cameraHeight}");
                Debug.Log($"LynxGuardian - Camera Y Offset saved, with Height: {cameraHeight}");
            }
        }

        /// <summary>
        /// Call this function to load the distance between the floor and the camera.
        /// </summary>
        public void LoadCameraHeight()
        {
            if (File.Exists(HEIGHT_SETUP_PATH))
            {
                string[] heightStr = File.ReadAllLines(HEIGHT_SETUP_PATH);
                if (heightStr.Length > 0) Height = float.Parse(heightStr[0]);
                else
                {
                    Height = xrOrigin.CameraYOffset;
                    Debug.LogWarning($"LynxGuardian - No content in {HEIGHT_SETUP_PATH}. Height set with Camera Y Offset value of the XR Origin.");
                }
            }
            else
            {
                Height = xrOrigin.CameraYOffset;
                Debug.LogWarning($"LynxGuardian - No config file for came height at {HEIGHT_SETUP_PATH}. Height set with Camera Y Offset value of the XR Origin.");
            }
        }

        /// <summary>
        /// Call this function to apply the Height as current Camera Y Offset in the XR Origin.
        /// </summary>
        public void ApplyCameraHeight()
        {

            float cameraToDeviceTrackingOriginYOffset = Camera.main.transform.position.y - xrOrigin.CameraYOffset;
            float heightWithCameraToDeviceTrackingOriginYOffset = Height - cameraToDeviceTrackingOriginYOffset;

            Vector3 newOffset = xrOrigin.CameraFloorOffsetObject.transform.position;
            newOffset.y = heightWithCameraToDeviceTrackingOriginYOffset;

            //xrOrigin.CameraYOffset = Height;
            xrOrigin.CameraYOffset = heightWithCameraToDeviceTrackingOriginYOffset;
            xrOrigin.CameraFloorOffsetObject.transform.position = newOffset;
        }

        #endregion

    }
}