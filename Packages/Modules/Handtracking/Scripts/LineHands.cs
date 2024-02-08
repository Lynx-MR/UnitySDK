//   ==============================================================================
//   | Lynx Mixed Reality                                                         |
//   |======================================                                      |
//   | Lynx SDK                                                                   |
//   | Generate hands with LineRenderer and sphere on tracking found              |
//   ==============================================================================


using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;


namespace Lynx
{

    public class LineHands : MonoBehaviour
    {
        [Range(0.005f, 0.03f)]
        public float jointScale = 0.01f;
        [Range(0.0025f, 0.03f)]
        public float lineScale = 0.005f;
        public Material LineMaterial;
        
        public enum HandsRendering
        {
            Both,
            Left,
            Right
        }

#pragma warning disable 0414
        [SerializeField] private HandsRendering handsToRender = HandsRendering.Both;
#pragma warning restore 0414

        #region ADVANCED COLOR PARAMETERS
        public enum ColorMode
        {
            Random,
            Color,
            Palette
        }
        public ColorMode colorMode = ColorMode.Color;

        public Color leftHandColor = Color.white;
        public Color rightHandColor = Color.white;

        public Color[] leftHandsPalette;
        public Color[] rightHandsPalette;



        private uint leftPaletteID;
        private uint rightPaletteID;
        #endregion

#if !UNITY_EDITOR

        #region SAFETY VARIABLES

    private bool isEnabled = false;
    private bool leftLine = false;
    private bool rightLine = false;
    private bool leftMarker = false;
    private bool rightMarker = false;

        #endregion

        #region HANDS OBJECTS
    private GameObject markerRootL;
    private GameObject markerRootR;
    private GameObject[] leftMarkers;
    private GameObject[] rightMarkers;

    private GameObject lineRootL;
    private GameObject lineRootR;

    private LineRenderer leftPalm;
    private LineRenderer leftThumb;
    private LineRenderer leftIndex;
    private LineRenderer leftMiddle;
    private LineRenderer leftRing;
    private LineRenderer leftLittle;

    private LineRenderer rightPalm;
    private LineRenderer rightThumb;
    private LineRenderer rightIndex;
    private LineRenderer rightMiddle;
    private LineRenderer rightRing;
    private LineRenderer rightLittle;
        #endregion

    //INIT
    void Start()
    {
        XRHandSubsystem m_Subsystem =
        XRGeneralSettings.Instance?
            .Manager?
            .activeLoader?
            .GetLoadedSubsystem<XRHandSubsystem>();

        if (m_Subsystem != null)
        {
            //ands Marker
            m_Subsystem.trackingAcquired += GenerateHandMarker;
            m_Subsystem.updatedHands += OnHandUpdateMarker;
            m_Subsystem.trackingLost += DestroyHandsMarker;

            //Hands Lines
            m_Subsystem.trackingAcquired += GenerateHandLine;
            m_Subsystem.updatedHands += OnHandUpdateLine;
            m_Subsystem.trackingLost += DestroyHandsLine;

        }
        
    }

        #region ENABLED SAFETY

    private void OnEnable()
    {
        isEnabled = true;

        //Generate hands if hands already tracked on enable
        if (LynxHandtrackingAPI.LeftHand.isTracked)
        {
            GenerateHandLine(LynxHandtrackingAPI.LeftHand);
            GenerateHandMarker(LynxHandtrackingAPI.LeftHand);
        }

        if (LynxHandtrackingAPI.RightHand.isTracked)
        {
            GenerateHandLine(LynxHandtrackingAPI.RightHand);
            GenerateHandMarker(LynxHandtrackingAPI.RightHand);
        }
    }

    private void OnDisable()
    {
        //Destroy hands when game object is disabled
        XRHand left = LynxHandtrackingAPI.LeftHand;
        XRHand right = LynxHandtrackingAPI.RightHand;
        if(leftLine)
            DestroyHandsLine(left);
        if(rightLine)
            DestroyHandsLine(right);
        if(leftMarker)
            DestroyHandsMarker(left);
        if(rightMarker)
            DestroyHandsMarker(right);


        isEnabled = false;
    }

