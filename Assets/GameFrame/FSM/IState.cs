using System;

namespace GameFrame.FSM
{
    /// <summary>
    /// IState接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IState<T> 
    {
        /// <summary>
        /// 状态的标识符
        /// </summary>
        T ID { get; }

        /// <summary>
        /// 进入当前状态
        /// </summary>
        void Enter();
        
        /// <summary>
        /// 退出当前状态
        /// </summary>
        void Exit();
        
        /// <summary>
        /// 更新当前状态
        /// </summary>
        /// <param name="deleaTime"></param>
        void Update(float deleaTime);
    }
}