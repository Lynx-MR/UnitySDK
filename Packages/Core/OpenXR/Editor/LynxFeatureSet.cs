/**
 * @file LynxFeatureSet.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief Create a Feature group for Lynx headset (available in Project Settigns under OpenXR tab).
 */

#if LYNX_OPENXR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif


namespace Lynx.OpenXR
{
#if LYNX_OPENXR
    [OpenXRFeatureSet(
        SupportedBuildTargets = new BuildTargetGroup[] { BuildTargetGroup.Android },
        RequiredFeatureIds = new string[]
        {
            LynxR1Feature.featureId
        },
        FeatureIds = new string[]
        {

            LynxR1Feature.featureId,
            xrHandsSubsystemId,
#if UNITY_6000_0_OR_NEWER
            xrHandInteractionPoses
#else
            xrHandtrackingAim
#endif
        },
        UiName = "Lynx",
        Description = "Feature set that allows for setting up the best environment for My Company's hardware.",
        FeatureSetId = featureId
    )]
#endif

    public class LynxFeatureSet
    {
        public const string featureId = "com.lynx.openxr.featureset";
        public const string xrHandsSubsystemId = "com.unity.openxr.feature.input.handtracking";
        public const string xrHandtrackingAim = "com.unity.openxr.feature.input.metahandtrackingaim";
        public const string xrHandInteractionPoses = "com.unity.openxr.feature.input.handinteractionposes";
    }
}
