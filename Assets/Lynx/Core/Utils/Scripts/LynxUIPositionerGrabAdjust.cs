using UnityEngine;

namespace Lynx.UI
{
    [RequireComponent(typeof(LynxUIPositioner))]
    public class LynxUIPositionerGrabAdjust : MonoBehaviour
    {
        //INSPECTOR
        public LynxUIPositioner lynxUIPositioner;
        public Transform grabberTransform;
        public Transform grabbableTransform;
        [Space]
        public bool isGrabbing = false;

        //PRIVATE
        private bool isGrabbingPrev = false;
        private float grabStartYPos;
        private float grabStartForwardPos;

        private Vector3 originLocalOffset;
        private Vector3 rotationLocalOffset;

        private float targetHeight;
        private float targetHeightThreshold;
        private float targetHeightLerpTime;

        private float grabStartRotationXOffset;
        private float grabHoldRotationXOffset;



        private void Awake()
        {
            if (lynxUIPositioner == null) lynxUIPositioner = GetComponent<LynxUIPositioner>();
        }

        private void Start()
        {
            originLocalOffset = lynxUIPositioner.originLocalOffset;
            rotationLocalOffset = lynxUIPositioner.rotationLocalOffset;

            targetHeight = lynxUIPositioner.targetHeight;
            targetHeightThreshold = lynxUIPositioner.targetHeightThreshold;
            targetHeightLerpTime = lynxUIPositioner.targetHeightLerpTime;
        }

        private void Update()
        {
            if (!isGrabbingPrev && isGrabbing)
            {
                OnGrabStart();
            }
            if (isGrabbingPrev && isGrabbing)
            {
                OnGrabHold();
            }
            if (isGrabbingPrev && !isGrabbing)
            {
                OnGrabEnd();
            }

            isGrabbingPrev = isGrabbing;
        }




        public void OnGrabStart()
        {
            //Height adjust
            lynxUIPositioner.targetHeightThreshold = 0;
            lynxUIPositioner.targetHeightLerpTime = 0;
            grabStartYPos = grabberTransform.position.y;

            //X axis angle adjust
            //grabStartForwardPos = lynxUIPositionner.uiTransform.InverseTransformPoint(grabberTransform.position).z;
            grabStartForwardPos = GetGrabForwardPosFromUIOriginWithGrabbableYOffset();

            grabStartRotationXOffset = lynxUIPositioner.rotationLocalOffset.x;
        }
        public void OnGrabHold()
        {
            //Height adjust
            float grabStartYPosDelta = grabberTransform.position.y - grabStartYPos;
            lynxUIPositioner.targetHeight = targetHeight + grabStartYPosDelta;

            //X axis angle adjust
            //float grabStartForwardPosDelta = grabberTransform.position.z - grabStartForwardPos;
            float grabStartForwardPosDelta = GetGrabForwardPosFromUIOriginWithGrabbableYOffset() - grabStartForwardPos;
            Debug.Log($"grabStartForwardPosDelta = {grabStartForwardPosDelta}");
            float grabbableZDistFromUIOrigin = -lynxUIPositioner.uiTransform.InverseTransformPoint(grabbableTransform.position).y;

            float maxAngleForwardRad = Mathf.Deg2Rad * 45f;
            float maxForwardDelta = Mathf.Tan(maxAngleForwardRad) * grabbableZDistFromUIOrigin;

            float forwardDeltaVal = Mathf.Clamp(grabStartForwardPosDelta, 0, maxForwardDelta);
            float forwardDeltaSign = Mathf.Sign(grabStartForwardPosDelta);

            float angleForwardDelta = Mathf.Lerp(0, 45f, Mathf.InverseLerp(0, maxForwardDelta, forwardDeltaVal)) * forwardDeltaSign;

            grabHoldRotationXOffset = grabStartRotationXOffset + angleForwardDelta;
            //Debug.Log($"grabHoldRotationXOffset = {grabHoldRotationXOffset}");

            lynxUIPositioner.rotationLocalOffset = new Vector3(grabHoldRotationXOffset, lynxUIPositioner.rotationLocalOffset.y, lynxUIPositioner.rotationLocalOffset.z);
        }
        public void OnGrabEnd()
        {
            lynxUIPositioner.targetHeightThreshold = targetHeightThreshold;
            lynxUIPositioner.targetHeightLerpTime = targetHeightLerpTime;
            targetHeight = lynxUIPositioner.targetHeight;
        }


        public float GetGrabForwardPosFromUIOriginWithGrabbableYOffset()
        {
            Vector3 UIForward = lynxUIPositioner.uiTransform.forward;
            Vector3 UIForwardProjXZ = new Vector3(UIForward.x, 0f, UIForward.z);
            float UIGrabbableLocalYDist = lynxUIPositioner.uiTransform.InverseTransformPoint(grabberTransform.position).y;
            Vector3 UIPosWithGrabbableYDistWorldOffest = lynxUIPositioner.uiTransform.position + (Vector3.up * UIGrabbableLocalYDist);

            float grabStartForwardPosFromUIOriginWithGrabbableYOffset = Vector3.Dot(grabberTransform.position - UIPosWithGrabbableYDistWorldOffest, UIForwardProjXZ);

            return grabStartForwardPosFromUIOriginWithGrabbableYOffset;

        }
    }
}