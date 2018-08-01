using System;

namespace GameFrame.FSM
{
    /// <summary>
    /// IStateMachine接口
    /// </summary>
    public interface IStateMachine<T>
    {
        /// <summary>
        /// 改变状态到目标状态
        /// </summary>
        /// <param name="state"></param>
        void ChangeState(T state);
        
        /// <summary>
        /// 更新状态机
        /// </summary>
        /// <param name="deltaTime"></param>
        void Update(float deltaTime);
    }
}