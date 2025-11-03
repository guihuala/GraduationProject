using UnityEngine;

namespace GuiFramework.Utils
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;

        public static T Instance
        {
            get { return instance; }
            set { instance = value; }
        }

        protected virtual void Awake()
        {
            instance = this as T;
        }
    }
}