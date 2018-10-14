using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GameFrameDebuger;
using UnityEngine;

namespace GameFrame
{
    /// <summary>
    /// 普通单例类
    /// </summary>
    public abstract class Singleton<T> : HandleMessage where T : class, new() 
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
                m_instance = Activator.CreateInstance<T>();
                if (m_instance != null)
                {
                    (m_instance as Singleton<T>).Init();
                }
                else
                {
                    Debuger.LogError("创建对象出错 Type ："+typeof(T));
                }
            }
        }

        public static void DestoryInstance()
        {
            if (m_instance != null)
            {
                (m_instance as Singleton<T>).UnInit();
                m_instance = null;
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
        /// <summary>
        /// 初始化
        /// </summary>
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
