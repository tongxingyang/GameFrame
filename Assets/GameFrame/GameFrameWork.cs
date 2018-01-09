using System;
using System.Collections;
using System.Collections.Generic;
using GameFrame;
using UnityEngine;
using Junfine.Debuger;
public class GameFrameWork : SingletonMono<GameFrameWork>
{
    
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
       Debuger.Log( DateTime.Now.Ticks.ToString("d" + 32));
       Debuger.Log( DateTime.Now.Ticks.ToString());
    }

    void InitBaseSys()
    {
        Singleton<TimeManager>.GetInstance();
        Singleton<EventManager>.GetInstance();
    }
    void Start()
    {

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