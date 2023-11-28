using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Lynx.UI
{
    public class LynxPointerHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {

        public UnityEvent onPointerEnter = null;
        public UnityEvent onPointerExit = null;

        public void OnPointerEnter(PointerEventData eventData)
        {
            onPointerEnter?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onPointerExit?.Invoke();
        }
    }
}