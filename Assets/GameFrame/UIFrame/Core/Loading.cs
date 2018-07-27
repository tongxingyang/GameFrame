using System.Collections;
using System.Collections.Generic;
using GameFrame;
using UIFrameWork;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loading : WindowBase
{
    private Text loadingText;
    private Slider Slider;
    private LoadingContent _loadingContent = null;
    private AsyncOperation _asyncOperation = null;
    protected override void OnInit(Camera UICamera)
    {
        base.OnInit(UICamera);
        loadingText = CacheTransform.Find("Text").GetComponent<Text>();
        Slider = CacheTransform.Find("Slider").GetComponent<Slider>();

    }

    protected override void OnWindowCustomUpdate()
    {
        base.OnWindowCustomUpdate();
        
    }

    protected override void OnAppear(int sequence, int openOrder, WindowContext context)
    {
        base.OnAppear(sequence, openOrder, context);
        _loadingContent = context as LoadingContent;
        if (_loadingContent != null)
        {
            SingletonMono<GameFrameWork>.GetInstance().StartCoroutine(StartLoading());
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
    /// <summary>
    /// 
    /// </summary>
    /// <param name="scenename"></param>
    /// <returns></returns>
    protected IEnumerator StartLoading()//防止出现进度条卡顿的问题
    {
        int displayProgress = 0;
        int toProgress = 0;
        _asyncOperation = SceneManager.LoadSceneAsync(_loadingContent.SceneName);
        _asyncOperation.allowSceneActivation = false;
        while (_asyncOperation.priority<0.9f)
        {
            toProgress = (int) _asyncOperation.priority * 100;
            while (displayProgress<toProgress)
            {
                ++displayProgress;
                RefreshInfo(displayProgress);
                yield return new WaitForEndOfFrame();
            }
        }
        toProgress = 100;
        while (displayProgress<toProgress)
        {
            ++displayProgress;
            RefreshInfo(displayProgress);
            yield return new WaitForEndOfFrame();
        }
        _asyncOperation.allowSceneActivation = true;
        Singleton<WindowManager>.GetInstance().CloseWindow(false,"Loading");
        _asyncOperation = null;
        Clear();
    }

    private void Clear()
    {
        _asyncOperation = null;
        loadingText.text = "Loading.........";
        Slider.value = 0;
        
    }
    protected override void OnHide(WindowContext context)
    {
        base.OnHide(context);
        _loadingContent = null;
    }

    protected override void OnClose(WindowContext context)
    {
        base.OnClose(context);
        _loadingContent = null;
    }
}
