using UnityEngine;

namespace BeamableExample.Helpers
{
    public class SingletonPersistant<T> : MonoBehaviour where T : Component
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                return _instance;
            }
            set
            {
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        public virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(this);
            }
        }
    }

    public class SingletonNonPersistant<T> : MonoBehaviour where T : Component
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                return _instance;
            }
            set
            {
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        public virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
            }
            else
            {
                Destroy(this);
            }
        }
    }

    public class Singleton<T> : MonoBehaviour where T : Component
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(T).Name;
                    _instance = obj.AddComponent<T>();
                }
                return _instance;
            }
            set
            {
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}
