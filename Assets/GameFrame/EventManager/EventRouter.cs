using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFrame
{
    public class EventRouter : Singleton<EventRouter>
    {
        private Dictionary<string, Delegate> m_eventMap = null;

        public override void Init()
        {
            base.Init();
            m_eventMap = new Dictionary<string, Delegate>();
        }

        public void ClearAllEvents()
        {
            m_eventMap.Clear();
        }

        private bool OnBroadCasting(string eventType)
        {
            bool result = m_eventMap.ContainsKey(eventType);
            return result;
        }

        private bool OnHandlerRemoving(string eventType, Delegate handler)
        {
            bool result = true;
            if (OnBroadCasting(eventType))
            {
                Delegate @delegate = m_eventMap[eventType];
                if (@delegate != null)
                {
                    if (@delegate.GetType() != handler.GetType())
                    {
                        result = false;
                    }
                }
                else
                {
                    m_eventMap.Remove(eventType);
                    result = false;
                }
            }
            else
            {
                result = false;
            }
            return result;
        }

        private bool OnHandlerAdding(string eventType, Delegate handler)
        {
            bool result = true;
            if (OnBroadCasting(eventType) == false)
            {
                m_eventMap.Add(eventType, null);
            }
            Delegate @delegate = m_eventMap[eventType];
            if (@delegate != null && handler.GetType() != @delegate.GetType())
            {
                result = false;
            }
            return result;
        }

        public void AddEventHandler(string eventType, Action handler)
        {
            if (OnHandlerAdding(eventType, handler))
            {
                m_eventMap[eventType] = (Action)Delegate.Combine((Action)m_eventMap[eventType], handler);
            }
        }
        public void AddEventHandler<T>(string eventType, Action<T> handler)
        {
            if (OnHandlerAdding(eventType, handler))
            {
                m_eventMap[eventType] = (Action<T>)Delegate.Combine((Action<T>)m_eventMap[eventType], handler);
            }
        }
        public void AddEventHandler<T1, T2>(string eventType, Action<T1, T2> handler)
        {
            if (OnHandlerAdding(eventType, handler))
            {
                m_eventMap[eventType] = (Action<T1, T2>)Delegate.Combine((Action<T1, T2>)m_eventMap[eventType], handler);
            }
        }
        public void AddEventHandler<T1, T2, T3>(string eventType, Action<T1, T2, T3> handler)
        {
            if (OnHandlerAdding(eventType, handler))
            {
                m_eventMap[eventType] = (Action<T1, T2, T3>)Delegate.Combine((Action<T1, T2, T3>)m_eventMap[eventType], handler);
            }
        }
        public void AddEventHandler<T1, T2, T3, T4>(string eventType, Action<T1, T2, T3, T4> handler)
        {
            if (OnHandlerAdding(eventType, handler))
            {
                m_eventMap[eventType] = (Action<T1, T2, T3, T4>)Delegate.Combine((Action<T1, T2, T3, T4>)m_eventMap[eventType], handler);
            }
        }

        public void RemoveEventHandler(string eventType, Action handler)
        {
            if (OnHandlerRemoving(eventType, handler))
            {
                m_eventMap[eventType] = (Action)Delegate.Remove((Action)m_eventMap[eventType], handler);
            }
        }
        public void RemoveEventHandler<T1>(string eventType, Action<T1> handler)
        {
            if (OnHandlerRemoving(eventType, handler))
            {
                m_eventMap[eventType] = (Action<T1>)Delegate.Remove((Action<T1>)m_eventMap[eventType], handler);
            }
        }
        public void RemoveEventHandler<T1, T2>(string eventType, Action<T1, T2> handler)
        {
            if (OnHandlerRemoving(eventType, handler))
            {
                m_eventMap[eventType] = (Action<T1, T2>)Delegate.Remove((Action<T1, T2>)m_eventMap[eventType], handler);
            }
        }
        public void RemoveEventHandler<T1, T2, T3>(string eventType, Action<T1, T2, T3> handler)
        {
            if (OnHandlerRemoving(eventType, handler))
            {
                m_eventMap[eventType] = (Action<T1, T2, T3>)Delegate.Remove((Action<T1, T2, T3>)m_eventMap[eventType], handler);
            }
        }
        public void RemoveEventHandler<T1, T2, T3, T4>(string eventType, Action<T1, T2, T3, T4> handler)
        {
            if (OnHandlerRemoving(eventType, handler))
            {
                m_eventMap[eventType] = (Action<T1, T2, T3, T4>)Delegate.Remove((Action<T1, T2, T3, T4>)m_eventMap[eventType], handler);
            }
        }
        public void BroadCastEvent(string eventType)
        {
            if (OnBroadCasting(eventType))
            {
                Action action = m_eventMap[eventType] as Action;
                if (action != null)
                {
                    action();
                }
            }
        }
        public void BroadCastEvent<T>(string eventType, T args1)
        {
            if (OnBroadCasting(eventType))
            {
                Action<T> action = m_eventMap[eventType] as Action<T>;
                if (action != null)
                {
                    action(args1);
                }
            }
        }
        public void BroadCastEvent<T1, T2>(string eventType, T1 args1, T2 args2)
        {
            if (OnBroadCasting(eventType))
            {
                Action<T1, T2> action = m_eventMap[eventType] as Action<T1, T2>;
                if (action != null)
                {
                    action(args1, args2);
                }
            }
        }
        public void BroadCastEvent<T1, T2, T3>(string eventType, T1 args1, T2 args2, T3 args3)
        {
            if (OnBroadCasting(eventType))
            {
                Action<T1, T2, T3> action = m_eventMap[eventType] as Action<T1, T2, T3>;
                if (action != null)
                {
                    action(args1, args2, args3);
                }
            }
        }
        public void BroadCastEvent<T1, T2, T3, T4>(string eventType, T1 args1, T2 args2, T3 args3, T4 args4)
        {
            if (OnBroadCasting(eventType))
            {
                Action<T1, T2, T3, T4> action = m_eventMap[eventType] as Action<T1, T2, T3, T4>;
                if (action != null)
                {
                    action(args1, args2, args3, args4);
                }
            }
        }
    }
}