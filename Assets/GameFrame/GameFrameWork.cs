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
