//   ==============================================================================
//   | Lynx (2024)                                                                  |
//   |======================================                                        |
//   | UI_GrabPositioner Script                                                     |
//   | Script to add grabbable handle around interface and follow object            |
//   ==============================================================================

using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;


namespace Lynx.UI
{
    public class LynxUIGrabPositioner : MonoBehaviour
    {
        #region INPUTS

        [SerializeField] private RectTransform _Panel;
        public RectTransform Panel
        {
            get { return _Panel; }
            set { }
        }

        [SerializeField] protected float HandleGrabSize = 0.05f;
        [SerializeField] public bool MakeHandleMesh = true;
        [SerializeField] protected Material handleMaterial;

        [SerializeField] public Color handleColor = Color.white;
        [SerializeField] public Color handleGrabbedColor = Color.cyan;

        [SerializeField] public bool followObjectAtStart = true;
        [SerializeField] public Transform objectToFollow;
        [SerializeField] public Vector3 PosOffset;
        [SerializeField] public Vector3 RotOffset;
        [SerializeField] public float followDistanceThreshold = 0.5f;


        private bool _shouldFollow;
        /// <summary>
        /// activate or disable auto follow of the canvas
        /// </summary>
        [HideInInspector]
        public bool shouldFollow
        {
            get { return _shouldFollow; }
            set
            {
                Debug.Log(value);
                if (value && value != _shouldFollow)
                {
                    _shouldFollow = value;
                    StartCoroutine(CheckPanelTargetDistance());
                }
                else
                    _shouldFollow = value;
            }
        }

        #endregion

        #region PRIVATE VARIABLES

        private Vector2 panelSize;
        protected GameObject panelParent;
        protected GameObject[] handles = new GameObject[4];

        protected XRGrabInteractable XRGrab;
        protected Rigidbody Rb;

        protected MeshRenderer mr;
        private MeshFilter mf;

        private bool isGrabbed = false;
        private bool isLerpinToTarget = false;

        #endregion

        #region UNITY API

        protected void Awake()
        {
            UpdatePanelProperty();
            ConstructHandleCollider();

            //Add a parent to the canvas, wich will have all control script for grab
            panelParent = new GameObject(Panel.name + "_ParentController");
            panelParent.transform.position = _Panel.position;
            _Panel.transform.SetParent(panelParent.transform);

            //Setup XRGrabInteractable on panel parent
            XRGrab = panelParent.gameObject.AddComponent<LynxFineGrabInteractable>();
            XRGrab.movementType = XRBaseInteractable.MovementType.VelocityTracking;
            XRGrab.useDynamicAttach = true;
            XRGrab.trackScale = false;
            XRGrab.selectMode = InteractableSelectMode.Multiple;
            XRGrab.snapToColliderVolume = false;

            XRGrab.firstSelectEntered.AddListener(PanelGrabbed);
            XRGrab.lastSelectExited.AddListener(PanelDroped);

            //Setup Rigidbody on panel parent
            Rb = panelParent.GetComponent<Rigidbody>();
            Rb.useGravity = false;
            Rb.isKinematic = true;
            Rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;


            if (MakeHandleMesh)
            {
                //Add a mesh on panelParent that will be around canvas
                mf = panelParent.AddComponent<MeshFilter>();
                mr = panelParent.AddComponent<MeshRenderer>();
                mf.mesh = ConstructHandleMesh(_Panel.transform.rotation);
                mr.material = handleMaterial;
                StartCoroutine(HandleShaderUpdate());
                mr.material.SetColor("_Color", handleColor);
            }

            shouldFollow = followObjectAtStart;

        }

