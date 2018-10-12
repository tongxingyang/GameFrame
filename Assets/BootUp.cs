using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using GameFrame;
using UnityEngine;

public class BootUp : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        SingletonMono<GameFrameWork>.GetInstance();
    }
}