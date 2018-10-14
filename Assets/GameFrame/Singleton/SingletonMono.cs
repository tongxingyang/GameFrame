﻿using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GameFrame
{
    public abstract class SingletonMono<T> : MonoBehaviour ,HandleMessage where T : SingletonMono<T>
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
            if (m_instance != null)
            {
                UnInit();
                m_instance = null;
            }
        }
        public virtual void Init()
        {

        }

        public virtual void UnInit()
        {

        }
        /// <summary>
        /// 调用一些非公开的方法
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="args"></param>
        public void HandleMessage(string msg, object[] args)
        {
            var mi = this.GetType()
                .GetMethod(msg, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (mi != null)
            {
                mi.Invoke(this, BindingFlags.NonPublic, null, args, null);
            }
        }

        public object HandleMessageRetValue(string msg, object[] args)
        {
            var mi = this.GetType()
                .GetMethod(msg, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (mi != null)
            {
                return mi.Invoke(this, BindingFlags.NonPublic, null, args, null);
            }
            return null;
        }
    }
}
