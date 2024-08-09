/**
 * @file HandtrackingControllerAdapter.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief Handtracking implementation of the Lynx XRController.
 */

using UnityEngine;
using UnityEngine.Scripting;

[assembly: AlwaysLinkAssembly]
namespace Lynx
{
    public class HandtrackingControllerAdapter : AXRControllerManager
    {

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoad()
        {
            if (LynxHandtrackingAPI.IsHandSubsystemEnabled)
                SetInstance(EControllerPriority.HANDTRACKING, new HandtrackingControllerAdapter());
        }


        #region OVERRIDES
        public override bool IsTracked()
        {
            return LynxHandtrackingAPI.LeftHand.isTracked || LynxHandtrackingAPI.RightHand.isTracked;
        }

        public override bool IsTracked(EHandedness handedness)
        {
            return handedness == EHandedness.Left ? LynxHandtrackingAPI.LeftHand.isTracked : LynxHandtrackingAPI.RightHand.isTracked;
        }

        public override Vector3 GetPose()
        {
            Vector3 res = Vector3.zero;
            Pose p;
            if (LynxHandtrackingAPI.LeftHand.isTracked)
            {
                if(LynxHandtrackingAPI.LeftHand.GetJoint(UnityEngine.XR.Hands.XRHandJointID.Palm).TryGetPose(out p))
                    res = p.position;
            }

            else if (LynxHandtrackingAPI.RightHand.isTracked)
            {
                if(LynxHandtrackingAPI.RightHand.GetJoint(UnityEngine.XR.Hands.XRHandJointID.Palm).TryGetPose(out p))
                    res = p.position;
            }

            return res;
        }

        public override Vector3 GetPose(EHandedness handedness)
        {
            Vector3 res = Vector3.zero;
            Pose p;
            if (handedness == EHandedness.Left && LynxHandtrackingAPI.LeftHand.isTracked)
            {
                if (LynxHandtrackingAPI.LeftHand.GetJoint(UnityEngine.XR.Hands.XRHandJointID.Palm).TryGetPose(out p))
                    res = p.position;
            }
            else if (handedness == EHandedness.Right && LynxHandtrackingAPI.RightHand.isTracked)
            {
                if (LynxHandtrackingAPI.RightHand.GetJoint(UnityEngine.XR.Hands.XRHandJointID.Palm).TryGetPose(out p))
                    res = p.position;
            }

            return res;
        }
        #endregion
    }
}