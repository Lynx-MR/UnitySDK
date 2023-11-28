/**
 * @file DisableInEditor.cs
 *
 * @author Geoffrey Marhuenda
 *
 * @brief Disable attached GameObject to avoid it to run in Editor (ex: object supported only on android).
 */
using UnityEngine;

namespace Lynx
{
    public class DisableInEditor : MonoBehaviour
    {
#if UNITY_EDITOR
        void Awake()
        {
            this.gameObject.SetActive(false);
        }
#endif
    }
}