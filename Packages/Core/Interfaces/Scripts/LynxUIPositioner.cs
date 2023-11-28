//   ==============================================================================
//   | Lynx Interfaces (2023)                                                     |
//   |======================================                                      |
//   | LynxUIPositioner Script                                                    |
//   | Script to manage the position of an UI.                                    |
//   ==============================================================================

using UnityEngine;

namespace Lynx.UI
{
    public class LynxUIPositioner : MonoBehaviour
    {
        #region PUBLIC VARIABLES

        // Generic Parameters
        public Transform uiTransform = null;

        public PositionMode positionMode;
        public enum PositionMode { locked, followCarthesian, followCylindrical };

        public Transform uiOriginTarget = null;
        public Transform uiLookAtTarget = null;

        // Carthesian Parameters
        public Vector3 originLocalOffset;
        public float originDistThreshold;
        public float originLerpTime;

        // Cylindrical Parameters
        public float targetDistance;
        public float targetDistanceThreshold;
        public float targetDistanceLerpTime;

        public float targetAngle;
        public float targetAngleThreshold;
        public float targetAngleLerpTime;

        public float targetHeight;
        public float targetHeightThreshold;
        public float targetHeightLerpTime;

        // Rotation Parameters
        public bool yAxisRotUpdateOnlyOnPosLerp = true;
        public YAxisMode yAxisMode;
        public enum YAxisMode { lockedWorld, lookAtTargetYAxis, followTargetYAngle };
        public XAxisMode xAxisMode;
        public enum XAxisMode { locked, lookAtTargetXAxis, followTargetXAngle };
        public ZAxisMode zAxisMode;
        public enum ZAxisMode { locked, followTargetZAngle };
        public Vector3 rotationLocalOffset;

        // Hide In Inspector
        public float currentLocalRotX;
        public float currentLocalRotY;
        public float currentLocalRotZ;

        #endregion

        #region PRIVATE VARIABLES

        private Vector3 velocityCarthesianMov = Vector3.zero;
        private Vector3 velocityCylindricalMov = Vector3.zero;

        private float velocitySmoothdampDistance = 0;
        private float velocitySmoothdampAngle = 0;
        private float velocitySmoothdampHeight = 0;

        private bool lerpingCarthesian = false;
        private bool lerpingCylDistance = false;
        private bool lerpingCylAngle = false;
        private bool lerpingCylHeight = false;

        #endregion

        #region UNITY API

        // Awake is called when an enabled script instance is being loaded.
        private void Awake()
        {
            if (uiTransform == null) uiTransform = this.transform;
            if (uiOriginTarget == null) uiOriginTarget = Camera.main.gameObject.transform;
            if (uiLookAtTarget == null) uiLookAtTarget = Camera.main.gameObject.transform;
        }

        // Start is called on the frame when a script is enabled just before any of the Update methods are called the first time.
        private void Start()
        {
            currentLocalRotX = uiTransform.localEulerAngles.x;
            currentLocalRotY = uiTransform.localEulerAngles.y;
            currentLocalRotZ = uiTransform.localEulerAngles.z;
        }

