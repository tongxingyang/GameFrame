﻿using System;
using GameFrame;
using UIFrameWork;
//using LuaInterface;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Text.RegularExpressions;
using GameFrame.ConfigManager;

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
        UnityEngine.Debug.LogWarning(String.Format("SystemInfo.deviceModel: [{0}]",SystemInfo.deviceModel));
        UnityEngine.Debug.LogWarning(String.Format("SystemInfo.deviceName: [{0}]", SystemInfo.deviceName));
        UnityEngine.Debug.LogWarning(String.Format("SystemInfo.graphicsDeviceName: [{0}]", SystemInfo.graphicsDeviceName));
        UnityEngine.Debug.LogWarning(String.Format("SystemInfo.graphicsDeviceID: [{0}]", SystemInfo.graphicsDeviceID));
        UnityEngine.Debug.LogWarning(String.Format("SystemInfo.graphicsDeviceVendor: [{0}]", SystemInfo.graphicsDeviceVendor));
        UnityEngine.Debug.LogWarning(String.Format("SystemInfo.graphicsDeviceVendorID: [{0}]", SystemInfo.graphicsDeviceVendorID));
        UnityEngine.Debug.LogWarning(String.Format("SystemInfo.graphicsDeviceVersion: [{0}]", SystemInfo.graphicsDeviceVersion));
        UnityEngine.Debug.LogWarning(String.Format("SystemInfo.graphicsMemorySize: [{0}]", SystemInfo.graphicsMemorySize));
        UnityEngine.Debug.LogWarning(String.Format("SystemInfo.graphicsShaderLevel: [{0}]", SystemInfo.graphicsShaderLevel));
        UnityEngine.Debug.LogWarning(String.Format("SystemInfo.maxTextureSize: [{0}]", SystemInfo.maxTextureSize));
        UnityEngine.Debug.LogWarning(String.Format("SystemInfo.npotSupport: [{0}]", SystemInfo.npotSupport.ToString()));
        UnityEngine.Debug.LogWarning(String.Format("SystemInfo.operatingSystem: [{0}]", SystemInfo.operatingSystem));
        UnityEngine.Debug.LogWarning(String.Format("SystemInfo.processorType: [{0}]", SystemInfo.processorType));
        UnityEngine.Debug.LogWarning(String.Format("SystemInfo.supportedRenderTargetCount: [{0}]", SystemInfo.supportedRenderTargetCount));
        UnityEngine.Debug.LogWarning(String.Format("SystemInfo.supportsImageEffects: [{0}]", SystemInfo.supportsImageEffects));
        UnityEngine.Debug.LogWarning(String.Format("SystemInfo.supportsRenderTextures: [{0}]", SystemInfo.supportsRenderTextures));
        UnityEngine.Debug.LogWarning(String.Format("SystemInfo.SupportsRenderTextureFormat Depth: [{0}]", SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth)));
        UnityEngine.Debug.LogWarning(String.Format("SystemInfo.supportsShadows: [{0}]", SystemInfo.supportsShadows));
        UnityEngine.Debug.LogWarning(String.Format("SystemInfo.systemMemorySize: [{0}]", SystemInfo.systemMemorySize));
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
        InitRenderFeature();  
        InitBaseSys();
        Util.SetResolution(Resolution);
        Application.backgroundLoadingPriority = UnityEngine.ThreadPriority.High;
        Application.runInBackground = true;
        Application.targetFrameRate = 30;
        QualitySettings.blendWeights = BlendWeights.TwoBones;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    void Start()
    {
        #if UNITY_ANDROID || UNITY_IPHONE
//        PlayLogoVider("logo.mp4",false);
//        Singleton<UpdateManager>.GetInstance().StartCheckUpdate(UpdateCallback);
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
        StartCoroutine(ChangeScence());
    }

 
    IEnumerator ChangeScence()
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("Login", LoadSceneMode.Single);
        yield return asyncOperation;
        Singleton<WindowManager>.GetInstance().Init();
        Singleton<WindowManager>.Instance.OpenWindow("LoginAndRegister",true);
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
        Singleton<WindowManager>.GetInstance();
        Singleton<ResourceManager>.GetInstance();
        Singleton<PhotonManager>.GetInstance();
    }

    void Update()
    {
        Singleton<TimeManager>.GetInstance().Update();
        if (IsUpdateDone == false)
        {
            Singleton<UpdateManager>.GetInstance().Update();
        }
        Singleton<WindowManager>.GetInstance().Update();
        Singleton<ResourceManager>.GetInstance().Update();
        Singleton<PhotonManager>.GetInstance().Update();
    }

    void LateUpdate()
    {
        Singleton<WindowManager>.GetInstance().LateUpdate();
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
        SingletonMono<LuaManager>.GetInstance().Close();
        Singleton<PhotonManager>.GetInstance().OnDestory();
    }
}
