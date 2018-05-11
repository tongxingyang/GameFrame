using System.Collections.Generic;
using GameFrame;
using UIFrameWork;
using UnityEditor;
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
        private bool m_needSort;
        private bool m_needUpdateHide;

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
            //监听相关时间消息处理
            Singleton<EventManager>.GetInstance().AddEventListener(enEventID.UI_OnFormPriorityChanged, new EventManager.OnEventHandler(this.OnFormPriorityChanged));
            Singleton<EventManager>.GetInstance().AddEventListener(enEventID.UI_OnFormVisibleChanged, new EventManager.OnEventHandler(this.OnFormVisibleChanged));
        }
        //事件接受函数
        private void OnFormPriorityChanged(GameFrame.Event @event)
        {
            this.m_needSort = true;
        }

        private void OnFormVisibleChanged(GameFrame.Event @event)
        {
            this.m_needUpdateHide = true;
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
            camera.cullingMask = 32;// todo
            this.m_UICamera = camera;
        }
        
        public void Update()
        {
            for (int i = 0; i < this.m_windows.Count; i++)
            {
                this.m_windows[i].CustomUpdate();
                if (this.m_windows[i].IsClosed())
                {
                    this.RecycleWindow(i);
                    this.m_needSort = true;
                }
            }
            if (this.m_needSort)
            {
                this.ProcessWindowList(true, true);
            }else if (this.m_needUpdateHide)
            {
                this.ProcessWindowList(false, true);
            }
            this.m_needSort = false;
            this.m_needUpdateHide = false;
        }
        private void ProcessWindowList(bool sort, bool handleInputAndHide)
        {
            if (sort)
            {
                this.m_windows.Sort();//m_sortingOrder从小到大 
                for (int i = 0; i < this.m_windows.Count; i++)
                {
                    int openorder = this.GetWindowOpenOrder(this.m_windows[i].GetSequence());
                    this.m_windows[i].SetDisplayOrder(openorder);
                }
            }
            if (handleInputAndHide)
            {
                this.UpdateWindowHided();
                //this.UpdateWindowRaycaster();
            }
            if (this.OnWindowSorted != null)
            {
                this.OnWindowSorted(this.m_windows);
            }
        }

        private void UpdateWindowHided()
        {
            bool flag = false;
            for (int i = this.m_windows.Count - 1; i >= 0; i--)
            {
                if (flag)
                {
                    this.m_windows[i].Hide(false,false,null);
                }
                else
                {
                    int openorder = this.GetWindowOpenOrder(this.m_windows[i].GetSequence());
                    this.m_windows[i].Appear(false,openorder,null);
                }
                if (!flag && !this.m_windows[i].IsHided() && this.m_windows[i].m_hideUnderUIs)
                {
                    flag = true;
                }
            }
        }

        private void UpdateWindowRaycaster()
        {
            for (int i = this.m_windows.Count - 1; i >= 0; i--)
            {
                GraphicRaycaster graphicRaycaster = this.m_windows[i].GetGraphicRaycaster();
                if (this.m_windows[i].m_enableInput && !this.m_windows[i].IsHided())
                {
                    if (graphicRaycaster != null)
                    {
                        graphicRaycaster.enabled = true;
                    }
                }
                else
                {
                    if (graphicRaycaster != null)
                    {
                        graphicRaycaster.enabled = false;
                    }
                }
            }
        }
        public string GetWindowName(string path)
        {
            string[] arr = path.Split('/');
            return arr[arr.Length - 1];
        }
        public void LateUpdate()
        {
            for (int i = 0; i < m_windows.Count; i++)
            {
                m_windows[i].CustomLateUpdate();
            }
        }

        public void CloseWindow(bool isforce,WindowBase windowBase)
        {
            for (int i = 0; i < m_windows.Count; i++)
            {
                if (this.m_windows[i] == windowBase)
                {
                    if (this.m_windows[i].m_isUsePool)
                    {
                        this.m_windows[i].Hide(true,isforce,null);
                    }
                    else
                    {
                        this.m_windows[i].Close(isforce,null);
                    }
                }
            }
        }
        public void CloseWindow(bool isforce,string path)
        {
            for (int i = 0; i < m_windows.Count; i++)
            {
                if (this.m_windows[i].WindowInfo.PerfabPath == path)
                {
                    if (this.m_windows[i].m_isUsePool)
                    {
                        this.m_windows[i].Hide(true,isforce,null);
                    }
                    else
                    {
                        this.m_windows[i].Close(isforce,null);
                    }
                }
            }
        }
        public void CloseWindow(bool isforce,int sque)
        {
            for (int i = 0; i < m_windows.Count; i++)
            {
                if (this.m_windows[i].GetSequence() == sque)
                {
                    if (this.m_windows[i].m_isUsePool)
                    {
                        this.m_windows[i].Hide(true,isforce,null);
                    }
                    else
                    {
                        this.m_windows[i].Close(isforce,null);
                    }
                }
            }
        }

        public void CloseAllWindow(bool closeImmediated = true,bool clearPool = true,bool isforce = false)
        {
            for (int i = 0; i < this.m_windows.Count; i++)
            {
                if (this.m_windows[i].m_isUsePool)
                {
                    this.m_windows[i].Hide(true,isforce,null);
                }
                else
                {
                    this.m_windows[i].Close(isforce,null);
                }
            }
            if (closeImmediated)
            {
                int k = 0;
                while (k<this.m_windows.Count)
                {
                    if (m_windows[k].IsHided())
                    {
                        RecycleWindow(k);
                    }
                    else
                    {
                        k++;
                    }
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
                        this.m_windows[i].Hide(true,isforce,null);
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
                return 0;
            }
        }

        private void RecycleWindow(WindowBase windowBase,bool isforce= false)
        {
            if (windowBase == null)
            {
                return;
            }
            if (windowBase.m_isUsePool)
            {
                windowBase.Hide(true,isforce, null);
                this.m_pooledWindows.Add(windowBase);
            }
            else
            {
                if (windowBase.m_canvasScaler != null)
                {
                    windowBase.m_canvasScaler.enabled = false;
                }
                Object.DestroyImmediate(windowBase.CacheGameObject);
            } 
        }

        private void RecycleWindow(int index)
        {
            this.RemoveFromExitSquenceList(this.m_windows[index].GetSequence());
            this.RecycleWindow(this.m_windows[index]);
            this.m_windows.RemoveAt(index);
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
                obj = GameObject.Instantiate(res);
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

        public WindowBase OpenWindow(string path,bool isusePool,bool useCameraRender = true)
        {
            WindowBase windowBase = GetUnClosedWindow(path);
            if (windowBase != null && windowBase.WindowInfo.IsSinglen)
            {
                this.RemoveFromExitSquenceList(windowBase.GetSequence());
                this.AddToExitSquenceList(this.m_windowSequence);
                int openorder = this.GetWindowOpenOrder(this.m_windowSequence);
                windowBase.Appear(true, openorder, null);
                this.m_windowSequence++;
                this.m_needSort = true;
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
                    windowBase.Init(useCameraRender?m_UICamera:null,m_windowSequence,null);
                } 
                this.AddToExitSquenceList(this.m_windowSequence);
                int openorder = GetWindowOpenOrder(this.m_windowSequence);
                windowBase.Appear(true, openorder, null);
                if (windowBase.WindowInfo.Group > 0)
                {
                    this.CloseGroupWindow(windowBase.WindowInfo.Group,false);
                }
                this.m_windows.Add(windowBase);
            }
            this.m_windowSequence++;
            this.m_needSort = true;
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
                        go = new GameObject("ransparencyCollider", typeof(RectTransform), typeof(Image), typeof(Button));
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