        // Update is called every frame, if the MonoBehaviour is enabled.
        private void Update()
        {
            if (positionMode == PositionMode.followCarthesian) UpdatePosFollowCarthesian();
            else if (positionMode == PositionMode.followCylindrical) UpdatePosFollowCylindrical();

            UpdateRot();
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// 
        /// </summary>
        public void ResetToTargetOffsets()
        {
            if (positionMode == PositionMode.followCarthesian)
            {
                Vector3 targetPosition = uiOriginTarget.TransformPoint(originLocalOffset);
                //uiTransform.position = Vector3.SmoothDamp(uiTransform.position, targetPosition, ref velocityCarthesianMov, originLerpTime);
                uiTransform.position = targetPosition;
            }
            else if (positionMode == PositionMode.followCylindrical || positionMode == PositionMode.locked)
            {
                float targetDistanceFinal = targetDistance;
                float targetAngleFinal = GetUiOriginTargetWorldYAngle() + targetAngle;
                float targetHeightFinal = targetHeight;
                Vector3 targetCylPos = WorldCylindricalForwardAngleZeroToCarthesian(targetDistanceFinal, targetAngleFinal, targetHeightFinal);
                Vector3 targetPos = uiOriginTarget.position + targetCylPos;
                uiTransform.position = targetPos;
            }

            bool temp = yAxisRotUpdateOnlyOnPosLerp;
            if (yAxisRotUpdateOnlyOnPosLerp) yAxisRotUpdateOnlyOnPosLerp = false;
            UpdateRot();
            yAxisRotUpdateOnlyOnPosLerp = temp;
        }

        /// <summary>
        /// Call thus function to convert to cartesian.
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="azimuthAngleDegrees"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public Vector3 ToCartesian(float radius, float azimuthAngleDegrees, float height)
        {
            float azimuthAngleRadians = (azimuthAngleDegrees + 90) * Mathf.Deg2Rad;
            float x = radius * Mathf.Cos(azimuthAngleRadians);
            float z = radius * Mathf.Sin(azimuthAngleRadians);
            return new Vector3(x, height, z);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Vector3 WorldCarthesianToCylindricalForwardAngleZero(Vector3 position)
        {
            float radius = Mathf.Sqrt(position.x * position.x + position.z * position.z);
            float azimuthAngleRadians = Mathf.Atan2(-position.x, position.z);
            float azimuthAngleDegrees = azimuthAngleRadians * Mathf.Rad2Deg;
            float height = position.y;

            return new Vector3(radius, azimuthAngleDegrees, height);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="azimuthAngleDegrees"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public Vector3 WorldCylindricalForwardAngleZeroToCarthesian(float radius, float azimuthAngleDegrees, float height)
        {
            float azimuthAngleRadians = azimuthAngleDegrees * Mathf.Deg2Rad;

            float x = radius * Mathf.Sin(azimuthAngleRadians);
            float z = radius * Mathf.Cos(azimuthAngleRadians);
            float y = height;

            return new Vector3(x, y, z);
        }

        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// 
        /// </summary>
        private void UpdatePosFollowCarthesian()
        {
            Vector3 targetPosition = uiOriginTarget.TransformPoint(originLocalOffset);

            float targetPosDelta = Mathf.Abs(GetUiOriginTargetDistDelta());
            if (lerpingCarthesian == false && targetPosDelta > originDistThreshold)
            {
                lerpingCarthesian = true;
                Invoke(nameof(StopLerpingCarthesian), originLerpTime + 1f);
            }
            if (lerpingCarthesian == true) uiTransform.position = Vector3.SmoothDamp(uiTransform.position, targetPosition, ref velocityCarthesianMov, originLerpTime);

        }

        private void StopLerpingCarthesian() => lerpingCarthesian = false;

        /// <summary>
        /// 
        /// </summary>
        private void UpdatePosFollowCylindrical()
        {
            float currentDistance = GetCylDistanceFromOrigin();
            float currentAngle = GetUiCylCoordAngleFromOriginTargetWorld();
            float currentHeight = uiTransform.position.y - uiOriginTarget.position.y;

            float targetDistanceFinal = targetDistance;
            float targetAngleFinal = GetUiOriginTargetWorldYAngle() + targetAngle;
            float targetHeightFinal = targetHeight;

            if (Mathf.Abs(currentAngle - targetAngleFinal) > 180)
            {
                if (targetAngleFinal < 0) targetAngleFinal += 360;
                else targetAngleFinal -= 360;
            }

            if (lerpingCylDistance == false && Mathf.Abs(GetCylDistanceTargetFromOriginDelta()) > targetDistanceThreshold)
            {
                lerpingCylDistance = true;
                Invoke(nameof(StopLerpingDistance), targetDistanceLerpTime + 1f);
            }
            if (lerpingCylAngle == false && Mathf.Abs(GetUiCylCoordAngleFromOriginTargetLocal()) > targetAngleThreshold)
            {
                lerpingCylAngle = true;
                Invoke(nameof(StopLerpingAngle), targetAngleLerpTime + 1f);
            }
            if (lerpingCylHeight == false && Mathf.Abs(GetCylHeightTargetFromOriginDelta()) > targetHeightThreshold)
            {
                lerpingCylHeight = true;
                Invoke(nameof(StopLerpingHeight), targetHeightLerpTime + 1f);
            }

            float targetDistanceSD = Mathf.SmoothDamp(currentDistance, targetDistanceFinal, ref velocitySmoothdampDistance, targetDistanceLerpTime);
            float targetAngleSD = Mathf.SmoothDamp(currentAngle, targetAngleFinal, ref velocitySmoothdampAngle, targetAngleLerpTime);
            float targetHeightSD = Mathf.SmoothDamp(currentHeight, targetHeightFinal, ref velocitySmoothdampHeight, targetHeightLerpTime);

            float targetDistanceNext = lerpingCylDistance ? targetDistanceSD : currentDistance;
            float targetAngleNext = lerpingCylAngle ? targetAngleSD : currentAngle;
            float targetHeightNext = lerpingCylHeight ? targetHeightSD : currentHeight;

            Vector3 targetCylPos = WorldCylindricalForwardAngleZeroToCarthesian(targetDistanceNext, targetAngleNext, targetHeightNext);
            Vector3 targetPos = uiOriginTarget.position + targetCylPos;

            //Vector3 targetPosSmooth = Vector3.SmoothDamp(uiTransform.position, targetPos, ref velocityCylindricalMov, 0.5f);
            uiTransform.position = targetPos;
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdatePosFollowCylindricalTest()
        {
            float currentDistance = GetCylDistanceFromOrigin();
            //float currentAngle = GetCylAngleFromOrigin();
            float currentAngle = GetUiCylCoordAngleFromOriginTargetWorld();
            float currentHeight = uiTransform.position.y - uiOriginTarget.position.y;

            float targetDistanceFinal = targetDistance;
            float targetAngleFinal = GetUiOriginTargetWorldYAngle() + targetAngle;
            float targetHeightFinal = targetHeight;

            //if (lerpingDistance == false && Mathf.Abs(GetCylDistanceTargetFromOriginDelta()) > targetDistanceThreshold) lerpingDistance = true;
            //if (lerpingAngle == false && Mathf.Abs(GetCylAngleFromOrigin()) > targetAngleThreshold) lerpingAngle = true;
            //if (lerpingHeight == false && Mathf.Abs(GetCylHeightTargetFromOriginDelta()) > targetHeightThreshold) lerpingHeight = true;


            if (lerpingCylDistance == false && Mathf.Abs(GetCylDistanceTargetFromOriginDelta()) > targetDistanceThreshold)
            {
                lerpingCylDistance = true;
                Invoke(nameof(StopLerpingDistance), targetDistanceLerpTime + 0.5f);
            }
            if (lerpingCylAngle == false && Mathf.Abs(GetUiCylCoordAngleFromOriginTargetLocal()) > targetAngleThreshold)
            {
                lerpingCylAngle = true;
                Invoke(nameof(StopLerpingAngle), targetAngleLerpTime + 1f);
            }
            if (lerpingCylHeight == false && Mathf.Abs(GetCylHeightTargetFromOriginDelta()) > targetHeightThreshold)
            {
                lerpingCylHeight = true;
                Invoke(nameof(StopLerpingHeight), targetHeightLerpTime + 0.5f);
            }

            float targetDistanceSD = Mathf.SmoothDamp(currentDistance, targetDistanceFinal, ref velocitySmoothdampDistance, targetDistanceLerpTime);
            float targetAngleSD = Mathf.SmoothDamp(currentAngle, targetAngleFinal, ref velocitySmoothdampAngle, targetAngleLerpTime);
            float targetHeightSD = Mathf.SmoothDamp(currentHeight, targetHeightFinal, ref velocitySmoothdampHeight, targetHeightLerpTime);

            //float targetDistanceSD = Mathf.Lerp(currentDistance, targetDistanceFinal, targetDistanceLerpTime * Time.deltaTime);
            //float targetAngleSD = Mathf.LerpAngle(currentAngle, targetAngleFinal, targetAngleLerpTime * Time.deltaTime);
            //float targetHeightSD = Mathf.Lerp(currentHeight, targetHeightFinal, targetHeightLerpTime * Time.deltaTime);

            //lerpingDistance = (Mathf.Abs(GetCylDistanceTargetFromOriginDelta()) > targetDistanceThreshold);
            //lerpingAngle = (Mathf.Abs(GetCylAngleFromOrigin()) > targetAngleThreshold);
            //lerpingHeight = (Mathf.Abs(GetCylHeightTargetFromOriginDelta()) > targetHeightThreshold);

            //float targetDistanceNext = lerpingDistance ? targetDistanceFinal : currentDistance;
            //float targetAngleNext = lerpingAngle ? targetAngleFinal : currentAngle;
            //float targetHeightNext = lerpingHeight ? targetHeightFinal : currentHeight;

            float targetDistanceNext = lerpingCylDistance ? targetDistanceSD : currentDistance;
            float targetAngleNext = lerpingCylAngle ? targetAngleSD : currentAngle;
            float targetHeightNext = lerpingCylHeight ? targetHeightSD : currentHeight;

            //Vector3 targetCylPos = WorldCylindricalForwardAngleZeroToCarthesian(targetDistanceSD, targetAngleSD, targetHeightSD);
            //Vector3 targetCylPos = WorldCylindricalForwardAngleZeroToCarthesian(targetDistanceFinal, targetAngleFinal, targetHeightFinal);
            Vector3 targetCylPos = WorldCylindricalForwardAngleZeroToCarthesian(targetDistanceNext, targetAngleNext, targetHeightNext);
            Vector3 targetPos = uiOriginTarget.position + targetCylPos;


            Vector3 targetPosSmooth = Vector3.SmoothDamp(uiTransform.position, targetPos, ref velocityCylindricalMov, 0.5f);
            uiTransform.position = targetPos;

            //if (lerpingDistance == true && Mathf.Abs(GetCylDistanceTargetFromOriginDelta()) < 0.01) lerpingDistance = false;
            //if (lerpingAngle == true && Mathf.Abs(GetCylAngleFromOrigin()) < 1) lerpingAngle = false;
            //if (lerpingHeight == true && Mathf.Abs(GetCylHeightTargetFromOriginDelta()) < 0.01) lerpingHeight = false;
        }

        private void StopLerpingDistance() => lerpingCylDistance = false;
        private void StopLerpingAngle() => lerpingCylAngle = false;
        private void StopLerpingHeight() => lerpingCylHeight = false;

        /// <summary>
        /// 
        /// </summary>
        private void UpdateRot()
        {
            uiTransform.rotation = Quaternion.identity;

            //Y Axis
            bool updateRot = !yAxisRotUpdateOnlyOnPosLerp || (yAxisRotUpdateOnlyOnPosLerp && (lerpingCarthesian || lerpingCylAngle));
            if (yAxisMode == YAxisMode.lookAtTargetYAxis && updateRot)
            {
                Vector3 uiTargetToUi = uiTransform.position - uiLookAtTarget.position;
                Quaternion targetRotation = Quaternion.LookRotation(uiTargetToUi, uiTransform.up);
                Vector3 targetEulerAngles = targetRotation.eulerAngles;
                uiTransform.rotation = Quaternion.Euler(uiTransform.eulerAngles.x, targetEulerAngles.y, uiTransform.eulerAngles.z);

                currentLocalRotY = uiTransform.rotation.eulerAngles.y;
            }
            else if (yAxisMode == YAxisMode.followTargetYAngle && updateRot)
            {
                Vector3 uiLookAtTargetRotEuler = uiLookAtTarget.rotation.eulerAngles;
                uiTransform.rotation = Quaternion.Euler(uiTransform.eulerAngles.x, uiLookAtTargetRotEuler.y, uiTransform.eulerAngles.z);

                currentLocalRotY = uiTransform.rotation.eulerAngles.y;
            }
            else if (yAxisMode == YAxisMode.lockedWorld)
            {
                uiTransform.rotation = Quaternion.Euler(uiTransform.eulerAngles.x, currentLocalRotY, uiTransform.eulerAngles.z);
            }
            //currentLocalRotY = uiTransform.rotation.eulerAngles.y;
            uiTransform.rotation = Quaternion.Euler(uiTransform.eulerAngles.x, currentLocalRotY + rotationLocalOffset.y, uiTransform.eulerAngles.z);


            //X Axis
            if (xAxisMode == XAxisMode.lookAtTargetXAxis)
            {
                Vector3 uiTargetToUi = uiTransform.position - uiLookAtTarget.position;
                Quaternion targetRotation = Quaternion.LookRotation(uiTargetToUi, uiTransform.up);
                Vector3 targetEulerAngles = targetRotation.eulerAngles;
                uiTransform.localRotation = Quaternion.Euler(targetEulerAngles.x, uiTransform.localEulerAngles.y, uiTransform.localEulerAngles.z);
            }
            else if (xAxisMode == XAxisMode.followTargetXAngle)
            {
                Vector3 uiLookAtTargetRotEuler = uiLookAtTarget.rotation.eulerAngles;
                uiTransform.localRotation = Quaternion.Euler(uiLookAtTargetRotEuler.x, uiTransform.localEulerAngles.y, uiTransform.localEulerAngles.z);
            }
            else if (xAxisMode == XAxisMode.locked)
            {
                uiTransform.rotation = Quaternion.Euler(currentLocalRotX, uiTransform.eulerAngles.y, uiTransform.eulerAngles.z);
            }
            currentLocalRotX = uiTransform.localRotation.eulerAngles.x;
            if (rotationLocalOffset.x != 0)
            {
                uiTransform.rotation = Quaternion.Euler(currentLocalRotX + rotationLocalOffset.x, uiTransform.eulerAngles.y, uiTransform.eulerAngles.z);
            }

            //Z Axis
            if (zAxisMode == ZAxisMode.followTargetZAngle)
            {
                Vector3 uiLookAtTargetRotEuler = uiLookAtTarget.rotation.eulerAngles;
                uiTransform.localRotation = Quaternion.Euler(uiTransform.localEulerAngles.x, uiTransform.localEulerAngles.y, uiLookAtTargetRotEuler.z);
            }
            else if (zAxisMode == ZAxisMode.locked)
            {
                uiTransform.rotation = Quaternion.Euler(uiTransform.eulerAngles.x, uiTransform.eulerAngles.y, currentLocalRotZ);
            }
            currentLocalRotZ = uiTransform.localRotation.eulerAngles.z;
            if (rotationLocalOffset.z != 0)
            {
                uiTransform.rotation = Quaternion.Euler(uiTransform.eulerAngles.x, uiTransform.eulerAngles.y, currentLocalRotZ + rotationLocalOffset.z);
            }
        }

        /// <summary>
        /// Call this function to get the distance between the UI and the target.
        /// </summary>
        /// <returns>float distance.</returns>
        private float GetUiOriginTargetDistDelta()
        {
            if (originLocalOffset == Vector3.zero)
            {
                return Vector3.Distance(uiTransform.position, uiOriginTarget.position);
            }
            else
            {
                Vector3 OriginPosWithLocalOffset = uiOriginTarget.TransformPoint(originLocalOffset);
                return Vector3.Distance(uiTransform.position, OriginPosWithLocalOffset);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private float GetCylDistanceFromOrigin()
        {
            float currentDistance = Vector3.Distance(new Vector3(uiOriginTarget.position.x, 0, uiOriginTarget.position.z),
                                                     new Vector3(uiTransform.position.x, 0, uiTransform.position.z));
            return currentDistance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private float GetCylDistanceTargetFromOriginDelta()
        {
            float currentDistance = GetCylDistanceFromOrigin();
            float targetDistanceDelta = Mathf.Abs(targetDistance - currentDistance);
            return targetDistanceDelta;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private float GetUiOriginTargetWorldYAngle()
        {
            Vector3 originForwardProjXZ = Vector3.Normalize(new Vector3(uiOriginTarget.transform.forward.x, 0, uiOriginTarget.transform.forward.z));
            float angleFromOrigin = Vector3.SignedAngle(Vector3.forward, originForwardProjXZ, Vector3.up);
            return angleFromOrigin;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private float GetUiCylCoordAngleFromOriginTargetWorld()
        {
            Vector3 originToUIVector = uiTransform.position - uiOriginTarget.transform.position;
            Vector3 originToUIVectorProjXZ = Vector3.Normalize(new Vector3(originToUIVector.x, 0, originToUIVector.z));
            float angleFromWorldOrigin = Vector3.SignedAngle(Vector3.forward, originToUIVectorProjXZ, Vector3.up);
            return angleFromWorldOrigin;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private float GetUiCylCoordAngleFromOriginTargetLocal()
        {
            Vector3 originForwardProjXZ = Vector3.Normalize(new Vector3(uiOriginTarget.transform.forward.x, 0, uiOriginTarget.transform.forward.z));
            Vector3 originToUIVector = uiTransform.position - uiOriginTarget.transform.position;
            Vector3 originToUIVectorProjXZ = Vector3.Normalize(new Vector3(originToUIVector.x, 0, originToUIVector.z));
            float angleFromOrigin = Vector3.SignedAngle(originForwardProjXZ, originToUIVectorProjXZ, Vector3.up);
            return angleFromOrigin;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private float GetCylAngleTargetFromOriginDelta()
        {
            float angleFromOrigin = GetUiCylCoordAngleFromOriginTargetLocal();
            float angleDelta = angleFromOrigin - targetAngle;
            return angleDelta;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private float GetCylHeightFromOrigin()
        {
            return uiTransform.position.y - uiOriginTarget.position.y;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private float GetCylHeightTargetFromOriginDelta()
        {
            return uiTransform.position.y - (uiOriginTarget.position.y + targetHeight);
        }

        #endregion
    }
}