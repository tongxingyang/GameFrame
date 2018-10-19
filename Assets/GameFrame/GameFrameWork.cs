using System;
using GameFrame;
using UIFrameWork;
using UnityEngine;
using System.Collections;
using GameFrame.Language;
using GameFrame.Common;
using GameFrame.NetWork;
using GameFrame.Update;
using GameFrameDebuger;
using SceneManager = GameFrame.Scene.SceneManager;

namespace GameFrame
{   
    public class GameFrameWork : SingletonMono<GameFrameWork>
    {
        //标记当前更新是否结束
        private bool IsUpdateDone = false;
    
        public GameConfig GameConfig = null;



        #region 当前机器的渲染能力参数

        private const int SupportFeaturePostProcess = 0; 
        private const int SupportFeatureDynamicShadow = 1;
        private const int SupportFeatureMaxTextureSize2048 = 2;
        private const int SupportFeatureRenderTarget30 = 3;
        private const int SupportFeatureHighPerformance = 4;
        private const int SupportFeatureNum = 5;
        
        private static BitArray feature = new BitArray(SupportFeatureNum);

        public static BitArray Feature
        {
            get { return feature; }
        }
        
        private void InitRenderFeature()
        {
            feature[SupportFeaturePostProcess]        = SystemInfo.supportsImageEffects;
            feature[SupportFeatureDynamicShadow]      = SystemInfo.graphicsShaderLevel >= 30 &&
                                                          (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth) || SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8));
            feature[SupportFeatureMaxTextureSize2048] = SystemInfo.maxTextureSize >= 2048;
            feature[SupportFeatureRenderTarget30]     = SystemInfo.graphicsShaderLevel >= 30;
            feature[SupportFeatureHighPerformance]    = SystemInfo.systemMemorySize > 1024 ;
    
    #if UNITY_IPHONE
            var aspectRatio = (float)Screen.width / Screen.height;
            m_feature[SupportFeatureHighPerformance]    = SystemInfo.systemMemorySize > 768 && aspectRatio > 1.5f;
    #endif
    
    #if UNITY_ANDROID
            var pattern = @"\bAdreno.*[3-9][3-9][0-9]|\bAdreno.*[5-9][0-9][0-9]|\bMali-T8[6-9][0-9]\s+MP[6-9]|\bMali.*MP8|.*MP12";  
            m_feature[SupportFeatureHighPerformance]  &= Regex.IsMatch(SystemInfo.graphicsDeviceName, pattern);
    #endif
            feature[SupportFeatureDynamicShadow] &=  feature[SupportFeatureHighPerformance];
            
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
            Debuger.Log(String.Format("SystemInfo.SupportsRenderTextureFormat Depth: [{0}]", SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth)));
            Debuger.Log(String.Format("SystemInfo.supportsShadows: [{0}]", SystemInfo.supportsShadows));
            Debuger.Log(String.Format("SystemInfo.systemMemorySize: [{0}]", SystemInfo.systemMemorySize));
            for (int i = 0; i < feature.Count; i++)
            {
                Debuger.Log(String.Format("RenderInfo : [{0}]", feature[i]));
            }
    #endif
    
        }
        #endregion
    
        void Awake()
        {
            if (!GameConfig)
            {
                GameConfig = gameObject.AddComponent<GameConfig>();
            }

            Util.SetResolution(GameConfig.Resolution);
            InitDebuger();
            InitRenderFeature();
            InitBaseSys();
            //Singleton<LanguageManager>.GetInstance().LoadLanguageConfig();
            Singleton<LanguageManager>.GetInstance().SetCurrentLanguage(GameConfig.Language);
            Application.backgroundLoadingPriority = UnityEngine.ThreadPriority.High;
            Application.runInBackground = true;
            Application.targetFrameRate = GameConfig.FrameRate;
            QualitySettings.blendWeights = BlendWeights.TwoBones;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

       
        /// <summary>
        /// 初始化Debuger
        /// </summary>
        private void InitDebuger()
        {
            Debuger.Init(Platform.DEBUG_LOG_PATH, new UnityDebugerConsole());
            Debuger.EnableLog = GameConfig.EnableLog;
            Debuger.EnableSave = GameConfig.EnableSave;
        }
        
        void Start()
        {
            
    #if UNITY_ANDROID || UNITY_IPHONE
            PlayLogoVider("logo.mp4",false);
    #endif
    
            if (GameConfig.CheckUpdate)
            {
                Singleton<UpdateManager>.GetInstance().StartCheckUpdate(UpdateCallback);
            }
            else
            {
                UpdateCallback(true);
            }
        }
    
        private void UpdateCallback(bool isok)
        {
            IsUpdateDone = isok;
            SingletonMono<LuaManager>.GetInstance().InitStart();
            Singleton<ConfigManager.ConfigManager>.GetInstance().InitConfig();
            ChangeScence();
        }
    
     
        void ChangeScence()
        {
            SingletonMono<SceneManager>.GetInstance().LoadScene("Login",null, () =>
            {
                Singleton<WindowManager>.GetInstance().OpenWindow("LoginAndRegister",true);

            },null);
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
                      Debuger.Log("播放Logo出错");
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
                 Debuger.Log("播放Logo出错");
            }
            #endif 
        }
        
        void InitBaseSys()
        {
            SingletonMono<LuaManager>.GetInstance();
            SingletonMono<AudioManager>.GetInstance();
            Singleton<ConfigManager.ConfigManager>.GetInstance();
            SingletonMono<MonoHelper>.GetInstance();
            Singleton<TimeManager>.GetInstance();
            Singleton<EventManager>.GetInstance();
            Singleton<EventRouter>.GetInstance();
            Singleton<WindowManager>.GetInstance();
            SingletonMono<CoroutineExecutor>.GetInstance();
            Singleton<ResourceManager>.GetInstance();
            SingletonMono<ServerManager>.GetInstance();
            Singleton<LanguageManager>.GetInstance();
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
            SingletonMono<AudioManager>.GetInstance().OnUpdate();
            SingletonMono<MonoHelper>.GetInstance().OnUpdate();
        }
    
        void LateUpdate()
        {
            Singleton<WindowManager>.GetInstance().OnLateUpdate();
            SingletonMono<MonoHelper>.GetInstance().OnLaterUpdate();
        }

        void FixedUpdate()
        {
            SingletonMono<MonoHelper>.GetInstance().OnFixedUpdate();
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
}

