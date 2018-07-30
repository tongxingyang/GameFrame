using System;
using System.Collections;
using System.Collections.Generic;
using UIFrameWork;
using UnityEngine;

public class LoadingContent : WindowContext
{
    public string SceneName = string.Empty;
    public Action CallBack = null;
    /// <summary>
    /// 构造
    /// </summary>
    /// <param name="name">要加载的游戏场景名称</param>
    public LoadingContent(string name,Action action)
    {
        SceneName = name;
        CallBack = action;
    }
}
