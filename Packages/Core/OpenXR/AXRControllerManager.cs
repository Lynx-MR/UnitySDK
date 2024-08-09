/**
* @file AXRControllerManager.cs
* 
* @author Geoffrey Marhuenda
* 
* @brief    Manage any controller used by the application. Inherited members should set there instance via SetInstance.
*           By priority: Handtracking, Controllers, gaze.
*/
using UnityEngine;

namespace Lynx
{
    public abstract class AXRControllerManager
    {
        // Singleton
        public static AXRControllerManager Instance { get; private set; }

        private EControllerPriority m_currentPriority = EControllerPriority.OTHER;

        /// <summary>
        /// Define the priority of the current instance.
        /// The adapter will take highest controller priority.
        /// </summary>
        /// <param name="priority">Currently, Handtracking should be used if available. You can use TOP_PRIORITY to force the Adapter to use your instance.</param>
        public static void SetInstance(EControllerPriority priority, AXRControllerManager instance)
        {
            if (Instance == null || priority <= Instance.m_currentPriority)
            {
                Instance = instance;
                Instance.m_currentPriority = priority;
                Debug.Log($"Lynx XR Controller Manager instance: {priority}");
            }
        }

        /// <summary>
        /// Define if the current controller is tracked not depending on handeness.
        /// </summary>
        /// <returns>True if the controller is tracked</returns>
        public abstract bool IsTracked();

        /// <summary>
        /// Define if the current controller with given handedness is tracked.
        /// </summary>
        /// <param name="handedness">Define spcific chirality to track.</param>
        /// <returns>True if the controller with given handedness is tracked</returns>
        public abstract bool IsTracked(EHandedness handedness);

        /// <summary>
        /// Return available pose of the controller.
        /// </summary>
        /// <returns>Absolute position of the tracked controller. Zeros if not tracked.</returns>
        public abstract Vector3 GetPose();

        /// <summary>
        /// Return available pose of the controller for given handedness.
        /// </summary>
        /// <param name="handedness">Define spcific chirality to track.</param>
        /// <returns>Absolute position of the tracked controller. Zeros if not tracked.</r</returns>
        public abstract Vector3 GetPose(EHandedness handedness);

        /// <summary>
        /// Handedness to specify which chirality to use.
        /// </summary>
        public enum EHandedness
        {
            Left,
            Right
        }

        /// <summary>
        /// Priorities for instance.
        /// The adapter will use the controller with the lowest priority.
        /// </summary>
        public enum EControllerPriority
        {
            TOP_PRIORITY = 0,
            HANDTRACKING,
            CONTROLLER,
            GAZE,
            OTHER
        }
    }
}