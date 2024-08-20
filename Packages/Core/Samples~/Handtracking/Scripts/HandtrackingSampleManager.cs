/**
 * @file HandtrackingSampleManager.cs
 * 
 * @author Geoffrey Marhuenda
 * 
 * @brief Manage handtracking sample features.
 */

using UnityEngine;

namespace Lynx
{
    public class HandtrackingSampleManager : MonoBehaviour
    {
        public void ToggleARVR()
        {
            LynxAPI.ToggleAR();
        }
    }
}