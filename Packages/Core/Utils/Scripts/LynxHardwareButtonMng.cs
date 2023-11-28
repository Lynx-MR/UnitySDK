using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Lynx
{
    public class LynxHardwareButtonMng : MonoBehaviour
    {
        //INSPECTOR
        [Header("Delay Settings")]
        [SerializeField] private float clicksCountingDelay = 0.3f;
        [SerializeField] private float longClickDuration = 1.0f;
        [Header("Events")]
        public UnityEvent OnSingleClick;
        public UnityEvent OnDoubleClick;
        public UnityEvent OnTripleClick;
        public UnityEvent OnLongClick;

        public KeyCode debugInput;

        [HideInInspector] public static KeyCode LYNX_BUTTON = KeyCode.RightCommand;

        //PRIVATE
        private int clickCount = 0;
        private bool longClickActive = false;
        private bool clickProcessed = false;

        private Coroutine clickCounterCoroutine;
        private Coroutine longClickCoroutine;

        private void Start()
        {
            // Use old keycode only for OS < 1.1.8
            int compareVersionResult = AndroidVersionChecker.CompareVersion("v1.1.8");
            if (compareVersionResult == 1) // 1.1.8 > OS version --> Old keycode
            {
                LYNX_BUTTON = KeyCode.JoystickButton0;
                Debug.Log("Use OLD keycode");
            }
            else // OS version >= 1.1.8 --> New Keycode
                Debug.Log("Use NEW keycode");
        }


        private void Update()
        {
            ProcessClicks();
        }

        private void ProcessClicks()
        {
            if (GetClickDown())
            {
                clickCount++;
                clickProcessed = false;
                if (clickCounterCoroutine == null) clickCounterCoroutine = StartCoroutine(ClickCounter());
            }
            else if (GetClick() && !longClickActive && !clickProcessed)
            {
                if (longClickCoroutine == null) longClickCoroutine = StartCoroutine(LongClick());
            }
            else if (GetClickUp())
            {
                longClickActive = false;
            }
        }

        private IEnumerator ClickCounter()
        {
            yield return new WaitForSeconds(clicksCountingDelay);

            if (!longClickActive && !clickProcessed)
            {
                if (clickCount == 1) OnSingleClick.Invoke();
                else if (clickCount == 2) OnDoubleClick.Invoke();
                else if (clickCount >= 3) OnTripleClick.Invoke();
            }
            clickProcessed = true;
            clickCount = 0;

            clickCounterCoroutine = null;
        }
        private IEnumerator LongClick()
        {
            float duration = 0f;
            longClickActive = true;

            while (GetClick() && longClickActive)
            {
                duration += Time.deltaTime;

                if (duration >= longClickDuration)
                {
                    OnLongClick.Invoke();
                    longClickActive = false;
                    clickCount = 0;
                }

                yield return null;
            }

            longClickCoroutine = null;
        }

        private bool GetClick()
        {
#if UNITY_EDITOR
            return (Input.GetKey(debugInput));
#else
        return (Input.GetKey(LYNX_BUTTON));
#endif
        }
        private bool GetClickDown()
        {
#if UNITY_EDITOR
            return (Input.GetKeyDown(debugInput));
#else
        return (Input.GetKeyDown(LYNX_BUTTON));
#endif
        }
        private bool GetClickUp()
        {
#if UNITY_EDITOR
            return (Input.GetKeyUp(debugInput));
#else
        return (Input.GetKeyUp(LYNX_BUTTON));
#endif
        }


        //test methods
        public void LogSimpleClick()
        {
            Debug.Log("SimpleClick");
        }
        public void LogDoubleClick()
        {
            Debug.Log("DoubleClick");
        }
        public void LogTripleClick()
        {
            Debug.Log("TripleClick");
        }
        public void LogLongClick()
        {
            Debug.Log("LongClick");
        }

    }
}