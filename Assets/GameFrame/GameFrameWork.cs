using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using GameFrame;
using UnityEngine;
using Junfine.Debuger;
public class GameFrameWork : SingletonMono<GameFrameWork>
{
    private bool CheckUpdate = false;
#if UNITY_IPHONE
    private const int Resolution = 1080;
#else
    private const int Resolution = 810;
#endif
    
    public override void Init()
    {
        
        base.Init();
    }

    public override void UnInit()
    {
        base.UnInit();
    }

    void Awake()
    {
        Util.SetResolution(Resolution);
        Application.targetFrameRate = 30;
        QualitySettings.blendWeights = BlendWeights.TwoBones;//动画期间可影响某个指定顶点的骨骼的数量。可用的选项有一、二或四根骨骼
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    void Start()
    {
        //play Logo
        PlayLogoVider("logo.mp4",false);
        Singleton<UpdateManager>.GetInstance();
        Singleton<UpdateManager>.GetInstance().StartCheckUpdate(UpdateCallback);
    }

    private void UpdateCallback(bool isok)
    {
        
    }
    void PlayLogoVider(string filename,bool cancancel)
    {
        #if UNITY_ANDROID
            try
            {
                FullScreenMovieControlMode mode =
                    cancancel ? FullScreenMovieControlMode.CancelOnInput : FullScreenMovieControlMode.Hidden;
                Handheld.PlayFullScreenMovie(filename, Color.black, mode, FullScreenMovieScalingMode.AspectFit);
                
            }
            catch(Exception e)
            {
            }
        #elif UNITY_IPHONE
        try
        {
        }
        catch (Exception e)
        {
            string version = UnityEngine.iOS.Device.systemVersion.Substring(0, 1);
            int ver = Convert.ToInt32(version);
            if (ver > 7 || ver < 2)
            {
                FullScreenMovieControlMode mode =
                    cancancel ? FullScreenMovieControlMode.CancelOnInput : FullScreenMovieControlMode.Hidden;
                Handheld.PlayFullScreenMovie(filename, Color.black, mode, FullScreenMovieScalingMode.AspectFit);
            }
        }
        #else
        #endif 
    }
    void InitBaseSys()
    {
        Singleton<LauncherString>.GetInstance();
        Singleton<TimeManager>.GetInstance();
        Singleton<EventManager>.GetInstance();
    }

    void Update()
    {
        Singleton<TimeManager>.GetInstance().Update();
    }

    private void OnDestroy()
    {

    }
    private void OnApplicationPause(bool pauseStatus)
    {

    }

    private void OnApplicationFocus(bool hasFocus)
    {

    }

    private void OnApplicationQuit()
    {

    }
}
