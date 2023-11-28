using TMPro;
using UnityEngine;

namespace Lynx
{
    public class DisplayAppVersion : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI versionText;


        private void Start()
        {
            versionText.text = "ver : " + Application.version;
        }

    }
}