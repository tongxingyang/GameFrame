using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIFrameWork
{
    /// <summary>
    /// 打开面板的类型
    /// </summary>
    public enum ShowMode
    {
        /// <summary>
        /// Normal 正常类型 比如背包 排行榜 装备 只有这种类型可以追溯
        /// </summary>
        Normal,
        /// <summary>
        /// 主场景 不能被关闭
        /// </summary>
        Main,
        /// <summary>
        /// 固定位置界面 比如 人物头像信息
        /// </summary>
        Fixed,
        /// <summary>
        /// 弹出界面 信息对话框
        /// </summary>
        PopUp,

    }
    /// <summary>
    /// 界面打开时的行为
    /// </summary>
    public enum OpenAction
    {
        /// <summary>
        /// 不处理
        /// </summary>
        DoNothing,
        /// <summary>
        /// 隐藏Normal类型与Main类型
        /// </summary>
        HideNormalAndMain,
        /// <summary>
        /// 隐藏所有
        /// </summary>
        HideAll,
    }
    /// <summary>
    /// 打开时是否屏蔽ui事件
    /// </summary>
    public enum ColliderMode
    {
        /// <summary>
        /// 不处理
        /// </summary>
        Node,
        /// <summary>
        /// 透明遮罩
        /// </summary>
        Transparent,
        /// <summary>
        /// 黑色遮罩
        /// </summary>
        Dark,
    }
    
    public enum UIState
    {
        None,
        Init,
        Enter,
        Exit,
        Pause,
        Resume
    }
    public enum WindowType
    {
        LoginAndRegister,
        GameInfo,
        Mail,
        Rank,
    }
}
