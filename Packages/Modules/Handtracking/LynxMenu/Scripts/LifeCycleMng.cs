using UnityEngine;


namespace Lynx
{
    public class LifeCycleMng : MonoBehaviour
    {
        //[SerializeField] QuickMenuMng    QuickMenuMng;
        //[SerializeField] MiniLauncherMng MiniLauncherMng;
        [SerializeField] float LifePeriod = 20.0f;

        bool panelsVisible = false;
        float timer = 0.0f;

        // Start is called before the first frame update
        void Start()
        {
            panelsVisible = false;

            //MiniLauncherEventMng.Instance.miniLauncherOpened.AddListener(MiniLauncherOpened);
            //MiniLauncherEventMng.Instance.miniLauncherClosed.AddListener(MiniLauncherClosed);
        }

        // Update is called once per frame
        void Update()
        {
            if (!panelsVisible) return;

            timer += Time.deltaTime;

            if (timer >= LifePeriod)
            {
                Debug.Log(" Mini Launcher end of cycle **********");
                timer = 0;

                //QuickMenuMng.DeactivateQuickMenu();
                //MiniLauncherMng.DeactivateMiniLauncher();
                panelsVisible = false;
            }
        }

        void MiniLauncherOpened()
        {
            panelsVisible = true;
            timer = 0.0f;
        }

        void MiniLauncherClosed()
        {
            panelsVisible = false;
            timer = 0.0f;
        }
    }
}