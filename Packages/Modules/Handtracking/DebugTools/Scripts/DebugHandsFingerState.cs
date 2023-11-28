using Leap.Unity;
using UnityEngine;


namespace Lynx
{
    public class DebugHandsFingerState : MonoBehaviour
    {
        //PUBLIC
        public Transform cameraEye;
        [Space]
        public bool debugHandRight = true;
        public bool debugHandLeft = true;
        [Space]
        public bool debugThump = true;
        public bool debugIndex = true;
        public bool debugMiddle = true;
        public bool debugRing = true;
        public bool debugPinky = true;

        [Header("Debug 3D Elements")]
        public GameObject debugCubeTemplate;



        //PRIVATE
        private GameObject debugCubeRightThumb;
        private GameObject debugCubeRightIndex;
        private GameObject debugCubeRightMiddle;
        private GameObject debugCubeRightRing;
        private GameObject debugCubeRightPinky;

        private GameObject debugCubeLeftThumb;
        private GameObject debugCubeLeftIndex;
        private GameObject debugCubeLeftMiddle;
        private GameObject debugCubeLeftRing;
        private GameObject debugCubeLeftPinky;

        private GameObject[] debugCubesRight;
        private GameObject[] debugCubesLeft;

        private bool showDebug = true;



        private void OnValidate()
        {
            if (cameraEye == null) cameraEye = Camera.main.gameObject.transform;
        }

        private void Start()
        {
            if (debugCubeTemplate.activeSelf) debugCubeTemplate.SetActive(false);
            SetupHandDebugCubes();
        }

        private void Update()
        {
            if (showDebug) UpdateFingersExtendedDebugCubes();
        }



        //Public methods
        public void Show()
        {
            if (showDebug) return;

            if (debugHandRight) for (int i = 0; i < 5; i++) debugCubesRight[i].SetActive(true);
            if (debugHandLeft) for (int i = 0; i < 5; i++) debugCubesLeft[i].SetActive(true);

            showDebug = true;
        }
        public void Hide()
        {
            if (!showDebug) return;

            if (debugHandRight) for (int i = 0; i < 5; i++) debugCubesRight[i].SetActive(false);
            if (debugHandLeft) for (int i = 0; i < 5; i++) debugCubesLeft[i].SetActive(false);

            showDebug = false;
        }


        //Private methods
        private void SetupHandDebugCubes()
        {
            if (debugHandRight)
            {
                debugCubesRight = new GameObject[5];
                for (int i = 0; i < 5; i++)
                {
                    debugCubesRight[i] = Instantiate(debugCubeTemplate, debugCubeTemplate.transform.parent);
                    debugCubesRight[i].SetActive(false);
                }
            }
            if (debugHandLeft)
            {
                debugCubesLeft = new GameObject[5];
                for (int i = 0; i < 5; i++)
                {
                    debugCubesLeft[i] = Instantiate(debugCubeTemplate, debugCubeTemplate.transform.parent);
                    debugCubesLeft[i].SetActive(false);
                }
            }
        }

        private void UpdateFingersExtendedDebugCubes()
        {
            if (debugHandRight)
            {
                UpdateHandCube(Hands.Provider.GetHand(Chirality.Right), ref debugCubesRight);
            }
            if (debugHandLeft)
            {
                UpdateHandCube(Hands.Provider.GetHand(Chirality.Left), ref debugCubesLeft);
            }
        }
        private void UpdateHandCube(Leap.Hand hand, ref GameObject[] debugCubes)
        {
            if (hand == null)
            {
                for (int i = 0; i < 5; i++) debugCubes[i].SetActive(false);
            }
            else
            {
                //for (int i = 0; i < 5; i++) debugCubes[i].SetActive(true);

                if (debugThump) debugCubes[0].SetActive(true);
                if (debugIndex) debugCubes[1].SetActive(true);
                if (debugMiddle) debugCubes[2].SetActive(true);
                if (debugRing) debugCubes[3].SetActive(true);
                if (debugPinky) debugCubes[4].SetActive(true);

                if (debugThump) UpdateFingerCube(hand.GetThumb(), debugCubes[0]);
                if (debugIndex) UpdateFingerCube(hand.GetIndex(), debugCubes[1]);
                if (debugMiddle) UpdateFingerCube(hand.GetMiddle(), debugCubes[2]);
                if (debugRing) UpdateFingerCube(hand.GetRing(), debugCubes[3]);
                if (debugPinky) UpdateFingerCube(hand.GetPinky(), debugCubes[4]);
            }
        }
        private void UpdateFingerCube(Leap.Finger finger, GameObject debugCube)
        {
            Vector3 tipPos = finger.TipPosition;
            Vector3 tipDir = finger.Direction;
            bool isExtendedLeap = finger.IsExtended;
            debugCube.transform.position = tipPos;
            debugCube.transform.rotation = Quaternion.LookRotation(tipDir);
            debugCube.GetComponent<MeshRenderer>().material.color = isExtendedLeap ? Color.green : Color.red;
        }


