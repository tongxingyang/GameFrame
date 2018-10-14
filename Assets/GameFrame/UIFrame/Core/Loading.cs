using System.Collections;
using System.Collections.Generic;
using GameFrame;
using UIFrameWork;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SceneManager = GameFrame.Scene.SceneManager;

/// <summary>
/// 加载Scene的Loading进度条 跳转场景
/// </summary>
public class Loading : WindowBase
{
    private Text loadingText;
    private Slider Slider;
    private LoadingContent _loadingContent = null;
    protected override void OnInit(Camera UICamera)
    {
        base.OnInit(UICamera);
        loadingText = CacheTransform.Find("Text").GetComponent<Text>();
        Slider = CacheTransform.Find("Slider").GetComponent<Slider>();

    }

    protected override void OnAppear(int sequence, int openOrder, WindowContext context)
    {
        base.OnAppear(sequence, openOrder, context);
        _loadingContent = context as LoadingContent;
        Clear();
        if (_loadingContent != null)
        {
            if (_loadingContent.AppearCallBack != null)
            {
                _loadingContent.AppearCallBack.Invoke();
                SingletonMono<SceneManager>.GetInstance().ProgressLoad += RefreshInfo;
            }
        }
        else
        {
            Debug.LogError("Loading Error ............");
        }
    }

    private void RefreshInfo(int value)
    {
        loadingText.text = "Loading........." + value + "%";
        Slider.value = value;
    }

    private void Clear()
    {
        loadingText.text = "Loading.........";
        Slider.value = 0;
    }
    protected override void OnHide(WindowContext context)
    {
        base.OnHide(context);
        _loadingContent.Clear();
        SingletonMono<SceneManager>.GetInstance().ProgressLoad -= RefreshInfo;
        _loadingContent = null;
    }

    protected override void OnClose(WindowContext context)
    {
        base.OnClose(context);
        _loadingContent.Clear();
        SingletonMono<SceneManager>.GetInstance().ProgressLoad -= RefreshInfo;
        _loadingContent = null;
    }
}
