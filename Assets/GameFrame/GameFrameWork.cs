﻿using System;
using GameFrame;
using UIFrameWork;
//using LuaInterface;
using UnityEngine;
using System.Collections;
using GameFrame.Common;
using GameFrame.ConfigManager;
using GameFrame.NetWork;
using GameFrameDebuger;
using SceneManager = GameFrame.Scene.SceneManager;

public class GameFrameWork : SingletonMono<GameFrameWork>
{
    private bool CheckUpdate = false;
    private bool IsUpdateDone = false;
#if UNITY_IPHONE
    private const int Resolution = 1080;
#else
    private const int Resolution = 1080;
#endif

    #region render feature
    private const int SupportFeaturePostProcess = 0; 
    private const int SupportFeatureDynamicShadow = 1;
    private const int SupportFeatureMaxTextureSize2048 = 2;
    private const int SupportFeatureRenderTexture = 3;
    private const int SupportFeatureRenderTarget30 = 4;
    private const int SupportFeatureHighPerformance = 5;
    private const int SupportFeatureNum = 6;

    public static BitArray m_feature = new BitArray(SupportFeatureNum);
    public static BitArray feature
    {
        get { return m_feature; }
    }
    public static bool supportFeatureDynamiShadow { get { return m_feature[SupportFeatureDynamicShadow]; } }
    public static bool supportFeaturePostProcess { get { return m_feature[SupportFeaturePostProcess]; } }
    public static bool supportFeatureHighPerformance { get { return m_feature[SupportFeatureHighPerformance]; } }

    private void InitRenderFeature()
    {
        
        m_feature[SupportFeaturePostProcess]        = SystemInfo.supportsImageEffects && SystemInfo.supportsRenderTextures;
        m_feature[SupportFeatureDynamicShadow]      = SystemInfo.supportsRenderTextures && 
                                                      SystemInfo.graphicsShaderLevel >= 30 &&
                                                      (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth) || SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8));
        m_feature[SupportFeatureMaxTextureSize2048] = SystemInfo.maxTextureSize >= 2048;
        m_feature[SupportFeatureRenderTexture]      = SystemInfo.supportsRenderTextures;
        m_feature[SupportFeatureRenderTarget30]     = SystemInfo.graphicsShaderLevel >= 30;
        m_feature[SupportFeatureHighPerformance]    = SystemInfo.systemMemorySize > 1024 ;

#if UNITY_IPHONE
        var aspectRatio = (float)Screen.width / Screen.height;
        m_feature[SupportFeatureHighPerformance]    = SystemInfo.systemMemorySize > 768 && aspectRatio > 1.5f;
#endif

#if UNITY_ANDROID
        var pattern = @"\bAdreno.*[3-9][3-9][0-9]|\bAdreno.*[5-9][0-9][0-9]|\bMali-T8[6-9][0-9]\s+MP[6-9]|\bMali.*MP8|.*MP12";  
        m_feature[SupportFeatureHighPerformance]  &= Regex.IsMatch(SystemInfo.graphicsDeviceName, pattern);
#endif
        m_feature[SupportFeatureDynamicShadow] &=  m_feature[SupportFeatureHighPerformance];
#if UNITY_EDITOR
        Debuger.Log(String.Format("SystemInfo.deviceModel: [{0}]",SystemInfo.deviceModel));
        Debuger.Log(String.Format("SystemInfo.deviceName: [{0}]", SystemInfo.deviceName));
        Debuger.Log(String.Format("SystemInfo.graphicsDeviceName: [{0}]", SystemInfo.graphicsDeviceName));
        Debuger.Log(String.Format("SystemInfo.graphicsDeviceID: [{0}]", SystemInfo.graphicsDeviceID));
        Debuger.Log(String.Format("SystemInfo.graphicsDeviceVendor: [{0}]", SystemInfo.graphicsDeviceVendor));
        Debuger.Log(String.Format("SystemInfo.graphicsDeviceVendorID: [{0}]", SystemInfo.graphicsDeviceVendorID));
        Debuger.Log(String.Format("SystemInfo.graphicsDeviceVersion: [{0}]", SystemInfo.graphicsDeviceVersion));
        Debuger.Log(String.Format("SystemInfo.graphicsMemorySize: [{0}]", SystemInfo.graphicsMemorySize));
        Debuger.Log(String.Format("SystemInfo.graphicsShaderLevel: [{0}]", SystemInfo.graphicsShaderLevel));
        Debuger.Log(String.Format("SystemInfo.maxTextureSize: [{0}]", SystemInfo.maxTextureSize));
        Debuger.Log(String.Format("SystemInfo.npotSupport: [{0}]", SystemInfo.npotSupport.ToString()));
        Debuger.Log(String.Format("SystemInfo.operatingSystem: [{0}]", SystemInfo.operatingSystem));
        Debuger.Log(String.Format("SystemInfo.processorType: [{0}]", SystemInfo.processorType));
        Debuger.Log(String.Format("SystemInfo.supportedRenderTargetCount: [{0}]", SystemInfo.supportedRenderTargetCount));
        Debuger.Log(String.Format("SystemInfo.supportsImageEffects: [{0}]", SystemInfo.supportsImageEffects));
        Debuger.Log(String.Format("SystemInfo.supportsRenderTextures: [{0}]", SystemInfo.supportsRenderTextures));
        Debuger.Log(String.Format("SystemInfo.SupportsRenderTextureFormat Depth: [{0}]", SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth)));
        Debuger.Log(String.Format("SystemInfo.supportsShadows: [{0}]", SystemInfo.supportsShadows));
        Debuger.Log(String.Format("SystemInfo.systemMemorySize: [{0}]", SystemInfo.systemMemorySize));
        for (int i = 0; i < m_feature.Count; i++)
        {
            Debuger.Log(String.Format("RenderInfo : [{0}]", m_feature[i]));
        }
