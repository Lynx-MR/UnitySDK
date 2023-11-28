using UnityEngine;

namespace Lynx
{
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        [SerializeField] private bool DestroyOnLoad = true;

        private static T instance;

        public static T Instance
        {
            get
            {
                if (!IsInitialized && searchForInstance)
                {
                    searchForInstance = false;
                    T[] objects = FindObjectsOfType<T>();
                    if (objects.Length == 1)
                    {
                        instance = objects[0];
                    }
                    else if (objects.Length > 1)
                    {
                        //Debug.LogErrorFormat("Expected exactly 1 {0} but found {1}.", typeof(T).Name, objects.Length);
                    }
                    else
                    {
                        instance = new GameObject().AddComponent<T>();
                        instance.DestroyOnLoad = true;
                        instance.transform.name = instance.GetType().ToString();
                    }
                }
                return instance;
            }
        }

        private static bool searchForInstance = true;

        public static void AssertIsInitialized()
        {
            Debug.Assert(IsInitialized, string.Format("The {0} singleton has not been initialized.", typeof(T).Name));
        }

        public static bool IsInitialized
        {
            get
            {
                return instance != null;
            }
        }

        protected virtual void Awake()
        {
            if (IsInitialized && instance != this)
            {
                if (Application.isEditor)
                {
                    DestroyImmediate(gameObject);
                }
                else
                {
                    Destroy(gameObject);
                }

                //Debug.LogFormat("Trying to instantiate a second instance of singleton class {0}. Additional Instance was destroyed", GetType().Name);
            }
            else if (!IsInitialized)
            {
                instance = (T)this;
                searchForInstance = false;

                if (!DestroyOnLoad && transform.parent == null)
                    DontDestroyOnLoad(this.gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
                searchForInstance = true;
            }
        }
    }
}