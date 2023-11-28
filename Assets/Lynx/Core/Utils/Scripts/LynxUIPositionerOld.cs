using UnityEngine;


namespace Lynx
{
    /// <summary>
    /// Positions UI in front of XRCamera with different mvt & threshold options
    /// </summary>
    public class LynxUIPositionerOld : MonoBehaviour
    {
        //INSPECTOR
        [SerializeField] private GameObject CameraSrc = null;
        [Header("Movement parameters")]
        [SerializeField] private float distance = 0.4f;
        [SerializeField] private float heightRelToHead = 0f;
        [Space]
        [SerializeField] private float thresholdHeight = 0.15f;
        [SerializeField] private float thresholdDistance = 0.15f;
        [SerializeField] private float thresholdRotation = 50f;
        [Space]
        [SerializeField] private float lerpTime = 0.5f;

        [Space]
        [SerializeField] private Vector3 offsetAtRecenter = new Vector3(0.0f, 0.0f, 0.0f);

        //PUBLIC
        public enum Positioning { Locked, FollowEyeForward, ThresholdLerps };
        public Positioning positioning;
        public Positioning positioningPrev;


        //PRIVATE
        private Vector3 velocity = Vector3.zero;
        private Vector3 velocityHeight = Vector3.zero;
        private Vector3 velocityDistance = Vector3.zero;
        private Vector3 velocityRotationVector = Vector3.zero;

        private float lerpTimeHeight = 0.5f;
        private float lerpTimeDistance = 0.5f;
        private float lerpTimeRotation = 0.5f;

        private bool lerpingHeight = false;
        private bool lerpingDistance = false;
        private bool lerpingRotation = false;


        private void Awake()
        {
            if (CameraSrc == null) CameraSrc = Camera.main.gameObject;
        }

        private void Update()
        {
            if (positioning == Positioning.ThresholdLerps) UpdatePositionThresholdLerps();
            else if (positioning == Positioning.FollowEyeForward) UpdatePositionFollowCameraForward();
        }


        //Private methods
        private void UpdatePositionThresholdLerps()
        {
            //get current deltas for UI height relative to camera
            float targetHeight = CameraSrc.transform.position.y + heightRelToHead;
            float targetHeightDelta = Mathf.Abs(transform.position.y - targetHeight);
            //get current deltas for UI distance relative to camera
            float targetDistance = distance;
            float currentDistance = Vector3.Distance(new Vector3(CameraSrc.transform.position.x, 0, CameraSrc.transform.position.z), new Vector3(transform.position.x, 0, transform.position.z));
            float targetDistanceDelta = Mathf.Abs(targetDistance - currentDistance);
            //get current deltas for UI angle relative to camera
            Vector3 cameraOrientationXZ = Vector3.Normalize(new Vector3(CameraSrc.transform.forward.x, 0, CameraSrc.transform.forward.z));
            Vector3 cameraToUI = transform.position - CameraSrc.transform.position;
            Vector3 UIOrientationXZ = Vector3.Normalize(new Vector3(cameraToUI.x, 0, cameraToUI.z));
            float angleDelta = Vector3.SignedAngle(cameraOrientationXZ, UIOrientationXZ, Vector3.up);

            //if height, distance or angle threshold exceeded, enable specific repositioning 
            if (lerpingHeight == false && targetHeightDelta > thresholdHeight) lerpingHeight = true;
            if (lerpingDistance == false && targetDistanceDelta > thresholdDistance) lerpingDistance = true;
            if (lerpingRotation == false && Mathf.Abs(angleDelta) > thresholdRotation) lerpingRotation = true;

            if (lerpingHeight || lerpingDistance || lerpingRotation)
            {
                //define target height, distance & angle vectors to curretn pos by default
                Vector3 targetHeightVector = new Vector3(0, transform.position.y, 0);
                float targetDistanceVal = currentDistance;
                Vector3 targetAngleVector = cameraOrientationXZ;
                //if repositioning enabled, set target vectors
                if (lerpingHeight) targetHeightVector = new Vector3(0, targetHeight, 0);
                if (lerpingDistance) targetDistanceVal = targetDistance;
                if (lerpingRotation) targetAngleVector = Quaternion.AngleAxis(0, Vector3.up) * cameraOrientationXZ;
                //Construct target position & smoothDamp UI towards it, then rotate UI towards camera
                Vector3 CameraPosXY = new Vector3(CameraSrc.transform.position.x, 0, CameraSrc.transform.position.z);
                Vector3 targetPos = targetHeightVector + CameraPosXY + targetAngleVector * targetDistanceVal;
                transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, lerpTime);
                RotateTowardsCameraHorizontal();
            }