        // draw interaction box in editor to help visualize interaction distance 
        private void OnDrawGizmosSelected()
        {
            if (_Panel == null)
                return;
            Gizmos.matrix = _Panel.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(new Vector3(_Panel.sizeDelta.x/ 2 + HandleGrabSize / 2, 0, 0), new Vector3(HandleGrabSize, _Panel.sizeDelta.y + HandleGrabSize * 2, HandleGrabSize));
            Gizmos.DrawWireCube(new Vector3(-_Panel.sizeDelta.x/ 2 - HandleGrabSize / 2, 0, 0), new Vector3(HandleGrabSize, _Panel.sizeDelta.y + HandleGrabSize * 2, HandleGrabSize));
            Gizmos.DrawWireCube(new Vector3(0, _Panel.sizeDelta.y / 2 + HandleGrabSize / 2, 0), new Vector3(_Panel.sizeDelta.x + HandleGrabSize * 2, HandleGrabSize, HandleGrabSize));
            Gizmos.DrawWireCube(new Vector3(0, -_Panel.sizeDelta.y / 2 - HandleGrabSize / 2, 0), new Vector3(_Panel.sizeDelta.x + HandleGrabSize * 2, HandleGrabSize, HandleGrabSize));
        }

        private void OnValidate()
        {
            if (_Panel == null && this.gameObject.GetComponent<RectTransform>() != null)
                _Panel = this.gameObject.GetComponent<RectTransform>();
            if (objectToFollow == null)
                objectToFollow = Camera.main.transform;
        }

        #endregion

        #region PRIVATE METHODES

