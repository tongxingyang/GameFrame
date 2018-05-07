using System;
using GameFrame;
using UIFrameWork;
//using LuaInterface;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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
        InitRenderFeature()//初始化渲染设置 高端机型匹配
        Util.SetResolution(Resolution);
        Application.backgroundLoadingPriority = UnityEngine.ThreadPriority.High;
        Application.runInBackground = true;
        Application.targetFrameRate = 30;
        QualitySettings.blendWeights = BlendWeights.TwoBones;//动画期间可影响某个指定顶点的骨骼的数量。可用的选项有一、二或四根骨骼
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
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
        //加载Lua 虚拟机 todo
        //初始化resources manager todo
        //LuaState lua = new LuaState();
        //lua.Start();
        //string hello =
        //                        @"                
        //                            print('hello tolua#')                                  
        //                        ";
        //lua.DoString(hello);
        
        Debug.Log("加载Lua虚拟机 初始化资源manager");
        Debug.Log("开始游戏");

        //Singleton<ResourceManager>.GetInstance().AddTask("sfx/s_qz_yzb_01.bundle",
        //    (o) =>
        //    {

        //    }, (int) (enResourceLoadType.Cache|enResourceLoadType.LoadBundleFromFile));
        StartCoroutine(ChangeScence());
    }

 
    IEnumerator ChangeScence()
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("Login", LoadSceneMode.Single);
        yield return asyncOperation;
        Singleton<WindowManager>.Instance.InitWindowManager();
        Singleton<WindowManager>.Instance.OpenWindow(new WindowInfo(WindowType.LoginAndRegister, ShowMode.Normal,
            OpenAction.DoNothing, ColliderMode.Node));
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
        Singleton<LauncherString>.GetInstance();
        Singleton<TimeManager>.GetInstance();
        Singleton<EventManager>.GetInstance();
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
        Singleton<ResourceManager>.GetInstance().Update();
        Singleton<PhotonManager>.GetInstance().Update();
    }

    private void OnDestroy()
    {
        Singleton<PhotonManager>.GetInstance().OnDestory();
    }
    private void OnApplicationPause(bool pauseStatus)
    {

    }

    private void OnApplicationFocus(bool hasFocus)
    {

    }

    private void OnApplicationQuit()
    {
        Singleton<PhotonManager>.GetInstance().OnDestory();
    }
}
