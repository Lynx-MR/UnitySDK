using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace Lynx.UI
{
    public class InterfaceGrabController : XRSimpleInteractable
    {
        #region PROPERTIES

        public float SelectExtendeDistance = 0;

        protected bool isGrabbed = false;
        protected bool dualGrab = false;

        IXRSelectInteractor mainInteractor;
        IXRSelectInteractor secondInteractor;
        protected Transform defaultParent;
        protected Transform grabParent;

        #endregion

        #region FILTER VARIABLES

        private const float DelayReset = 0.1f;

        private bool selectionIsValid = false;
        private bool firstFrameSelect = true;
        private float unselectDelay = 0.1f;

        #endregion

        /// <summary>
        /// Bind interactor event and set needed default parameters
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            grabParent = new GameObject().transform;
            defaultParent = this.transform.parent;

            firstSelectEntered.AddListener(StartSelect);
            selectEntered.AddListener(SelectEnter);
            selectExited.AddListener(SelectExit);
            lastSelectExited.AddListener(EndSelect);

            selectMode = InteractableSelectMode.Multiple;

            SetupFilters();
        }

        #region SELECT FILTER
        void SetupFilters()
        {
            var selectFilterDelegate = new XRSelectFilterDelegate((interactor, interactable) =>
            {
                if (firstFrameSelect)
                {
                    firstFrameSelect = false;
                    selectionIsValid = CheckInteractorDistance(interactor, interactable);
                    unselectDelay = DelayReset;
                    StartCoroutine(IsSelectedChecker());
                }

                unselectDelay = DelayReset;

                return selectionIsValid;
            });

            selectFilters.Add(selectFilterDelegate);
        }

        /// <summary>
        /// Check if interactor is directly inside interactable collider
        /// </summary>
        /// <param name="interactor"></param>
        /// <param name="interactable"></param>
        /// <remarks>
        /// Work only on convex collider
        ///</remarks>
        /// <returns></returns>
        private bool CheckInteractorDistance(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        {
            if (XRInteractableUtility.TryGetClosestPointOnCollider(interactable, interactor.transform.position, out DistanceInfo info))
            {
                float sqrP = (interactor.transform.position - info.collider.bounds.center).sqrMagnitude;
                float sqrC = (info.point - info.collider.bounds.center).sqrMagnitude;

                if (sqrP <= sqrC + (SelectExtendeDistance* SelectExtendeDistance))
                    return true;
                else
                    return false;
            }
            return false;
        }

        /// <summary>
        /// Used to check if SelectFilterDelegate is still called every frame
        /// </summary>
        /// <returns></returns>
        IEnumerator IsSelectedChecker()
        {
            while (unselectDelay > 0)
            {
                unselectDelay -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            firstFrameSelect = true;
        }

        #endregion

        #region SELECTION EVENTS

        /// <summary>
        /// Called on first grab to init and start grab coroutine
        /// </summary>
        /// <param name="arg"></param>
        private void StartSelect(SelectEnterEventArgs arg)
        {
            mainInteractor = arg.interactorObject;
            isGrabbed = true;
            InitGrab();

            StartCoroutine(ManageGrabMovement());
        }

        /// <summary>
        /// Check if the second hand has gabbed, if yes, set to dual grab control
        /// </summary>
        /// <param name="arg"></param>
        private void SelectEnter(SelectEnterEventArgs arg)
        {
            if (arg.interactorObject == mainInteractor)
                return;

            secondInteractor = arg.interactorObject;
            dualGrab = true;
            InitDualGrab();
        }

        /// <summary>
        /// Check wich hand stoped grabbing, and switch interactor if needed
        /// </summary>
        /// <param name="arg"></param>
        private void SelectExit(SelectExitEventArgs arg)
        {
            if (arg.interactorObject == mainInteractor && secondInteractor != null)
            {
                mainInteractor = secondInteractor;
                secondInteractor = null;
                dualGrab = false;
                InitGrab();
            }
            if (arg.interactorObject == secondInteractor)
            {
                secondInteractor = null;
                dualGrab = false;
                InitGrab();
            }
        }

        /// <summary>
        /// Stop coroutine
        /// </summary>
        /// <param name="arg"></param>
        private void EndSelect(SelectExitEventArgs arg)
        {
            mainInteractor = null;
            isGrabbed = false;
        }

        #endregion

        #region GRAB BEHAVIOR

        /// <summary>
        /// Called on first grab and change of main interactor<br />
        /// Set main interactor as parent
        /// </summary>
        protected virtual void InitGrab()
        {
            this.transform.parent = null;
            grabParent.transform.position = mainInteractor.transform.position;
            grabParent.transform.rotation = Quaternion.identity;
            grabParent.transform.parent = mainInteractor.transform;
            this.transform.SetParent(grabParent, true);
        }

        /// <summary>
        /// Set dualGrabParent to main interactor position, and rotate it to be ready for dual grab control 
        /// </summary>
        protected virtual void InitDualGrab()
        {
            this.transform.parent = null;
            grabParent.SetPositionAndRotation(mainInteractor.transform.position, Quaternion.identity);

            Vector3 lookVec = secondInteractor.transform.position - mainInteractor.transform.position;
            lookVec.Scale(new Vector3(1, 0, 1));

            grabParent.rotation *= Quaternion.FromToRotation(grabParent.forward, lookVec);

            this.transform.parent = grabParent;

        }

        /// <summary>
        /// Methode called at the start of the grab, after init
        /// </summary>
        protected virtual void StartGrabBehavior() { }

        /// <summary>
        /// Methodes called at every frame when grabbed<br />
        /// Force object rotation to be only on Y axis
        /// </summary>
        protected virtual void DefaultGrabBehavior()
        {
            grabParent.eulerAngles = new Vector3(0, grabParent.eulerAngles.y, 0);
        }

        /// <summary>
        /// Methodes called at every frame when dual grabbed, after defaultGrabBehavior<br />
        /// Rotate dualGrabParent to look at second interactor, only on Y axis
        /// </summary>
        protected virtual void DualGrabBehavior()
        {
            grabParent.position = mainInteractor.transform.position;

            Vector3 lookVec = secondInteractor.transform.position - mainInteractor.transform.position;
            lookVec.Scale(new Vector3(1, 0, 1));

            grabParent.rotation *= Quaternion.FromToRotation(grabParent.forward, lookVec);
        }

        /// <summary>
        /// Methodes called at every frame when dual grabbed, after defaultGrabBehavior<br />
        /// Check rotation and set default parent back
        /// </summary>
        protected virtual void EndGrabBehavior()
        {
            grabParent.eulerAngles = new Vector3(0, grabParent.eulerAngles.y, 0);
            this.transform.eulerAngles = new Vector3(0, this.transform.eulerAngles.y, 0);
            this.transform.parent = defaultParent;
        }

        #endregion

        /// <summary>
        /// Coroutine called when object is grabbed, handle behaviors of grab movements
        /// </summary>
        /// <returns></returns>
        IEnumerator ManageGrabMovement()
        {
            StartGrabBehavior();
            while (isGrabbed)
            {
                DefaultGrabBehavior();

                if (dualGrab)
                {
                    DualGrabBehavior();
                }
                yield return null;
            }
            EndGrabBehavior();
        }
    }
}

