
using System;
using System.Collections;
using GameFrameDebuger;
using UnityEngine;

namespace GameFrame.Common
{
    /// <summary>
    /// 实现每帧调用的帮助类
    /// </summary>
    public delegate void MonoUpdaterEvent();
    
    public class MonoHelper:SingletonMono<MonoHelper>
    {
        private event MonoUpdaterEvent UpdateEvent = null;
        private event MonoUpdaterEvent FixedUpdateEvent = null;
        private event MonoUpdaterEvent LaterUpdateEvent = null;

        public void AddUpdateListener(MonoUpdaterEvent listener)
        {
             UpdateEvent += listener;
        }

        public void RemoveUpdateListener(MonoUpdaterEvent listener)
        {
            UpdateEvent -= listener;
        }
        
        public void AddLaterUpdateListener(MonoUpdaterEvent listener)
        {
            LaterUpdateEvent += listener;
        }

        public void RemoveLaterUpdateListener(MonoUpdaterEvent listener)
        {
            LaterUpdateEvent -= listener;
        }


        public void AddFixedUpdateListener(MonoUpdaterEvent listener)
        {
            FixedUpdateEvent += listener;
        }

        public void RemoveFixedUpdateListener(MonoUpdaterEvent listener)
        {
            FixedUpdateEvent -= listener;
        }

        public void OnUpdate()
        {
            if (UpdateEvent != null)
            {
                try
                {
                    UpdateEvent();
                }
                catch (Exception e)
                {
                    Debuger.LogError("MonoHelper", "Update() Error:{0}\n{1}", e.Message, e.StackTrace);
                }
            }
        }

        public void OnFixedUpdate()
        {
            if (FixedUpdateEvent != null)
            {
                try
                {
                    FixedUpdateEvent();
                }
                catch (Exception e)
                {
                    Debuger.LogError("MonoHelper", "FixedUpdate() Error:{0}\n{1}", e.Message, e.StackTrace);
                }
            }
        }
        public void OnLaterUpdate()
        {
            if (LaterUpdateEvent != null)
            {
                try
                {
                    LaterUpdateEvent();
                }
                catch (Exception e)
                {
                    Debuger.LogError("MonoHelper", "LaterUpdate() Error:{0}\n{1}", e.Message, e.StackTrace);
                }
            }
        }

    }
}