using System;
using System.Collections.Generic;

namespace GameFrame.FSM
{
    /// <summary>
    /// State类
    /// </summary>
    public class State<T>:IState<T>
    {
        private EnterMethod enter;
        private ExitMethod exit;
        private UpdateMethod update;
        /// <summary>
        /// 状态的标识符
        /// </summary>
        public T ID { get; private set; }

        public State(T id,EnterMethod enterMethod,ExitMethod exitMethod,UpdateMethod updateMethod)
        {
            this.ID = id;
            this.enter += enterMethod;
            this.exit += exitMethod;
            this.update += updateMethod;
        }
        
        /// <summary>
        /// 进入当前状态
        /// </summary>
        public void Enter()
        {
            if (enter != null)
            {
                enter();
            }
        }
        
        /// <summary>
        /// 退出当前状态
        /// </summary>
        public void Exit()
        {
            if (exit != null)
            {
                exit();
            }
        }
        
        /// <summary>
        /// 更新当前状态
        /// </summary>
        /// <param name="deleaTime"></param>
        public void Update(float deleaTime)
        {
            if (update != null)
            {
                update(deleaTime);
            }
        }

        #region Action

        public void AddEvent(string eventType, Action handler)
        {
            Singleton<EventRouter>.GetInstance().AddEventHandler(eventType,handler);
        }
        public void AddEvent<T1>(string eventType, Action<T1> handler)
        {
            Singleton<EventRouter>.GetInstance().AddEventHandler<T1>(eventType,handler);
        }
        public void AddEvent<T1, T2>(string eventType, Action<T1, T2> handler)
        {
            Singleton<EventRouter>.GetInstance().AddEventHandler<T1,T2>(eventType,handler);
        }
        public void AddEvent<T1, T2, T3>(string eventType, Action<T1, T2, T3> handler)
        {
            Singleton<EventRouter>.GetInstance().AddEventHandler<T1,T2,T3>(eventType,handler);
        }
        public void AddEvent<T1, T2, T3, T4>(string eventType, Action<T1, T2, T3, T4> handler)
        {
            Singleton<EventRouter>.GetInstance().AddEventHandler<T1,T2,T3,T4>(eventType,handler);
        }
        public void RemoveEvent(string eventType, Action handler)
        {
            Singleton<EventRouter>.GetInstance().RemoveEventHandler(eventType,handler);
        }
        public void RemoveEvent<T1>(string eventType, Action<T1> handler)
        {
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<T1>(eventType,handler);
        }
        public void RemoveEvent<T1, T2>(string eventType, Action<T1, T2> handler)
        {
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<T1,T2>(eventType,handler);
        }
        public void RemoveEvent<T1, T2, T3>(string eventType, Action<T1, T2, T3> handler)
        {
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<T1,T2,T3>(eventType,handler);
        }
        public void RemoveEvent<T1, T2, T3, T4>(string eventType, Action<T1, T2, T3, T4> handler)
        {
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<T1,T2,T3,T4>(eventType,handler);
        }
        public void TriggerEventAction(string eventType)
        {
            Singleton<EventRouter>.GetInstance().BroadCastEventAction(eventType);
        }
        public void TriggerEventAction<T1>(string eventType, T1 args1)
        {
            Singleton<EventRouter>.GetInstance().BroadCastEventAction<T1>(eventType,args1);
        }
        public void TriggerEventAction<T1, T2>(string eventType, T1 args1, T2 args2)
        {
            Singleton<EventRouter>.GetInstance().BroadCastEventAction<T1,T2>(eventType,args1,args2);
        }
        public void TriggerEventAction<T1, T2, T3>(string eventType, T1 args1, T2 args2, T3 args3)
        {
            Singleton<EventRouter>.GetInstance().BroadCastEventAction<T1,T2,T3>(eventType,args1,args2,args3);
        }
        public void TriggerEventAction<T1, T2, T3, T4>(string eventType, T1 args1, T2 args2, T3 args3, T4 args4)
        {
            Singleton<EventRouter>.GetInstance().BroadCastEventAction<T1,T2,T3,T4>(eventType,args1,args2,args3,args4);
        }

        #endregion
        
        #region Func

        public void AddEvent<T1>(string eventType, Func<T1> handler)
        {
            Singleton<EventRouter>.GetInstance().AddEventHandler<T1>(eventType,handler);
        }
        public void AddEvent<T1, T2>(string eventType, Func<T1, T2> handler)
        {
            Singleton<EventRouter>.GetInstance().AddEventHandler<T1,T2>(eventType,handler);
        }
        public void AddEvent<T1, T2, T3>(string eventType, Func<T1, T2, T3> handler)
        {
            Singleton<EventRouter>.GetInstance().AddEventHandler<T1,T2,T3>(eventType,handler);
        }
        public void AddEvent<T1, T2, T3, T4>(string eventType, Func<T1, T2, T3, T4> handler)
        {
            Singleton<EventRouter>.GetInstance().AddEventHandler<T1,T2,T3,T4>(eventType,handler);
        }
        public void AddEvent<T1, T2, T3, T4,T5>(string eventType, Func<T1, T2, T3, T4,T5> handler)
        {
            Singleton<EventRouter>.GetInstance().AddEventHandler<T1,T2,T3,T4,T5>(eventType,handler);
        }
        public void RemoveEvent<T1>(string eventType, Func<T1> handler)
        {
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<T1>(eventType,handler);
        }
        public void RemoveEvent<T1, T2>(string eventType, Func<T1, T2> handler)
        {
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<T1,T2>(eventType,handler);
        }
        public void RemoveEvent<T1, T2, T3>(string eventType, Func<T1, T2, T3> handler)
        {
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<T1,T2,T3>(eventType,handler);
        }
        public void RemoveEvent<T1, T2, T3, T4>(string eventType, Func<T1, T2, T3, T4> handler)
        {
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<T1,T2,T3,T4>(eventType,handler);
        }
        public void RemoveEvent<T1, T2, T3, T4,T5>(string eventType, Func<T1, T2, T3, T4,T5> handler)
        {
            Singleton<EventRouter>.GetInstance().RemoveEventHandler<T1,T2,T3,T4,T5>(eventType,handler);
        }
        public T1 TriggerEventFunc<T1>(string eventType)
        {
            return Singleton<EventRouter>.GetInstance().BroadCastEventFunc<T1>(eventType);
        }
        public T2 TriggerEventFunc<T1,T2>(string eventType,T1 t1)
        {
            return Singleton<EventRouter>.GetInstance().BroadCastEventFunc<T1,T2>(eventType,t1);
        }
        public T3 TriggerEventFunc<T1,T2,T3>(string eventType,T1 t1,T2 t2)
        {
            return Singleton<EventRouter>.GetInstance().BroadCastEventFunc<T1,T2,T3>(eventType,t1,t2);
        }
        public T4 TriggerEventFunc<T1,T2,T3,T4>(string eventType,T1 t1,T2 t2,T3 t3)
        {
            return Singleton<EventRouter>.GetInstance().BroadCastEventFunc<T1,T2,T3,T4>(eventType,t1,t2,t3);
        }
        public T5 TriggerEventFunc<T1, T2, T3, T4, T5>(string eventType, T1 t1, T2 t2, T3 t3, T4 t4)
        {
            return Singleton<EventRouter>.GetInstance().BroadCastEventFunc<T1, T2, T3, T4, T5>(eventType, t1, t2, t3, t4);
        }
        #endregion
    }
}