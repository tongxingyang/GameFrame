using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using GameFrame;
using Junfine.Debuger;
using UnityEngine;

public class BootUp : MonoBehaviour
{

    void Start()
    {
        SingletonMono<GameFrameWork>.GetInstance();
        Singleton<EventManager>.GetInstance().AddEventListener(enEventID.Test, TestEventManager1);
        Singleton<EventManager>.GetInstance().AddEventListener(enEventID.Test, TestEventManager2);
        //		TestTimeManager();
        
        //minvalue    ok
        MUDebug.IsOpenDebug
       
    }

    void TestTimeManager()
    {
        Singleton<TimeManager>.GetInstance().AddTimer(2000, 10, TimeManager.enTimeType.NoTimeScale, (i, j) =>
        {
            Debug.LogError("aaaaaaaaa " + i + "   " + j[0]);
        }, 10);
    }
    void TestEventManager1(GameFrame.Event @event)
    {
        Debug.LogError(@event.EnEventId);
        Debug.LogError(@event.StEventParms.id);
        Debug.LogError(@event.StEventParms.name);
        Debug.LogError("11111");
    }
    void TestEventManager2(GameFrame.Event @event)
    {
        Debug.LogError(@event.EnEventId);
        Debug.LogError(@event.StEventParms.id);
        Debug.LogError(@event.StEventParms.name);
        Debug.LogError("22222");
    }

    void Update()
    {
        //测试TestEventManager
        //		if (Input.GetKeyDown(KeyCode.A))
        //		{
        //			stEventParms stEventParms = new stEventParms();
        //			stEventParms.id = 1000;
        //			stEventParms.name = "hello";
        //			Singleton<EventManager>.GetInstance().DispathEvent(enEventID.Test,stEventParms);
        //		}
        //		if (Input.GetKeyDown(KeyCode.B))
        //		{
        //			Singleton<EventManager>.GetInstance().RemoveEventListener(enEventID.Test, TestEventManager1);
        //		}
    }
}