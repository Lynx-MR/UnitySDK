using Leap.Unity;
using System.Text;
using TMPro;
using UnityEngine;

namespace Lynx
{
    public class DebugHandsText : MonoBehaviour
    {
        //PUBLIC
        public Transform cameraEye;
        [Header("Debug displays")]
        public GameObject debugCanvasTemplate;

        //PRIVATE
        private GameObject debugCanvasHandRight;
        private GameObject debugCanvasHandLeft;

        private TextMeshProUGUI debugTMPHandRight;
        private TextMeshProUGUI debugTMPHandLeft;
        
        private void OnValidate()
        {
            if (cameraEye == null) cameraEye = Camera.main.gameObject.transform;
        }

        private void Start()
        {
            debugCanvasTemplate.SetActive(false);
            InstantiateDebugCanvases();
        }

        private void Update()
        {
            UpdateDebugCanvases();
        }


        //Debug text methods
        private void InstantiateDebugCanvases()
        {
            InstantiateDebugCanvas(ref debugCanvasHandRight, ref debugTMPHandRight);
            InstantiateDebugCanvas(ref debugCanvasHandLeft, ref debugTMPHandLeft);
        }
        private void InstantiateDebugCanvas(ref GameObject debugCanvas, ref TextMeshProUGUI debugTMP)
        {
            debugCanvas = Instantiate(debugCanvasTemplate, transform);
            debugCanvas.SetActive(true);
            debugCanvas.transform.position = Vector3.zero;
            debugTMP = debugCanvas.GetComponentInChildren<TextMeshProUGUI>();
        }

        private void UpdateDebugCanvases()
        {
            //update position
            UpdateDebugCanvasePos(Hands.Provider.GetHand(Chirality.Right), ref debugCanvasHandRight);
            UpdateDebugCanvasePos(Hands.Provider.GetHand(Chirality.Left), ref debugCanvasHandLeft);

            //update text
            //debugTMPHandRight.text = DebugTextHandData(true);
            //debugTMPHandLeft.text = DebugTextHandData(false);
        }
        private void UpdateDebugCanvasePos(Leap.Hand hand, ref GameObject debugCanvasHand)
        {
            if (hand == null) debugCanvasHand.SetActive(false);
            else
            {
                if (debugCanvasHand.activeSelf == false) debugCanvasHand.SetActive(true);
                Vector3 palmPos = hand.PalmPosition;
                debugCanvasHand.transform.position = palmPos + Vector3.up * 0.15f;
                debugCanvasHand.transform.rotation = Quaternion.LookRotation(cameraEye.TransformDirection(Vector3.forward));
            }
        }
        private string DebugTextHandData(bool right)
        {
            Leap.Hand handRight = Hands.Provider.GetHand(Chirality.Right);
            Leap.Hand handLeft = Hands.Provider.GetHand(Chirality.Left);

            // old lign former lign :
            //QuickMenuMng quickMenuMng = FindObjectOfType<QuickMenuMng>();

            // replace quickMenuMng by LynxMenuMng to fix compilation error :  
            LynxMenuMng quickMenuMng = FindObjectOfType<LynxMenuMng>();

            StringBuilder stringBuilder = new StringBuilder();

            if (right) stringBuilder.Append($"pinchRight : ").Append(handRight.IsPinching()).Append("\n");
            else stringBuilder.Append($"pinchLeft : ").Append(handLeft.IsPinching()).Append("\n");
            stringBuilder.Append("\n");

            //if (right) stringBuilder.Append($"right fist strength : ").Append(handRight.GetFistStrength()).Append("\n");
            //else stringBuilder.Append($"left fist strength : ").Append(handLeft.GetFistStrength()).Append("\n");
            //stringBuilder.Append("\n");

            //if (right) stringBuilder.Append($"right pinch dist : ").Append(quickMenuMng.pinchRightDist).Append("\n");
            //else stringBuilder.Append($"left pinch dist : ").Append(quickMenuMng.pinchLeftDist).Append("\n");
            //stringBuilder.Append("\n");

            if (right) stringBuilder.Append($"right palmToFace : ").Append(Vector3.Dot(handRight.PalmNormal, (Vector3.Normalize(cameraEye.transform.position - handRight.PalmPosition)))).Append("\n");
            else stringBuilder.Append($"left palmToFace : ").Append(Vector3.Dot(handLeft.PalmNormal, (Vector3.Normalize(cameraEye.transform.position - handLeft.PalmPosition)))).Append("\n");
            stringBuilder.Append("\n");

            return stringBuilder.ToString();
        }


        public void DebugTextOverHands(string text)
        {
            //DebugTextOverHand(Hands.Provider.GetHand(Chirality.Right), text);
            //DebugTextOverHand(Hands.Provider.GetHand(Chirality.Left), text);
            //update text
            debugTMPHandRight.text = text;
            debugTMPHandLeft.text = text;
        }
        public void DebugTextOverHand(Leap.Hand hand, string text)
        {

        }

    }
}