        #endregion


        #region MARKER FUNCTION


    void OnHandUpdateMarker(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
    {
        if (updateType == XRHandSubsystem.UpdateType.BeforeRender && isEnabled)
        {
            if (subsystem.leftHand.isTracked && leftMarker)
            {
                markerRootL.transform.position = this.transform.position;
                markerRootL.transform.rotation = this.transform.rotation;

                UpdateHandMarkerTrs(subsystem.leftHand, leftMarkers);
            }

            if (subsystem.rightHand.isTracked && rightMarker)
            {
                markerRootR.transform.position = this.transform.position;
                markerRootR.transform.rotation = this.transform.rotation;

                UpdateHandMarkerTrs(subsystem.rightHand, rightMarkers);

            }
        }
    }


    /// <summary>
    /// Generate a game object for each bone in the hand and store them in the appropriate array.
    /// </summary>
    /// <param name="hand"></param>
    private void GenerateHandMarker(XRHand hand)
    {
        //Do not generate if requirment not met
        if (!isEnabled)
            return;
        if (hand.handedness == Handedness.Right && handsToRender == HandsRendering.Left)
            return;
        if (hand.handedness == Handedness.Left && handsToRender == HandsRendering.Right)
            return;

        //Parent of all markers for given hand
        GameObject handRoot = new GameObject();

        //Set color depending of selected option
        Color colorToSet = Color.white;
        if(colorMode == ColorMode.Random)
            colorToSet = Color.HSVToRGB(Random.value, 0.5f, 1.0f);
        else if(colorMode == ColorMode.Palette)
        {
            if (hand.handedness == Handedness.Left && leftHandsPalette.Length > 0)
            {
                ++leftPaletteID;
                colorToSet = leftHandsPalette[leftPaletteID%leftHandsPalette.Length];
            }
            else if (hand.handedness == Handedness.Right && rightHandsPalette.Length > 0)
            {
                ++rightPaletteID;
                colorToSet = rightHandsPalette[rightPaletteID % rightHandsPalette.Length];
            }
        }
        else if(colorMode == ColorMode.Color)
        {
            if (hand.handedness == Handedness.Left)
                colorToSet = leftHandColor;
            else
                colorToSet = rightHandColor;
        }
            

        if (hand.handedness == Handedness.Right)
        {
            markerRootR = handRoot;
            rightMarker = true;

        }
        else if(hand.handedness == Handedness.Left)
        {
            markerRootL = handRoot;
            leftMarker = true;
        }

        //Init markers list for all joints
        GameObject[] markers = new GameObject[XRHandJointID.EndMarker.ToIndex()];


        //Create a sphere for each joint and set its position
        for (var i = XRHandJointID.BeginMarker.ToIndex(); i < XRHandJointID.EndMarker.ToIndex(); i++)
        {
            markers[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            markers[i].transform.parent = handRoot.transform;

            markers[i].GetComponent<MeshRenderer>().material.color = colorToSet;
        }

        //Set all markers object to the corresponding list.
        if (hand.handedness == Handedness.Right)
            rightMarkers = markers;
        else
            leftMarkers = markers;
    }

    /// <summary>
    /// Update transform of each markers in the given hand
    /// </summary>
    /// <param name="hand">XRHand to take data from</param>
    /// <param name="markers">GameObjects that take transform data from the hand</param>
    private void UpdateHandMarkerTrs(XRHand hand, GameObject[] markers)
    {
        if (hand.handedness == Handedness.Right && handsToRender == HandsRendering.Left)
            return;
        if (hand.handedness == Handedness.Left && handsToRender == HandsRendering.Right)
            return;

        for (var i = XRHandJointID.BeginMarker.ToIndex(); i < XRHandJointID.EndMarker.ToIndex(); i++)
        {
            XRHandJoint trackingData = hand.GetJoint(XRHandJointIDUtility.FromIndex(i));

            if (trackingData.TryGetPose(out Pose pose))
            {
                markers[i].transform.localPosition = pose.position;
                markers[i].transform.localRotation = pose.rotation;
                markers[i].transform.localScale = Vector3.one * jointScale;
            }
        }
    }

    /// <summary>
    /// Destroy hands parent and all childs
    /// </summary>
    /// <param name="hand"></param>
    private void DestroyHandsMarker(XRHand hand)
    {
        if (hand.handedness == Handedness.Right && handsToRender == HandsRendering.Left)
            return;
        if (hand.handedness == Handedness.Left && handsToRender == HandsRendering.Right)
            return;

        if (hand.handedness == Handedness.Right)
        {
            Destroy(markerRootR);
            rightMarker = false;
        }
        else
        {
            Destroy(markerRootL);
            leftMarker = false;
        }
    }

        #endregion


        #region LINE FUNCTION

    /// <summary>
    /// Create a line renderer for the palm and fingers
    /// </summary>
    /// <param name="hand"></param>
    private void GenerateHandLine(XRHand hand)
    {
        if (hand.handedness == Handedness.Right && handsToRender == HandsRendering.Left)
            return;
        if (hand.handedness == Handedness.Left && handsToRender == HandsRendering.Right)
            return;
        //Do not generate line when gameobject is disabled
        if (!isEnabled)
            return;

        if (hand.handedness == Handedness.Left)
        {
            lineRootL = new GameObject("LineRoot Left");
            leftLine = true;

            //palm
            leftPalm = new GameObject("LeftPalmLine").AddComponent<LineRenderer>();
            leftPalm.positionCount = 9;
            leftPalm.startWidth = lineScale;
            leftPalm.endWidth = lineScale;
            leftPalm.material = LineMaterial;
            leftPalm.transform.parent = lineRootL.transform;
            //Thumb
            leftThumb = new GameObject("LeftThumbLine").AddComponent<LineRenderer>();
            leftThumb.positionCount = 4;
            leftThumb.startWidth = lineScale;
            leftThumb.endWidth = lineScale;
            leftThumb.material = LineMaterial;
            leftThumb.transform.parent = lineRootL.transform;
            //Index
            leftIndex = new GameObject("LeftIndexLine").AddComponent<LineRenderer>();
            leftIndex.positionCount = 4;
            leftIndex.startWidth = lineScale;
            leftIndex.endWidth = lineScale;
            leftIndex.material = LineMaterial;
            leftIndex.transform.parent = lineRootL.transform;
            //Middle
            leftMiddle = new GameObject("LeftMiddleLine").AddComponent<LineRenderer>();
            leftMiddle.positionCount = 4;
            leftMiddle.startWidth = lineScale;
            leftMiddle.endWidth = lineScale;
            leftMiddle.material = LineMaterial;
            leftMiddle.transform.parent = lineRootL.transform;
            //Ring
            leftRing = new GameObject("LeftRingine").AddComponent<LineRenderer>();
            leftRing.positionCount = 4;
            leftRing.startWidth = lineScale;
            leftRing.endWidth = lineScale;
            leftRing.material = LineMaterial;
            leftRing.transform.parent = lineRootL.transform;
            //Little
            leftLittle = new GameObject("LeftLittleLine").AddComponent<LineRenderer>();
            leftLittle.positionCount = 4;
            leftLittle.startWidth = lineScale;
            leftLittle.endWidth = lineScale;
            leftLittle.material = LineMaterial;
            leftLittle.transform.parent = lineRootL.transform;

        }

        if (hand.handedness == Handedness.Right)
        {
            lineRootR = new GameObject("LineRoot Right");
            rightLine = true;

            //palm
            rightPalm = new GameObject("RightPalmLine").AddComponent<LineRenderer>();
            rightPalm.positionCount = 9;
            rightPalm.startWidth = lineScale;
            rightPalm.endWidth = lineScale;
            rightPalm.material = LineMaterial;
            rightPalm.transform.parent = lineRootR.transform;
            //Thumb
            rightThumb = new GameObject("RightThumbLine").AddComponent<LineRenderer>();
            rightThumb.positionCount = 4;
            rightThumb.startWidth = lineScale;
            rightThumb.endWidth = lineScale;
            rightThumb.material = LineMaterial;
            rightThumb.transform.parent = lineRootR.transform;
            //Index
            rightIndex = new GameObject("RightIndexLine").AddComponent<LineRenderer>();
            rightIndex.positionCount = 4;
            rightIndex.startWidth = lineScale;
            rightIndex.endWidth = lineScale;
            rightIndex.material = LineMaterial;
            rightIndex.transform.parent = lineRootR.transform;
            //Middle
            rightMiddle = new GameObject("RightMiddleLine").AddComponent<LineRenderer>();
            rightMiddle.positionCount = 4;
            rightMiddle.startWidth = lineScale;
            rightMiddle.endWidth = lineScale;
            rightMiddle.material = LineMaterial;
            rightMiddle.transform.parent = lineRootR.transform;
            //Ring
            rightRing = new GameObject("RightRingine").AddComponent<LineRenderer>();
            rightRing.positionCount = 4;
            rightRing.startWidth = lineScale;
            rightRing.endWidth = lineScale;
            rightRing.material = LineMaterial;
            rightRing.transform.parent = lineRootR.transform;
            //Little
            rightLittle = new GameObject("RightLittleLine").AddComponent<LineRenderer>();
            rightLittle.positionCount = 4;
            rightLittle.startWidth = lineScale;
            rightLittle.endWidth = lineScale;
            rightLittle.material = LineMaterial;
            rightLittle.transform.parent = lineRootR.transform;

        }
    }

    /// <summary>
    /// Update the position of each point of all line renderers to match tracking data based on markers
    /// </summary>
    /// <param name="subsystem"></param>
    /// <param name="updateSuccessFlags"></param>
    /// <param name="updateType"></param>
    private void OnHandUpdateLine(XRHandSubsystem subsystem, XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags, XRHandSubsystem.UpdateType updateType)
    {
        if (updateType == XRHandSubsystem.UpdateType.BeforeRender && isEnabled)
        {
        #region HANDS JOINTS INDEX INFO
            //This is the list of default XRHands indexes for each hand joint.
            // 00 = Wrist
            // 01 = Palm

            // 02 = Thumb Metacarpal
            // 03 = Thumb Proximal
            // 04 = Thumb Distal
            // 05 = Thumb Tip

            // 06 = Index Metacarpal
            // 07 = Index Proximal
            // 08 = Index Intermediate
            // 09 = Index Distal
            // 10 = Index Tip

            // 11 = Middle Metacarpal
            // 12 = Middle Proximal
            // 13 = Middle Intermediate
            // 14 = Middle Distal
            // 15 = Middle Tip

            // 16 = Ring Metacarpal
            // 17 = Ring Proximal
            // 18 = Ring Intermediate
            // 19 = Ring Distal
            // 20 = Ring Tip

            // 21 = Little Metacarpal
            // 22 = Little Proximal
            // 23 = Little Intermediate
            // 24 = Little Distal
            // 25 = Little Tip
        #endregion

            if (subsystem.leftHand.isTracked && leftLine && handsToRender != HandsRendering.Right)
            {
                // Palm
                Vector3[] newPos = new Vector3[9]; 
                newPos[0] = leftMarkers[21].transform.position;
                newPos[1] = leftMarkers[16].transform.position;
                newPos[2] = leftMarkers[11].transform.position;
                newPos[3] = leftMarkers[6].transform.position;
                newPos[4] = leftMarkers[7].transform.position;
                newPos[5] = leftMarkers[12].transform.position;
                newPos[6] = leftMarkers[17].transform.position;
                newPos[7] = leftMarkers[22].transform.position;
                newPos[8] = leftMarkers[21].transform.position;
                leftPalm.SetPositions(newPos);

                //Thumb
                newPos = new Vector3[4];
                newPos[0] = leftMarkers[2].transform.position;
                newPos[1] = leftMarkers[3].transform.position;
                newPos[2] = leftMarkers[4].transform.position;
                newPos[3] = leftMarkers[5].transform.position;
                leftThumb.SetPositions(newPos);

                //Index
                newPos[0] = leftMarkers[7].transform.position;
                newPos[1] = leftMarkers[8].transform.position;
                newPos[2] = leftMarkers[9].transform.position;
                newPos[3] = leftMarkers[10].transform.position;
                leftIndex.SetPositions(newPos);

                //Middle
                newPos[0] = leftMarkers[12].transform.position;
                newPos[1] = leftMarkers[13].transform.position;
                newPos[2] = leftMarkers[14].transform.position;
                newPos[3] = leftMarkers[15].transform.position;
                leftMiddle.SetPositions(newPos);

                //Ring
                newPos[0] = leftMarkers[17].transform.position;
                newPos[1] = leftMarkers[18].transform.position;
                newPos[2] = leftMarkers[19].transform.position;
                newPos[3] = leftMarkers[20].transform.position;
                leftRing.SetPositions(newPos);

                //Little
                newPos[0] = leftMarkers[22].transform.position;
                newPos[1] = leftMarkers[23].transform.position;
                newPos[2] = leftMarkers[24].transform.position;
                newPos[3] = leftMarkers[25].transform.position;
                leftLittle.SetPositions(newPos);
            }

            if (subsystem.rightHand.isTracked && rightLine && handsToRender != HandsRendering.Left)
            {
                // Palm
                Vector3[] newPos = new Vector3[9];
                newPos[0] = rightMarkers[21].transform.position;
                newPos[1] = rightMarkers[16].transform.position;
                newPos[2] = rightMarkers[11].transform.position;
                newPos[3] = rightMarkers[6].transform.position;
                newPos[4] = rightMarkers[7].transform.position;
                newPos[5] = rightMarkers[12].transform.position;
                newPos[6] = rightMarkers[17].transform.position;
                newPos[7] = rightMarkers[22].transform.position;
                newPos[8] = rightMarkers[21].transform.position;
                rightPalm.SetPositions(newPos);

                //Thumb
                newPos = new Vector3[4];
                newPos[0] = rightMarkers[2].transform.position;
                newPos[1] = rightMarkers[3].transform.position;
                newPos[2] = rightMarkers[4].transform.position;
                newPos[3] = rightMarkers[5].transform.position;
                rightThumb.SetPositions(newPos);

                //Index
                newPos[0] = rightMarkers[7].transform.position;
                newPos[1] = rightMarkers[8].transform.position;
                newPos[2] = rightMarkers[9].transform.position;
                newPos[3] = rightMarkers[10].transform.position;
                rightIndex.SetPositions(newPos);

                //Middle
                newPos[0] = rightMarkers[12].transform.position;
                newPos[1] = rightMarkers[13].transform.position;
                newPos[2] = rightMarkers[14].transform.position;
                newPos[3] = rightMarkers[15].transform.position;
                rightMiddle.SetPositions(newPos);

                //Ring
                newPos[0] = rightMarkers[17].transform.position;
                newPos[1] = rightMarkers[18].transform.position;
                newPos[2] = rightMarkers[19].transform.position;
                newPos[3] = rightMarkers[20].transform.position;
                rightRing.SetPositions(newPos);

                //Little
                newPos[0] = rightMarkers[22].transform.position;
                newPos[1] = rightMarkers[23].transform.position;
                newPos[2] = rightMarkers[24].transform.position;
                newPos[3] = rightMarkers[25].transform.position;
                rightLittle.SetPositions(newPos);
            }
        }
    }

    /// <summary>
    /// Detroy each line renderer corresponding to a given hand.
    /// </summary>
    /// <param name="hand"></param>
    private void DestroyHandsLine(XRHand hand)
    {
        if (!isEnabled)
            return;
        if (hand.handedness == Handedness.Right && handsToRender == HandsRendering.Left)
            return;
        if (hand.handedness == Handedness.Left && handsToRender == HandsRendering.Right)
            return;


        if (hand.handedness == Handedness.Left && leftLine)
        {
            Destroy(lineRootL);

            leftLine = false;
        }
        if (hand.handedness == Handedness.Right && rightLine)
        {
            Destroy(lineRootR);

            rightLine = true;
        }
    }
    

        #endregion

#endif
    }
}