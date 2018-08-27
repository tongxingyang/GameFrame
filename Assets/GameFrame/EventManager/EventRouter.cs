using System;
using System.Collections;
using System.Collections.Generic;
using GameFrameDebuger;
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

        #region Action

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
        public void BroadCastEventAction(string eventType)
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
        public void BroadCastEventAction<T>(string eventType, T args1)
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
        public void BroadCastEventAction<T1, T2>(string eventType, T1 args1, T2 args2)
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
        public void BroadCastEventAction<T1, T2, T3>(string eventType, T1 args1, T2 args2, T3 args3)
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
        public void BroadCastEventAction<T1, T2, T3, T4>(string eventType, T1 args1, T2 args2, T3 args3, T4 args4)
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

        #endregion
        
        #region Func

        public void AddEventHandler<T>(string eventType, Func<T> handler)
        {
            if (OnHandlerAdding(eventType, handler))
            {
                m_eventMap[eventType] = (Func<T>)Delegate.Combine((Func<T>)m_eventMap[eventType], handler);
            }
        }
        public void AddEventHandler<T1, T2>(string eventType, Func<T1, T2> handler)
        {
            if (OnHandlerAdding(eventType, handler))
            {
                m_eventMap[eventType] = (Func<T1, T2>)Delegate.Combine((Func<T1, T2>)m_eventMap[eventType], handler);
            }
        }
        public void AddEventHandler<T1, T2, T3>(string eventType, Func<T1, T2, T3> handler)
        {
            if (OnHandlerAdding(eventType, handler))
            {
                m_eventMap[eventType] = (Func<T1, T2, T3>)Delegate.Combine((Func<T1, T2, T3>)m_eventMap[eventType], handler);
            }
        }
        public void AddEventHandler<T1, T2, T3, T4>(string eventType, Func<T1, T2, T3, T4> handler)
        {
            if (OnHandlerAdding(eventType, handler))
            {
                m_eventMap[eventType] = (Func<T1, T2, T3, T4>)Delegate.Combine((Func<T1, T2, T3, T4>)m_eventMap[eventType], handler);
            }
        }
        public void AddEventHandler<T1, T2, T3, T4,T5>(string eventType, Func<T1, T2, T3, T4,T5> handler)
        {
            if (OnHandlerAdding(eventType, handler))
            {
                m_eventMap[eventType] = (Func<T1, T2, T3, T4,T5>)Delegate.Combine((Func<T1, T2, T3, T4,T5>)m_eventMap[eventType], handler);
            }
        }

        public void RemoveEventHandler<T1>(string eventType, Func<T1> handler)
        {
            if (OnHandlerRemoving(eventType, handler))
            {
                m_eventMap[eventType] = (Func<T1>)Delegate.Remove((Func<T1>)m_eventMap[eventType], handler);
            }
        }
        public void RemoveEventHandler<T1, T2>(string eventType, Func<T1, T2> handler)
        {
            if (OnHandlerRemoving(eventType, handler))
            {
                m_eventMap[eventType] = (Func<T1, T2>)Delegate.Remove((Func<T1, T2>)m_eventMap[eventType], handler);
            }
        }
        public void RemoveEventHandler<T1, T2, T3>(string eventType, Func<T1, T2, T3> handler)
        {
            if (OnHandlerRemoving(eventType, handler))
            {
                m_eventMap[eventType] = (Func<T1, T2, T3>)Delegate.Remove((Func<T1, T2, T3>)m_eventMap[eventType], handler);
            }
        }
        public void RemoveEventHandler<T1, T2, T3, T4>(string eventType, Func<T1, T2, T3, T4> handler)
        {
            if (OnHandlerRemoving(eventType, handler))
            {
                m_eventMap[eventType] = (Func<T1, T2, T3, T4>)Delegate.Remove((Func<T1, T2, T3, T4>)m_eventMap[eventType], handler);
            }
        }
        public void RemoveEventHandler<T1, T2, T3, T4,T5>(string eventType, Func<T1, T2, T3, T4,T5> handler)
        {
            if (OnHandlerRemoving(eventType, handler))
            {
                m_eventMap[eventType] = (Func<T1, T2, T3, T4,T5>)Delegate.Remove((Func<T1, T2, T3, T4,T5>)m_eventMap[eventType], handler);
            }
        }
        
        public T BroadCastEventFunc<T>(string eventType)
        {
            if (OnBroadCasting(eventType))
            {
                Func<T> action = m_eventMap[eventType] as Func<T>;
                if (action != null)
                {
                    return action();
                }
                else
                {
                    Debuger.LogError("EventRouter 出错 尝试调用类型不存在的函数  "+eventType);
                }
            }
            else
            {
                Debuger.LogError("EventRouter 出错 尝试调用不存在的函数  "+eventType);
            }
            return default(T);
        }
        public T2 BroadCastEventFunc<T1,T2>(string eventType,T1 t1)
        {
            if (OnBroadCasting(eventType))
            {
                Func<T1,T2> action = m_eventMap[eventType] as Func<T1,T2>;
                if (action != null)
                {
                    return action(t1);
                }
                else
                {
                    Debuger.LogError("EventRouter 出错 尝试调用类型不存在的函数  "+eventType);
                }
            }
            else
            {
                Debuger.LogError("EventRouter 出错 尝试调用不存在的函数  "+eventType);
            }
            return default(T2);
        }
        public T3 BroadCastEventFunc<T1,T2,T3>(string eventType,T1 t1,T2 t2)
        {
            if (OnBroadCasting(eventType))
            {
                Func<T1,T2,T3> action = m_eventMap[eventType] as Func<T1,T2,T3>;
                if (action != null)
                {
                    return action(t1,t2);
                }
                else
                {
                    Debuger.LogError("EventRouter 出错 尝试调用类型不存在的函数  "+eventType);
                }
            }
            else
            {
                Debuger.LogError("EventRouter 出错 尝试调用不存在的函数  "+eventType);
            }
            return default(T3);
        }
        public T4 BroadCastEventFunc<T1,T2,T3,T4>(string eventType,T1 t1,T2 t2,T3 t3)
        {
            if (OnBroadCasting(eventType))
            {
                Func<T1,T2,T3,T4> action = m_eventMap[eventType] as Func<T1,T2,T3,T4>;
                if (action != null)
                {
                    return action(t1,t2,t3);
                }
                else
                {
                    Debuger.LogError("EventRouter 出错 尝试调用类型不存在的函数  "+eventType);
                }
            }
            else
            {
                Debuger.LogError("EventRouter 出错 尝试调用不存在的函数  "+eventType);
            }
            return default(T4);
        }
        public T5 BroadCastEventFunc<T1,T2,T3,T4,T5>(string eventType,T1 t1,T2 t2,T3 t3,T4 t4)
        {
            if (OnBroadCasting(eventType))
            {
                Func<T1,T2,T3,T4,T5> action = m_eventMap[eventType] as Func<T1,T2,T3,T4,T5>;
                if (action != null)
                {
                    return action(t1,t2,t3,t4);
                }
                else
                {
                    Debuger.LogError("EventRouter 出错 尝试调用类型不存在的函数  "+eventType);
                }
            }
            else
            {
                Debuger.LogError("EventRouter 出错 尝试调用不存在的函数  "+eventType);
            }
            return default(T5);
        }
        #endregion

        public void OnDestory()
        {
            ClearAllEvents();
        }
    }
}