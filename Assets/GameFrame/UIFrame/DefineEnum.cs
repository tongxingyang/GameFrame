using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIFrameWork
{
    /// <summary>
    /// 面板优先级枚举
    /// </summary>
    public enum enWindowPriority {
        Priority0,
        Priority1,
        Priority2,
        Priority3,
        Priority4,
        Priority5,
        Priority6,
        Priority7,
        Priority8,
        Priority9
    }
    
    /// <summary>
    /// 打开时是否屏蔽ui事件
    /// </summary>
    public enum enWindowColliderMode
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
    
    public enum enWindowState
    {
        None,
        Init,
        Appear,
        Hide,
        Close,
    }
    public enum enWindowType
    {
        LoginAndRegister,
        GameInfo,
        Mail,
        Rank,
        MainUI,
        FriendUI,
    }
    public enum enWindowHideFlag
    {
        HideByCustom = 1,
        HideByOtherForm
    }

    public enum enLayer2Int
    {
        UIRawLayer = 5,
        UIParticleLayer = 6,
    }
}