        //Old
        private void SetupHandDebugCubesOld()
        {
            if (debugHandRight)
            {
                debugCubeRightThumb = Instantiate(debugCubeTemplate, debugCubeTemplate.transform.parent);
                debugCubeRightThumb.SetActive(true);
                debugCubeRightIndex = Instantiate(debugCubeTemplate, debugCubeTemplate.transform.parent);
                debugCubeRightIndex.SetActive(true);
                debugCubeRightMiddle = Instantiate(debugCubeTemplate, debugCubeTemplate.transform.parent);
                debugCubeRightMiddle.SetActive(true);
                debugCubeRightRing = Instantiate(debugCubeTemplate, debugCubeTemplate.transform.parent);
                debugCubeRightRing.SetActive(true);
                debugCubeRightPinky = Instantiate(debugCubeTemplate, debugCubeTemplate.transform.parent);
                debugCubeRightPinky.SetActive(true);
            }
            if (debugHandLeft)
            {
                debugCubeLeftThumb = Instantiate(debugCubeTemplate, debugCubeTemplate.transform.parent);
                debugCubeLeftThumb.SetActive(true);
                debugCubeLeftIndex = Instantiate(debugCubeTemplate, debugCubeTemplate.transform.parent);
                debugCubeLeftIndex.SetActive(true);
                debugCubeLeftMiddle = Instantiate(debugCubeTemplate, debugCubeTemplate.transform.parent);
                debugCubeLeftMiddle.SetActive(true);
                debugCubeLeftRing = Instantiate(debugCubeTemplate, debugCubeTemplate.transform.parent);
                debugCubeLeftRing.SetActive(true);
                debugCubeLeftPinky = Instantiate(debugCubeTemplate, debugCubeTemplate.transform.parent);
                debugCubeLeftPinky.SetActive(true);
            }
        }
        private void UpdateFingersExtendedDebugCubesOld()
        {
            if (debugHandRight)
            {
                Leap.Hand handRight = Hands.Provider.GetHand(Chirality.Right);
                if (handRight == null)
                {
                    debugCubeRightThumb.SetActive(false);
                    debugCubeRightIndex.SetActive(false);
                    debugCubeRightMiddle.SetActive(false);
                    debugCubeRightRing.SetActive(false);
                    debugCubeRightPinky.SetActive(false);
                }
                else
                {
                    debugCubeRightThumb.SetActive(true);
                    debugCubeRightIndex.SetActive(true);
                    debugCubeRightMiddle.SetActive(true);
                    debugCubeRightRing.SetActive(true);
                    debugCubeRightPinky.SetActive(true);

                    UpdateFingerCube(handRight.GetThumb(), debugCubeRightThumb);
                    UpdateFingerCube(handRight.GetIndex(), debugCubeRightIndex);
                    UpdateFingerCube(handRight.GetMiddle(), debugCubeRightMiddle);
                    UpdateFingerCube(handRight.GetRing(), debugCubeRightRing);
                    UpdateFingerCube(handRight.GetPinky(), debugCubeRightPinky);
                }
            }
            if (debugHandLeft)
            {
                Leap.Hand handLeft = Hands.Provider.GetHand(Chirality.Left);
                if (handLeft == null)
                {
                    debugCubeLeftThumb.SetActive(false);
                    debugCubeLeftIndex.SetActive(false);
                    debugCubeLeftMiddle.SetActive(false);
                    debugCubeLeftRing.SetActive(false);
                    debugCubeLeftPinky.SetActive(false);
                }
                else
                {
                    debugCubeLeftThumb.SetActive(true);
                    debugCubeLeftIndex.SetActive(true);
                    debugCubeLeftMiddle.SetActive(true);
                    debugCubeLeftRing.SetActive(true);
                    debugCubeLeftPinky.SetActive(true);

                    UpdateFingerCube(handLeft.GetThumb(), debugCubeLeftThumb);
                    UpdateFingerCube(handLeft.GetIndex(), debugCubeLeftIndex);
                    UpdateFingerCube(handLeft.GetMiddle(), debugCubeLeftMiddle);
                    UpdateFingerCube(handLeft.GetRing(), debugCubeLeftRing);
                    UpdateFingerCube(handLeft.GetPinky(), debugCubeLeftPinky);
                }
            }
        }
    }
}