using System;
using System.Collections.Generic;
using System.Text;
using GameFrameDebuger;
using UnityEngine;

namespace GameFrame.FSM
{
    public class StateMachine<T>:IStateMachine<T>
    {
        /// <summary>
        /// 当前的StateID
        /// </summary>
        private T currentStateID;
        
        /// <summary>
        /// 前一个StateID
        /// </summary>
        private T previousStateID;
        
        /// <summary>
        /// 状态字典
        /// </summary>
        private Dictionary<T, IState<T>> States;

        public IState<T> CurrentState
        {
            get { return States[currentStateID]; }
        }

        public IState<T> PerviousState
        {
            get { return States[previousStateID]; }
        }
        /// <summary>
        /// 当前状态机的ID
        /// </summary>
        public int ID;

        public StateMachine(IState<T>[] states, T initStateID,int id)
        {
            this.Initialize(states,initStateID);
            this.ID = id;
        }

        private void Initialize(IState<T>[] states, T initStateID)
        {
            this.VerifyEnum();
            this.VerifyMissing(states);
            this.VerifyDuplicates(states);
            this.States = new Dictionary<T, IState<T>>();
            for (int i = 0; i < states.Length; i++)
            {
                this.States.Add(states[i].ID,states[i]);
            }
            this.currentStateID = initStateID;
            this.CurrentState.Enter();
        }


        private void VerifyEnum()
        {
            if (!typeof(T).IsEnum)
            {
                Debuger.LogError("类型出错不是枚举类型");
            }
        }

        private void VerifyDuplicates(IState<T>[] states)
        {
            var duplicateIDs = GetDuplicateIDs(states);
            if (duplicateIDs.Length > 0)
            {
               Debuger.LogError("重复添加枚举："+duplicateIDs);
            }
        }

        private void VerifyMissing(IState<T>[] states)
        {
            var missings = GetMessingIDs(states);
            if (missings.Length > 0)
            {
                Debuger.LogError("丢失相关枚举结构："+missings);
            }
        }
        
        /// <summary>
        /// 改变状态机的状态
        /// </summary>
        /// <param name="state"></param>
        public void ChangeState(T state)
        {
            if (state.Equals(this.currentStateID))
            {
                return;
            }
            this.CurrentState.Exit();
            this.previousStateID = this.currentStateID;
            this.currentStateID = state;
            this.CurrentState.Enter();
        }
        
        /// <summary>
        /// 更新状态机
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {
            this.CurrentState.Update(deltaTime);
        }
        /// <summary>
        /// 获得MessingIDs
        /// </summary>
        /// <param name="states"></param>
        /// <returns></returns>
        private static string GetMessingIDs(IState<T>[] states)
        {
            var  found = new List<T>();
            for (int i = 0; i < states.Length; i++)
            {
                found.Add(states[i].ID);
            }
            var entries = Enum.GetValues(typeof(T));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < entries.Length; i++)
            {
                var entry = (T)entries.GetValue(i);
                if (!found.Contains(entry))
                {
                    sb.Append(entry.ToString());
                }
            }
            return sb.ToString();
        }
        /// <summary>
        /// 获得重复的IDs
        /// </summary>
        /// <param name="states"></param>
        /// <returns></returns>
        private static string GetDuplicateIDs(IState<T>[] states)
        {
            var extras = new List<T>();
            for (int i = 0; i < states.Length; i++)
            {
                extras.Add(states[i].ID);
            }
            var enteres = Enum.GetValues(typeof(T));
            for (int i = 0; i < enteres.Length; i++)
            {
                var entry = (T) enteres.GetValue(i);
                if (extras.Contains(entry))
                {
                    extras.Remove(entry);
                }
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < extras.Count; i++)
            {
                sb.Append(extras[i].ToString());
            }
            return sb.ToString();
        }
    }
}