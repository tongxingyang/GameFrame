﻿using System.Collections.Generic;
using GameFrame;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIFrameWork
{
    /// <summary>
    /// Window 管理类
    /// </summary>
    public class WindowManager : Singleton<WindowManager>
    {
        private List<WindowBase> m_windows;
        private List<WindowBase> m_pooledWindows;
        private int m_windowSequence;
        private List<int> m_exitWindowSequences;
        private GameObject m_root;
        public OnWindowSorted OnWindowSorted;
        private EventSystem m_eventSystem;
        private Camera m_UICamera;

        public Camera UICamera
        {
            get { return m_UICamera; }
        }

        public override void Init()
        {
            base.Init();
            m_windows = new List<WindowBase>();
            m_pooledWindows = new List<WindowBase>();
            m_windowSequence = 0;
            m_exitWindowSequences = new List<int>();
            CreateUIRoot();
            CreateEventSystem();
            CreateCamera();
        }
      
        private void CreateUIRoot()
        {
            this.m_root = new GameObject("UIRoot");
            GameObject obj = GameObject.Find("BootUp");
            if (obj != null)
            {
                this.m_root.transform.SetParent(obj.transform);
                this.m_root.transform.localPosition = Vector3.zero;
            }
        }

        private void CreateEventSystem()
        {
            if (this.m_eventSystem == null)
            {
                GameObject obj = new GameObject("EventSystem");
                this.m_eventSystem = obj.AddComponent<EventSystem>();
                obj.AddComponent<StandaloneInputModule>();
            }
            this.m_eventSystem.gameObject.transform.SetParent(m_root.transform);
        }

        private void CreateCamera()
        {
            GameObject obj = new GameObject("UICamera");
            obj.transform.SetParent(this.m_root.transform,true);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.transform.localRotation = Quaternion.identity;
            Camera camera = obj.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 50;
            camera.clearFlags = CameraClearFlags.Depth;
            camera.depth = 10;
            this.m_UICamera = camera;
        }

        public void UpdateSortingOrder()
        {
            this.m_windows.Sort();
            foreach (WindowBase t in this.m_windows)
            {
                int openorder = this.GetWindowOpenOrder(t.GetSequence());
                t.SetDisplayOrder(openorder);
            }
            if (this.OnWindowSorted != null)
            {
                this.OnWindowSorted(this.m_windows);
            }
        }
        public void Update()
        {
            foreach (WindowBase t in this.m_windows)
            {
                t.CustomUpdate();
            }
        }
        public string GetWindowName(string path)
        {
            string[] arr = path.Split('/');
            return arr[arr.Length - 1];
        }
        public void LateUpdate()
        {
            foreach (WindowBase t in this.m_windows)
            {
                t.CustomLateUpdate();
            }
        }

        public void CloseWindow(bool isforce,WindowBase windowBase,WindowContext windowContext = null)
        {
            for (int i = 0; i < m_windows.Count; i++)
            {
                if (this.m_windows[i] == windowBase)
                {
                    if (this.m_windows[i].m_isUsePool)
                    {
                        this.m_windows[i].Hide(isforce, windowContext);
                    }
                    else
                    {
                        this.m_windows[i].Close(isforce, windowContext);
                    }
                }
            }
        }
        public void CloseWindow(bool isforce,string path, WindowContext windowContext = null)
        {
            for (int i = 0; i < m_windows.Count; i++)
            {
                if (this.m_windows[i].WindowInfo.PerfabPath == path)
                {
                    if (this.m_windows[i].m_isUsePool)
                    {
                        this.m_windows[i].Hide(isforce, windowContext);
                    }
                    else
                    {
                        this.m_windows[i].Close(isforce, windowContext);
                    }
                }
            }
        }
        public void CloseWindow(bool isforce,int sque, WindowContext windowContext = null)
        {
            for (int i = 0; i < m_windows.Count; i++)
            {
                if (this.m_windows[i].GetSequence() == sque)
                {
                    if (this.m_windows[i].m_isUsePool)
                    {
                        this.m_windows[i].Hide(isforce, windowContext);
                    }
                    else
                    {
                        this.m_windows[i].Close(isforce, windowContext);
                    }
                }
            }
        }

        public void CloseAllWindow(bool clearPool = true,bool isforce = false)
        {
            int k = 0;
            while (k<this.m_windows.Count)
            {
                if (this.m_windows[k].m_isUsePool)
                {
                    this.m_windows[k].Hide(isforce, null);
                }
                else
                {
                    this.m_windows[k].Close(isforce, null);
                }
            }
            if (clearPool)
            {
                ClearPool();
            }
        }

        public bool HasWindow()
        {
            return this.m_windows.Count > 0;
        }

        public WindowBase GetWindow(string path)
        {
            for (int i = 0; i < this.m_windows.Count; i++)
            {
                if (this.m_windows[i].WindowInfo.PerfabPath == path)
                {
                    return this.m_windows[i];
                }
            }
            return null;
        }

        public WindowBase GetWindow(int sque)
        {
            for (int i = 0; i < m_windows.Count; i++)
            {
                if (m_windows[i].GetSequence() == sque)
                {
                    return m_windows[i];
                }
            }
            return null;
        }

        public void CloseGroupWindow(int group,bool isforce)
        {
            if(group == 0) return;
            for (int i = 0; i < m_windows.Count; i++)
            {
                if (this.m_windows[i].WindowInfo.Group == group)
                {
                    if (this.m_windows[i].m_isUsePool)
                    {
                        this.m_windows[i].Hide(isforce,null);
                    }
                    else
                    {
                        this.m_windows[i].Close(isforce,null);
                    }
                }
            }
        }

        public WindowBase GetTopWindow()
        {
            WindowBase windowBase = null;
            for (int i = 0; i < this.m_windows.Count; i++)
            {
                if (!(this.m_windows[i] == null))
                {
                    if (windowBase == null)
                    {
                        windowBase = this.m_windows[i];
                    }
                    else if (this.m_windows[i].GetSortingOrder() > windowBase.GetSortingOrder())
                    {
                        windowBase = this.m_windows[i];
                    }
                }
            }
            return windowBase;
        }
        public void DisableInput()
        {
            if (this.m_eventSystem != null)
            {
                this.m_eventSystem.gameObject.SetActive(false);
            }
        }

        private WindowBase GetUnClosedWindow(string path)
        {
            for (int i = 0; i < this.m_windows.Count; i++)
            {
                if (this.m_windows[i].WindowInfo.PerfabPath.Equals(path) && !this.m_windows[i].IsHided())
                {
                    return this.m_windows[i];
                }
            }
            return null;
        }
        public void EnableInput()
        {
            if (this.m_eventSystem != null)
            {
                this.m_eventSystem.gameObject.SetActive(true);
            }
        }
        public void ClearPool()
        {
            for (int i = 0; i < m_pooledWindows.Count; i++)
            {
                Object.DestroyImmediate(m_pooledWindows[i].gameObject);
            }
            this.m_pooledWindows.Clear();
        }
        public void AddToExitSquenceList(int squence)
        {
            if (this.m_exitWindowSequences != null)
            {
                this.m_exitWindowSequences.Add(squence);
            }
        }

        public void RemoveFromExitSquenceList(int squence)
        {
            if (this.m_exitWindowSequences != null)
            {
                this.m_exitWindowSequences.Remove(squence);
            }
        }

        public int GetWindowOpenOrder(int squence)
        {
            int num = this.m_exitWindowSequences.IndexOf(squence);
            if (num >= 0)
            {
                return (num + 1);
            }
            else
            {
                Debug.LogError("error 不应该出现不存在的序列号 请检查调试问题");
                return 0;
            }
        }

        public void RecycleWindow(WindowBase windowBase)
        {
            this.RemoveFromExitSquenceList(windowBase.GetSequence());
            if (windowBase.m_isUsePool)
            {
                this.m_pooledWindows.Add(windowBase);
            }
            this.m_windows.Remove(windowBase);
        }

        private GameObject CreateWindow(string path,bool usePool)
        {
            GameObject obj = null;
            if (usePool)
            {
                for (int i = 0; i < m_pooledWindows.Count; i++)
                {
                    if (string.Equals(path, this.m_pooledWindows[i].WindowInfo.PerfabPath))
                    {
                        obj = this.m_pooledWindows[i].gameObject;
                        this.m_pooledWindows.RemoveAt(i);
                        break;
                    }
                }
            }
            if (obj == null)
            {
                GameObject res = Resources.Load<GameObject>(path);
                if (res == null)
                {
                    return null;
                }
                obj = Object.Instantiate(res);
            }
            if (obj != null)
            {
                WindowBase windowBase = obj.GetComponent<WindowBase>();
                if (windowBase != null)
                {
                    windowBase.m_isUsePool = usePool;
                }
            }
            return obj;
        }

        public WindowBase OpenWindow(string path,bool isusePool,bool useCameraRender = true,WindowContext appear = null)
        {
            WindowBase windowBase = GetUnClosedWindow(path);
            if (windowBase != null && windowBase.WindowInfo.IsSinglen)
            {
                this.RemoveFromExitSquenceList(windowBase.GetSequence());
                this.AddToExitSquenceList(this.m_windowSequence);
                int openorder = this.GetWindowOpenOrder(this.m_windowSequence);
                windowBase.Appear(this.m_windowSequence, openorder, appear);
                this.m_windowSequence++;
                return windowBase;
            }
            GameObject obj = CreateWindow(path, isusePool);
            if (obj == null)
            {
                return null;
            }
            string name = GetWindowName(path);
            obj.name = name;
            if (obj.transform.parent != this.m_root.transform)
            {
                obj.transform.SetParent(m_root.transform);
            }
            windowBase = obj.GetComponent<WindowBase>();
            if (windowBase != null)
            {
                if (!windowBase.IsInitialized())
                {
                    AddCollider(windowBase);//添加遮罩
                    windowBase.Init(useCameraRender?m_UICamera:null);
                } 
                this.AddToExitSquenceList(this.m_windowSequence);
                int openorder = GetWindowOpenOrder(this.m_windowSequence);
                windowBase.Appear(this.m_windowSequence, openorder, appear);
                if (windowBase.WindowInfo.Group > 0)
                {
                    this.CloseGroupWindow(windowBase.WindowInfo.Group,false);
                }
                this.m_windows.Add(windowBase);
            }
            this.m_windowSequence++;
            return windowBase;
        }

        public void AddCollider(WindowBase windowBase)
        {
                Image image = null;
                Button button = null;
                GameObject go = null;
                switch (windowBase.WindowInfo.ColliderMode)
                {
                    case enWindowColliderMode.Node:
                        break;
                    case enWindowColliderMode.Dark:
                        go = new GameObject("DarkCollider", typeof(RectTransform), typeof(Image), typeof(Button));
                        image = go.GetComponent<Image>();
                        image.color = new Color(0, 0, 0, 100 / 255f);
                        image.raycastTarget = true;
                        button = go.GetComponent<Button>();
                        button.transition = Selectable.Transition.SpriteSwap;
                        button.targetGraphic = image;
                        button.onClick.AddListener(windowBase.ColliderCallBack);
                        break;
                    case enWindowColliderMode.Transparent:
                        go = new GameObject("ansparencyCollider", typeof(RectTransform), typeof(Image), typeof(Button));
                        image = go.GetComponent<Image>();
                        image.color = new Color(0, 0, 0, 0);
                        image.raycastTarget = true;
                        button = go.GetComponent<Button>();
                        button.transition = Selectable.Transition.SpriteSwap;
                        button.targetGraphic = image;
                        button.onClick.AddListener(windowBase.ColliderCallBack);
                        break;
                }
                if (go != null)
                {
                    var rectTran = go.GetComponent<RectTransform>();
                    rectTran.transform.SetParent(windowBase.CacheTransform);
                    rectTran.transform.SetSiblingIndex(0);
                    rectTran.localPosition = Vector3.zero;
                    rectTran.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTran.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTran.pivot = new Vector2(0.5f, 0.5f);
                    rectTran.sizeDelta = new Vector2(2000, 2000);
                }
        }
    }

}
