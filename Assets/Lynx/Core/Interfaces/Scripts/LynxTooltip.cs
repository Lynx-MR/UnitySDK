/**
 * @file LynxTooltip.cs
 *
 * @author Geoffrey Marhuenda
 *
 * @brief Basic tooltip behaviour to draw a line from the tooltip to the target.
 */
using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lynx.UI
{
    public class LynxTooltip : MonoBehaviour
    {
        [SerializeField] public Transform attachPoint = null;
        [SerializeField] public Vector3 AttachOffset = Vector3.zero;
        [SerializeField] public Vector3 BaseOffset = new Vector3(0.0f, -0.025f, 0.0f);
        [SerializeField] public bool IsStartingVisible = false;

        [SerializeField] private LineRenderer lineRenderer;

        #region UNITY API
        public void Start()
        {
            InitTooltip();
        }

        private void OnEnable()
        {
            StartCoroutine(UpdateLineRendering());
        }

        private void OnDisable()
        {
            StopCoroutine(UpdateLineRendering());
        }
        #endregion


        /// <summary>
        /// Use to initialize visibility of the tooltip once created.
        /// </summary>
        public void InitTooltip()
        {
            this.gameObject.SetActive(IsStartingVisible);
        }

        /// <summary>
        /// Draw a line between the tooltip and the attach point.
        /// </summary>
        void PlaceLineRendering()
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, lineRenderer.transform.position + lineRenderer.transform.rotation * BaseOffset);
            lineRenderer.SetPosition(1, attachPoint.position + attachPoint.rotation * AttachOffset);
        }

        /// <summary>
        /// Coroutine to draw the line between the tooltip and the attach point.
        /// Called once this gameobject is enabled.
        /// </summary>
        public IEnumerator UpdateLineRendering()
        {
            while (this.gameObject.activeSelf)
            {
                PlaceLineRendering();
                yield return new WaitForEndOfFrame();
            }
        }

        /// <summary>
        /// Enable or disable visibility of the tooltip depending on previous state.
        /// </summary>
        public void ToggleVisibility()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }



#if UNITY_EDITOR
        [CustomEditor(typeof(LynxTooltip))]
        public class LynxTooltipEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                LynxTooltip script = (LynxTooltip)target;
                if(script.attachPoint != null && script.lineRenderer != null)
                    script.PlaceLineRendering();

            }
        }
#endif
    }
}