            //update deltas for UI height, distance & angle relative to camera
            targetHeightDelta = Mathf.Abs(transform.position.y - targetHeight);
            currentDistance = Vector3.Distance(new Vector3(CameraSrc.transform.position.x, 0, CameraSrc.transform.position.z), new Vector3(transform.position.x, 0, transform.position.z));
            targetDistanceDelta = Mathf.Abs(targetDistance - currentDistance);
            cameraToUI = transform.position - CameraSrc.transform.position;
            UIOrientationXZ = Vector3.Normalize(new Vector3(cameraToUI.x, 0, cameraToUI.z));
            angleDelta = Vector3.SignedAngle(cameraOrientationXZ, UIOrientationXZ, Vector3.up);
            //if height, distance or angle deltas small enough, toggle of lerping flags 
            if (targetHeightDelta < 0.01) lerpingHeight = false;
            if (targetDistanceDelta < 0.01) lerpingDistance = false;
            if (Mathf.Abs(angleDelta) < 8) lerpingRotation = false;
        }
        private void UpdatePositionThresholdLerpsSeparate()
        {
            //update height
            float cameraHeightDelta = Mathf.Abs(transform.position.y - CameraSrc.transform.position.y);
            //Debug.LogError($"cameraHeightDelta = {cameraHeightDelta}");
            if (lerpingHeight == false && cameraHeightDelta > thresholdHeight)
            {
                lerpingHeight = true;
            }
            if (lerpingHeight)
            {
                Vector3 targetHeightPos = new Vector3(transform.position.x, CameraSrc.transform.position.y, transform.position.z);
                //transform.position = Vector3.Lerp(transform.position, targetHeightPos, lerpSpeedHeight * Time.deltaTime);
                transform.position = Vector3.SmoothDamp(transform.position, targetHeightPos, ref velocityHeight, lerpTimeHeight);

                if (cameraHeightDelta < 0.01) lerpingHeight = false;
            }


            //update distance
            float targetDistance = distance;
            //float currentDistance = Mathf.Sqrt(  Mathf.Pow(CameraSrc.transform.position.x - transform.position.x, 2)
            //                                   + Mathf.Pow(CameraSrc.transform.position.y - transform.position.y, 2));
            float currentDistance = Vector3.Distance(new Vector3(CameraSrc.transform.position.x, 0, CameraSrc.transform.position.z),
                                               new Vector3(transform.position.x, 0, transform.position.z));
            float targetDistanceDelta = Mathf.Abs(targetDistance - currentDistance);
            //Debug.LogError($"currentDistance = {currentDistance}");
            if (lerpingDistance == false && targetDistanceDelta > thresholdDistance)
            {
                lerpingDistance = true;
            }
            if (lerpingDistance)
            {
                Vector3 UIPosXY = new Vector3(transform.position.x, 0, transform.position.z);
                Vector3 CameraPosXY = new Vector3(CameraSrc.transform.position.x, 0, CameraSrc.transform.position.z);
                Vector3 fromCameraToUIXZNormalized = Vector3.Normalize(UIPosXY - CameraPosXY);

                Vector3 targetDistancePos = CameraSrc.transform.position + (fromCameraToUIXZNormalized * targetDistance);
                //transform.position = Vector3.Lerp(transform.position, targetDistancePos, lerpSpeedHeight);
                transform.position = Vector3.SmoothDamp(transform.position, targetDistancePos, ref velocityDistance, lerpTimeDistance);

                if (targetDistanceDelta < 0.01) lerpingDistance = false;
            }


            //update rotation
            Vector3 cameraOrientation = CameraSrc.transform.forward;
            Vector3 cameraToUI = transform.position - CameraSrc.transform.position;
            Vector3 UIOrientation = Vector3.Normalize(new Vector3(cameraToUI.x, 0, cameraToUI.z));
            float angleDelta = Vector3.SignedAngle(cameraOrientation, UIOrientation, Vector3.up);

            Debug.LogError($"angleDelta = {angleDelta}");

            if (lerpingRotation == false && Mathf.Abs(angleDelta) > thresholdRotation)
            {
                lerpingRotation = true;
            }
            if (lerpingRotation)
            {
                //smoothDamp the rot float & reconstruct pos from pivot
                //float smoothDampRot = Mathf.SmoothDampAngle(angleDelta, 0, ref velocityRotation, lerpTimeRotation);
                float smoothDampRot = 0;

                //float distance = currentDistance;
                Vector3 cameraPos = CameraSrc.transform.position;
                Vector3 cameraForwardXZ = Vector3.Normalize(new Vector3(cameraOrientation.x, 0, cameraOrientation.z));
                Vector3 rotatedVector = Quaternion.AngleAxis(smoothDampRot, Vector3.up) * cameraForwardXZ;
                Vector3 targetRotationPos = cameraPos + rotatedVector * distance;

                //transform.position = cameraPos + rotatedVector * distance;
                transform.position = Vector3.SmoothDamp(transform.position, targetRotationPos, ref velocityRotationVector, lerpTimeRotation);

                cameraOrientation = CameraSrc.transform.forward;
                cameraToUI = transform.position - CameraSrc.transform.position;
                UIOrientation = Vector3.Normalize(new Vector3(cameraToUI.x, 0, cameraToUI.z));
                angleDelta = Vector3.SignedAngle(cameraOrientation, UIOrientation, Vector3.up);
                if (Mathf.Abs(angleDelta) < 8) lerpingRotation = false;
            }

            RotateTowardsCameraHorizontal();
        }
        private void UpdatePositionFollowCameraForward()
        {
            Vector3 targetPosition = CameraSrc.transform.TransformPoint(new Vector3(0, heightRelToHead, distance));
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, lerpTime);
            Vector3 lookAtPos = new Vector3(CameraSrc.transform.position.x, transform.position.y, CameraSrc.transform.position.z);
            transform.LookAt(lookAtPos);
            transform.Rotate(new Vector3(0, 1, 0), 180);
        }

        private void RotateTowardsCameraHorizontal()
        {
            Vector3 lookAtPos = new Vector3(CameraSrc.transform.position.x, transform.position.y, CameraSrc.transform.position.z);
            transform.LookAt(lookAtPos);
            transform.Rotate(new Vector3(0, 1, 0), 180);
        }

        private void ChangeDistanceAndScale(float distance, float scale)
        {
            transform.position = CameraSrc.transform.position;
            transform.rotation = Quaternion.Euler(0, CameraSrc.transform.eulerAngles.y, 0);
            transform.Translate(Vector3.forward * distance, Space.Self);
            transform.localScale = Vector3.one * scale;
        }


        //Public methods
        public void RecenterPanel()
        {
            transform.position = CameraSrc.transform.position;
            transform.rotation = Quaternion.Euler(0, CameraSrc.transform.eulerAngles.y, 0);
            float targetDistance = distance;
            transform.Translate(Vector3.forward * (targetDistance), Space.Self);

            transform.Translate(offsetAtRecenter, Space.Self);

            Debug.Log("RecenterPanel() called");

        }

        public void LockPanel()
        {
            if (positioning == Positioning.Locked) positioningPrev = Positioning.ThresholdLerps;
            else positioningPrev = positioning;

            positioning = Positioning.Locked;
        }
        public void UnlockPanel()
        {
            if (positioningPrev == Positioning.Locked) positioning = Positioning.ThresholdLerps;
            else positioning = positioningPrev;
        }

    }
}