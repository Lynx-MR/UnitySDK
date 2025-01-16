/**
 * @file LynxHandtrackingAPI.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief API to easily access Hands through XR Hands subsystem.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;


namespace Lynx
{
    public static class LynxHandtrackingAPI 
    { 
        private static XRHandSubsystem m_handSubsystem = null;

        static LynxHandtrackingAPI()
        {
            List<XRHandSubsystem> handSusbsytems = new List<XRHandSubsystem>();
            SubsystemManager.GetSubsystems(handSusbsytems);
            if (handSusbsytems.Count > 0)
            {
                m_handSubsystem = handSusbsytems[0];
                EnableUpdate();
            }
        }

        public static bool IsHandSubsystemEnabled => m_handSubsystem != null;

        public static void EnableUpdate()
        {
            if (m_handSubsystem != null)
                m_handSubsystem.updatedHands += OnHandUpdate;
        }

        public static XRHandSubsystem HandSubsystem  {
            get
            {
                return m_handSubsystem;
            }
        }

        public static XRHand LeftHand => HandSubsystem.leftHand;
        public static XRHand RightHand => HandSubsystem.rightHand;

        public static event Action LeftHandDynamicUpdate;
        public static event Action RightHandDynamicUpdate;
        public static event Action LeftHandBeforeRenderUpdate;
        public static event Action RightHandBeforeRenderUpdate;

        private static void OnHandUpdate(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
        {
            if (subsystem.leftHand.isTracked)
            {
                if (updateType == XRHandSubsystem.UpdateType.Dynamic)
                    LeftHandDynamicUpdate?.Invoke();

                if (updateType == XRHandSubsystem.UpdateType.BeforeRender)
                    LeftHandBeforeRenderUpdate?.Invoke();
            }

            if (subsystem.rightHand.isTracked)
            {
                if (updateType == XRHandSubsystem.UpdateType.Dynamic)
                    RightHandDynamicUpdate?.Invoke();

                if (updateType == XRHandSubsystem.UpdateType.BeforeRender)
                    RightHandBeforeRenderUpdate?.Invoke();
            }
        }
    }

}