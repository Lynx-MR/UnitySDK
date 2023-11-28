//   ==============================================================================
//   | Lynx (2023)                                                                  |
//   |======================================                                        |
//   | LynxUIPointerManager Script                                                  |
//   | Script to manage pointer shader property                                     |
//   ==============================================================================

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;

namespace Lynx
{
    public class LynxUIPointerManager : MonoBehaviour
    {
        [SerializeField] private InputActionReference leftHandDirection;
        [SerializeField] private InputActionReference RightHandDirection;
        [SerializeField] private float pointerRadius = 0.02f;
        [SerializeField] private float pointerDist = 0.3f;
        [SerializeField] private Color pointerColor = Color.white;

        private void Start()
        {
            Shader.SetGlobalFloat("_PointerSize", pointerRadius);
            Shader.SetGlobalFloat("_PointerDist", pointerDist);
            Shader.SetGlobalVector("_PointerColor", pointerColor);


            XRHandSubsystem m_Subsystem =
                XRGeneralSettings.Instance?
                    .Manager?
                    .activeLoader?
                    .GetLoadedSubsystem<XRHandSubsystem>();

            if (m_Subsystem != null)
                m_Subsystem.updatedHands += OnHandUpdate;
        }
        void OnHandUpdate(XRHandSubsystem subsystem,
                      XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags,
                      XRHandSubsystem.UpdateType updateType)
        {
            switch (updateType)
            {
                case XRHandSubsystem.UpdateType.Dynamic:
                    // Update game logic that uses hand data
                    break;
                case XRHandSubsystem.UpdateType.BeforeRender:
                    if (leftHandDirection.action != null)
                    {
                        Vector3 lForward = (leftHandDirection.action.ReadValue<Quaternion>() * Vector3.forward);
                        Shader.SetGlobalVector("_PointerLDir", new Vector4(lForward.x, lForward.y, lForward.z, 1.0f));
                    }
                    if (RightHandDirection.action != null)
                    {
                        Vector3 rForward = RightHandDirection.action.ReadValue<Quaternion>() * Vector3.forward;
                        Shader.SetGlobalVector("_PointerRDir", new Vector4(rForward.x, rForward.y, rForward.z, 1.0f));
                    }

                    XRHand lHand = LynxHandtrackingAPI.LeftHand;
                    XRHand rHand = LynxHandtrackingAPI.RightHand;
                    if (rHand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out Pose poseR))
                    {
                        Vector4 rPos = poseR.position;
                        Shader.SetGlobalVector("_PointerRPos", new Vector4(rPos.x, rPos.y, rPos.z, 1.0f));
                    }
                    if (lHand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out Pose poseL))
                    {
                        Vector4 lPos = poseL.position;
                        Shader.SetGlobalVector("_PointerLPos", new Vector4(lPos.x, lPos.y, lPos.z, 1.0f));
                    }
                    break;
            }
        }
    }
}
