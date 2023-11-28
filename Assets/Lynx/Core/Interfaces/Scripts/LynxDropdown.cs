//   ==============================================================================
//   | Lynx Interfaces (2023)                                                     |
//   |======================================                                      |
//   | LynxDropdown Script                                                        |
//   | Script to set a UI element as Dropdown.                                    |
//   ==============================================================================
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Lynx.UI
{
    public class LynxDropdown : TMP_Dropdown
    {
        #region INSPECTOR VARIABLES
        [SerializeField] private bool m_disableSelectState = true;
        [SerializeField] private Graphic[] m_secondaryTargetGraphic;
        #endregion



        protected override void OnEnable()
        {
            base.OnEnable();

            PointerEventData eventData = new PointerEventData(EventSystem.current);
            //base.OnPointerDown(eventData);
            base.OnDeselect(eventData);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            // State Select can affect the expected behaviour of the button.
            // It is natively deactivated on our buttons.
            // But can be reactivated by unchecking disableSelectState

            if (m_disableSelectState)
            {
                base.OnDeselect(eventData);
            }
            else
            {
                base.OnSelect(eventData);
            }
        }

        //DoStateTranistion is called on every state change to manage graphic modification
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            if (transition != Transition.ColorTint) return;

            for (int i = 0; i < m_secondaryTargetGraphic.Length; ++i)
            {
                Color tintColor;
                switch (state)
                {
                    case SelectionState.Normal:
                        tintColor = colors.normalColor;
                        break;
                    case SelectionState.Highlighted:
                        tintColor = colors.highlightedColor;
                        break;
                    case SelectionState.Pressed:
                        tintColor = colors.pressedColor;
                        break;
                    case SelectionState.Selected:
                        tintColor = colors.selectedColor;
                        break;
                    case SelectionState.Disabled:
                        tintColor = colors.disabledColor;
                        break;
                    default:
                        tintColor = Color.black;
                        break;
                }
                m_secondaryTargetGraphic[i].CrossFadeColor(tintColor, instant ? 0f : colors.fadeDuration, true, true);
            }
        }
    }

}