#endif

    }
    #endregion
    
    
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
        InitDebuger();
        InitRenderFeature();  
        InitBaseSys();
        Util.SetResolution(Resolution);
        Application.backgroundLoadingPriority = UnityEngine.ThreadPriority.High;
        Application.runInBackground = true;
        Application.targetFrameRate = 30;
        QualitySettings.blendWeights = BlendWeights.TwoBones;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private void InitDebuger()
    {
        Debuger.Init(Platform.PERSISTENT_DATA_PATH + "/DebugerLog/", new UnityDebugerConsole());
        Debuger.EnableLog = true;
        Debuger.EnableSave = true;
    }
    void Start()
    {
        #if UNITY_ANDROID || UNITY_IPHONE
        PlayLogoVider("logo.mp4",false);
        Singleton<UpdateManager>.GetInstance().StartCheckUpdate(UpdateCallback);
        #endif
        #if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN
        UpdateCallback(true);
        #endif
    }

    private void UpdateCallback(bool isok)
    {
        IsUpdateDone = isok;
        //启动lua虚拟机 开始执行lua函数
        SingletonMono<LuaManager>.GetInstance().InitStart();
        Singleton<ConfigManager>.GetInstance().InitConfig();
        ChangeScence();
    }

 
    void ChangeScence()
    {
        Singleton<SceneManager>.GetInstance().LoadScene("Login", () =>
        {
            Singleton<WindowManager>.GetInstance().OpenWindow("LoginAndRegister",true);
//            Singleton<WindowManager>.GetInstance().OpenWindow("Dialogue",true,true,new DialogueContent("dialoguedata.json","node1"));
        });
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
                string version = UnityEngine.iOS.Device.systemVersion.Substring(0, 1);
                int ver = Convert.ToInt32(version);
                if (ver > 7 || ver < 2)
                {
                    FullScreenMovieControlMode mode =
                        cancancel ? FullScreenMovieControlMode.CancelOnInput : FullScreenMovieControlMode.Hidden;
                    Handheld.PlayFullScreenMovie(filename, Color.black, mode, FullScreenMovieScalingMode.AspectFit);
                }
        }
        catch (Exception e)
        {

        }
        #endif 
    }
    void InitBaseSys()
    {
        SingletonMono<LuaManager>.GetInstance();
        SingletonMono<AudioManager>.GetInstance();
        Singleton<ConfigManager>.GetInstance();
        Singleton<LauncherString>.GetInstance();
        Singleton<TimeManager>.GetInstance();
        Singleton<EventManager>.GetInstance();
        Singleton<EventRouter>.GetInstance();
        Singleton<WindowManager>.GetInstance();
        Singleton<ResourceManager>.GetInstance();
        SingletonMono<ServerManager>.GetInstance();
    }

    void Update()
    {
        Singleton<TimeManager>.GetInstance().OnUpdate();
        if (IsUpdateDone == false)
        {
            Singleton<UpdateManager>.GetInstance().OnUpdate();
        }
        Singleton<WindowManager>.GetInstance().OnUpdate();
        Singleton<ResourceManager>.GetInstance().OnUpdate();
        Singleton<AudioManager>.GetInstance().OnUpdate();
        
    }

    void LateUpdate()
    {
        Singleton<WindowManager>.GetInstance().OnLateUpdate();
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
        Singleton<EventRouter>.GetInstance().OnDestory();
        Singleton<EventManager>.GetInstance().OnDestory();
        Singleton<TimeManager>.GetInstance().OnDestory();
        SingletonMono<LuaManager>.GetInstance().Close();
        SingletonMono<ServerManager>.GetInstance().OnDestory();
        SingletonMono<AudioManager>.GetInstance().OnDestory();
    }
}
