//   ==============================================================================================
//   | Lynx (2024)                                                                                  |
//   |======================================                                                        |
//   | LynxFineGrabInteractable Script                                                              |
//   | XRGrabInteractable that check that the interactor is inside the collider before grabbing it  |
//   ===============================================================================================


using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Utilities;


namespace Lynx.UI
{
    public class LynxFineGrabInteractable : XRGrabInteractable
    {
        private const float DelayReset = 0.1f;
        
        private bool selectionIsValid = false;
        private bool firstFrameSelect = true;
        private float unselectDelay = 0.1f;


        // Add custom Filters to XRGRabInteractable on awake
        protected override void Awake()
        {
            base.Awake();
            SetupFilters();
        }

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
        protected bool CheckInteractorDistance(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        {
            if (XRInteractableUtility.TryGetClosestPointOnCollider(interactable, interactor.transform.position, out DistanceInfo info))
            {
                float sqrP = (interactor.transform.position - info.collider.bounds.center).sqrMagnitude;
                float sqrC = (info.point - info.collider.bounds.center).sqrMagnitude;
                if (sqrP <= sqrC)
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
            while(unselectDelay > 0)
            {
                unselectDelay -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            firstFrameSelect = true;
        }
    }
}