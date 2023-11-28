using UnityEngine;
using UnityEngine.Events;

namespace Lynx
{
    public class SimulateClickOnPhone : MonoBehaviour
    {
        public UnityEvent OnClick = new UnityEvent();

        void OnMouseDown()
        {
            //if (!EventSystem.current.IsPointerOverGameObject()) // cedric : one way to prevent raycast if a GUI element (canvas) is front of this 3D object. 
            //{
            //Debug.Log("OnMouseDown on this gameobject : " + gameObject.name);
            OnClick.Invoke();
            //}
        }

    }
}