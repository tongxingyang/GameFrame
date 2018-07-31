using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
    public abstract class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
    {

        private static T m_instance;

        public static T Instance
        {
            get
            {
                if (m_instance == null)
                {
                    CreateInstance();
                }
                return m_instance;
            }
        }

        private static void CreateInstance()
        {
            if (m_instance == null)
            {
                m_instance = GameObject.FindObjectOfType<T>();
                if (m_instance == null)
                {
                    m_instance = new GameObject(typeof(T).ToString()).AddComponent<T>();
                    m_instance.Init();
                    //
                    DontDestroyOnLoad(m_instance);
                }
            }
        }

        public static T GetInstance()
        {
            if (m_instance == null)
            {
                CreateInstance();
            }
            return m_instance;
        }

        public virtual void OnDestory()
        {
            UnInit();
        }
        public virtual void Init()
        {

        }

        public virtual void UnInit()
        {

        }
    }
}
