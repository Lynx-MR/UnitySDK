using System;
using TMPro;
using UnityEngine;

namespace Lynx
{
    /* Displays current time on text UI */
    public class DisplayClock : MonoBehaviour
    {
        private TextMeshProUGUI uiText;


        private void Start()
        {
            uiText = GetComponent<TextMeshProUGUI>();
        }
        private void Update()
        {
            //uiText.text = DateTime.Now.ToString("t").ToUpper();

            DateTime now = DateTime.Now;
            string formattedTime = now.ToString("HH:mm");
            uiText.text = formattedTime;
        }

    }
}