        /// <summary>
        /// Generate 4 BoxCollider around the panel for the XRGrabInteractable
        /// </summary>
        protected void ConstructHandleCollider()
        {
            for(int i = 0; i< 4; ++i)
            {
                //destroy current handle to create new one
                if (handles[i] != null)
                    Destroy(handles[i]);

                handles[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                handles[i].transform.parent = _Panel;
                handles[i].transform.localScale = new Vector3(1 / handles[i].transform.lossyScale.x, 1 / handles[i].transform.lossyScale.y, 1 / handles[i].transform.lossyScale.z);
                handles[i].transform.localRotation = Quaternion.identity;
            }

            // set handles size
            handles[0].GetComponent<BoxCollider>().size = new Vector3(HandleGrabSize, panelSize.y + HandleGrabSize * 2, HandleGrabSize);
            handles[1].GetComponent<BoxCollider>().size = new Vector3(HandleGrabSize, panelSize.y + HandleGrabSize * 2, HandleGrabSize);
            handles[2].GetComponent<BoxCollider>().size = new Vector3(panelSize.x + HandleGrabSize * 2, HandleGrabSize, HandleGrabSize);
            handles[3].GetComponent<BoxCollider>().size = new Vector3(panelSize.x + HandleGrabSize * 2, HandleGrabSize, HandleGrabSize);

            // set handles position 
            handles[0].transform.localPosition = new Vector3(panelSize.x / 2 + HandleGrabSize/2, 0, 0);
            handles[1].transform.localPosition = new Vector3(-panelSize.x / 2 - HandleGrabSize/2, 0, 0);
            handles[2].transform.localPosition = new Vector3(0, panelSize.y / 2 + HandleGrabSize/2, 0);
            handles[3].transform.localPosition = new Vector3(0, -panelSize.y / 2 - HandleGrabSize/2, 0);

            //add needed component to handle
            for (int i = 0; i < 4; ++i)
            {
                Destroy(handles[i].GetComponent<MeshRenderer>());
                Destroy(handles[i].GetComponent<MeshFilter>());
            }

        }

        /// <summary>
        /// Disable auto follow of the panel <br />
        /// Set Handle color for grabbed state <br />
        /// Lock rotation to prevent rigidbody "force leak"
        /// </summary>
        /// <param name="arg"></param>
        private void PanelGrabbed(SelectEnterEventArgs arg)
        {
            isGrabbed = true;
            shouldFollow = false;
            StartCoroutine(PanelRotationLock());
            mr.material.SetColor("_Color", handleGrabbedColor);
        }

        /// <summary>
        /// Set shader color for idle state
        /// </summary>
        /// <param name="arg"></param>
        private void PanelDroped(SelectExitEventArgs arg)
        {
            isGrabbed = false;
            mr.material.SetColor("_Color", handleColor);
        }

        /// <summary>
        /// Generate mesh data for an outline around the canvas
        /// </summary>
        /// <param name="rotation"> current world rotation of the canvas in world space </param>
        /// <returns> handle mesh data </returns>
        protected Mesh ConstructHandleMesh(Quaternion rotation)
        {

            Vector3 TR = new Vector3(panelSize.x / 2, panelSize.y / 2, 0);
            Vector3 DR = new Vector3(panelSize.x / 2, -panelSize.y / 2, 0);
            Vector3 TL = new Vector3(-panelSize.x / 2, panelSize.y / 2, 0);
            Vector3 DL = new Vector3(-panelSize.x / 2, -panelSize.y / 2, 0);
            float S = HandleGrabSize;

            Mesh mesh = new Mesh
            {
                name = "HandlesIndicator"
            };

            // Vertex position :
            // 2 3      14 15
            // 0 1      12 13
            //
            //
            // 4 5      8  9
            // 6 7      10 11

            mesh.vertices = new Vector3[]
            {
                rotation *(TL + new Vector3(-S,0,0)),   // 0
                rotation *(TL + new Vector3(0,0,0)),    // 1
                rotation *(TL + new Vector3(-S,S,0)),   // 2
                rotation *(TL + new Vector3(0,S,0)),    // 3

                rotation *(DL + new Vector3(-S,0,0)),   // 4
                rotation *(DL + new Vector3(0,0,0)),    // 5
                rotation *(DL + new Vector3(-S,-S,0)),  // 6
                rotation *(DL + new Vector3(0,-S,0)),   // 7

                rotation *(DR + new Vector3(0,0,0)),    // 8
                rotation *(DR + new Vector3(S,0,0)),    // 9
                rotation *(DR + new Vector3(0,-S,0)),   // 10
                rotation *(DR + new Vector3(S,-S,0)),   // 11

                rotation *(TR + new Vector3(0,0,0)),    // 12
                rotation *(TR + new Vector3(S,0,0)),    // 13
                rotation *(TR + new Vector3(0,S,0)),    // 14
                rotation *(TR + new Vector3(S,S,0)),    // 15

            };

            mesh.triangles = new int[]
            {
                2,1,0,2,3,1,
                0,5,4,0,1,5,
                4,5,6,6,5,7,
                5,8,7,7,8,10,
                8,9,10,9,11,10,
                12,13,8,8,13,9,
                14,15,12,12,15,13,
                3,14,1,1,14,12
            };

            mesh.uv = new Vector2[]
            {
                new Vector2(0f      ,0.5f),     // 0
                new Vector2(0.5f    ,0.5f),     // 1
                new Vector2(0f      ,1f),       // 2
                new Vector2(0.5f    ,1f),       // 3
                new Vector2(0f      ,0.5f),     // 4
                new Vector2(0.5f    ,0.5f),     // 5
                new Vector2(0f      ,0f),       // 6
                new Vector2(0.5f    ,0f),       // 7
                new Vector2(0.5f    ,0.5f),     // 8
                new Vector2(1f      ,0.5f),     // 9
                new Vector2(0.5f    ,0f),       // 10
                new Vector2(1f      ,0f),       // 11
                new Vector2(0.5f    ,0.5f),     // 12
                new Vector2(1f      ,0.5f),     // 13
                new Vector2(0.5f    ,1f),       // 14
                new Vector2(1f      ,1f)        // 15
            };
            return mesh;
        }

        #endregion

        #region PUBLIC METHODES

        /// <summary>
        /// Update panel property for constructor
        /// </summary>
        public void UpdatePanelProperty()
        {
            panelSize = _Panel.sizeDelta * _Panel.lossyScale;
        }

        /// <summary>
        /// Force panel to recenter
        /// </summary>
        public void ForceRecenter()
        {
            StartCoroutine(FollowObject());
        }

        #endregion

        #region COROUTINE
        /// <summary>
        /// Lock Panel X & Z axis while grabbed to prevent rigidbody force leak
        /// </summary>
        /// <returns></returns>
        IEnumerator PanelRotationLock()
        {
            yield return new WaitForFixedUpdate();
            while (isGrabbed)
            {
                Vector3 eulerAngle = panelParent.transform.rotation.eulerAngles;
                eulerAngle.Scale(Vector3.up);
                panelParent.transform.rotation = Quaternion.Euler(eulerAngle);
                yield return new WaitForFixedUpdate();
            }
        }

        /// <summary>
        /// Set shader property for left and right index & thumbs world space position
        /// </summary>
        /// <returns></returns>
        IEnumerator HandleShaderUpdate()
        {
            while(MakeHandleMesh)
            {
                Vector3 IndexLeftTipPosition = Vector3.positiveInfinity;
                Vector3 IndexRightTipPosition = Vector3.positiveInfinity;
                Vector3 ThumbRightTipPosition = Vector3.positiveInfinity;
                Vector3 ThumbLeftTipPosition = Vector3.positiveInfinity;
                Transform O = Camera.main.transform.parent;

                if (LynxHandtrackingAPI.LeftHand.GetJoint(UnityEngine.XR.Hands.XRHandJointID.IndexTip).TryGetPose(out Pose leftIndexPose))
                {
                    IndexLeftTipPosition = O.position + O.rotation * leftIndexPose.position;
                }

                if (LynxHandtrackingAPI.RightHand.GetJoint(UnityEngine.XR.Hands.XRHandJointID.IndexTip).TryGetPose(out Pose rightIndexPose))
                {
                    IndexRightTipPosition = O.position + O.rotation * rightIndexPose.position;
                }

                if (LynxHandtrackingAPI.RightHand.GetJoint(UnityEngine.XR.Hands.XRHandJointID.IndexTip).TryGetPose(out Pose rightThumbPose))
                {
                    ThumbRightTipPosition = O.position + O.rotation * rightThumbPose.position;
                }

                if (LynxHandtrackingAPI.LeftHand.GetJoint(UnityEngine.XR.Hands.XRHandJointID.IndexTip).TryGetPose(out Pose leftThumbPose))
                {
                    ThumbLeftTipPosition = O.position + O.rotation * leftThumbPose.position;
                }

                mr.material.SetVector("LI", IndexLeftTipPosition);
                mr.material.SetVector("RI", IndexRightTipPosition);
                mr.material.SetVector("LT", ThumbLeftTipPosition);
                mr.material.SetVector("RT", ThumbRightTipPosition);


                yield return new WaitForEndOfFrame();
            }
        }

        /// <summary>
        /// Check if panel distance from camera is above threshold <br />
        /// If distance is above, call follow object coroutine
        /// </summary>
        /// <returns></returns>
        IEnumerator CheckPanelTargetDistance()
        {
            while (shouldFollow)
            {
                float distance = Vector3.Distance(panelParent.transform.position, objectToFollow.position);
                if (distance > PosOffset.magnitude + followDistanceThreshold && !isLerpinToTarget)
                    StartCoroutine(FollowObject());

                yield return new WaitForSecondsRealtime(0.25f);
            }
        }

        /// <summary>
        /// Lerp canvas to targeted position over time
        /// </summary>
        /// <returns></returns>
        IEnumerator FollowObject()
        {
            isLerpinToTarget = true;

            Vector3 velocity = Vector3.zero;
            Quaternion baseRot = panelParent.transform.rotation;
            Vector3 objRotCorrected = objectToFollow.rotation.eulerAngles;
            float t = 0;

            while (Vector3.Distance(panelParent.transform.position, objectToFollow.position + objectToFollow.rotation * PosOffset) > 0.025f || t < 1)
            {
                t += Time.deltaTime;

                panelParent.transform.position = Vector3.SmoothDamp(panelParent.transform.position, objectToFollow.position + objectToFollow.rotation * PosOffset, ref velocity, 0.3f);
                objRotCorrected = objectToFollow.rotation.eulerAngles;
                objRotCorrected.Scale(Vector3.up);
                panelParent.transform.rotation = Quaternion.Slerp(baseRot, Quaternion.Euler(objRotCorrected + RotOffset), t);

                yield return new WaitForEndOfFrame();
            }

            objRotCorrected = objectToFollow.rotation.eulerAngles;
            objRotCorrected.Scale(Vector3.up);
            panelParent.transform.rotation = Quaternion.Euler(objRotCorrected + RotOffset);

            isLerpinToTarget = false;
        }


        #endregion

    }

}