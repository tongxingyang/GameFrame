using System;
using System.Collections.Generic;
using GameFrame;
using GameFrameDebuger;
using UnityEngine;
using UnityEngine.UI;

namespace UIFrameWork
{
    public class  WindowBase : MonoBehaviour,IComparable
    {
        public WindowInfo WindowInfo;
        public Vector2 m_referenceResolution = GameConfig.Resolution;
        public bool m_isUsePool;
        public bool m_isModel;

        private bool m_isInitialized;
        private enWindowPriority m_defaultPriority;
        private bool m_isClosed;
        private bool m_isHided;
        private bool m_isActivied;
        private float m_closeAniClipTime;
        private float m_hideAnimClipTime;
        private Canvas m_canvas;
        private CanvasScaler m_canvasScaler;
        private GraphicRaycaster m_graphicRaycaster;
        private int m_openOrder;
        private int m_sortingOrder;
        private int m_sequence;
        private List<UIComponent> m_components;
        private enWindowState windowState = enWindowState.None;
        private WindowStateChangeEvent WindowStateChange;
        private bool m_isPlayOpen = false;
        private bool m_isPlayClose = false;
        private bool m_isPlayHide = false;

        [SerializeField] private AnimationClip m_openAnimClip;
        [SerializeField] private AnimationClip m_closeAnimClip;
        [SerializeField] private AnimationClip m_hideAnimClip;
        
        public bool IsPlayOpen
        {
            get { return m_isPlayOpen; }
            set { m_isPlayOpen = value; }
        }

        public bool IsPlayClose
        {
            get { return m_isPlayClose; }
            set { m_isPlayClose = value; }
        }
        public bool IsPlayHide
        {
            get { return m_isPlayHide; }
            set { m_isPlayHide = value; }
        }

        private Transform _CacheTransform;
        public Transform CacheTransform
        {
            get
            {
                if (_CacheTransform == null)
                {
                    _CacheTransform = this.transform;
                }
                return _CacheTransform;
            }
        }
        private GameObject _CacheGameObject;
        public GameObject CacheGameObject
        {
            get
            {
                if (_CacheGameObject == null)
                {
                    _CacheGameObject = this.gameObject;
                }
                return _CacheGameObject;
            }
        }
        public enWindowState WindowState
        {
            get { return windowState; }
            set
            {
                if (value != windowState)
                {
                    enWindowState last = WindowState;
                    WindowState = value;
                    if (WindowStateChange != null)
                    {
                        WindowStateChange(this, value, last);
                    }
                }
            }
        }

        #region 调用Mono接口会相应调用虚方法
        void Awake()
        {
            OnWindowAwake();
        }
    
        void Start()
        {
            OnWindowStart();
        }

        public void CustomUpdate()
        {
            if (m_isPlayClose)
            {
                if (m_closeAniClipTime > 0)
                {
                    m_closeAniClipTime -= Time.deltaTime;
                    if (m_closeAniClipTime <= 0)
                    {
                        m_closeAniClipTime = 0;
                        this.m_isPlayClose = false;
                        CloseWorker();
                    }
                }
            }
          
            if (m_isPlayHide)
            {
                if (m_hideAnimClipTime > 0)
                {
                    m_hideAnimClipTime -= Time.deltaTime;
                    if (m_hideAnimClipTime <= 0)
                    {
                        m_hideAnimClipTime = 0;
                        this.m_isPlayHide = false;
                        HideWorker();
                    }
                }
            }
           
            OnWindowCustomUpdate();
        }

        public void CustomLateUpdate()
        {
            OnWindowLateCustomUpdate();
        }

        void OnDestroy()
        {
            foreach (UIComponent uiComponent in m_components)
            {
                uiComponent.OnDestory();
            }
            this.m_components = null;
            OnWindowDestory();
        }
        
        protected virtual void OnWindowAwake(){}
        protected virtual void OnWindowStart(){}
        protected virtual void OnWindowCustomUpdate(){}
        protected virtual void OnWindowLateCustomUpdate(){}
        protected virtual void OnWindowDestory(){}
        #endregion
        
        #region 自定义界面周期函数

        public void Init(Camera UICamera)
        {
            if (this.m_isInitialized)
            {
                return;
            }
            this.InitializeCanvas();
            this.SetCanvasMode(UICamera);
            this.m_components = new List<UIComponent>();
            this.m_isClosed = false;
            this.m_isHided = false;
            this.m_isActivied = false;
            windowState= enWindowState.Init;
            this.m_defaultPriority = this.WindowInfo.Priority;
            this.InitUIComponent(gameObject);
            this.InitComponent();
            OnInit(UICamera);
            this.m_isInitialized = true;
            Debug.Log("<color=#FFFF00>" + "Init "+WindowInfo.Name +" ................."+ "</color>");
        }

        public void Appear( int sequence, int openOrder,WindowContext context)
        {
            if (!this.m_isInitialized)
            {
                return;
            }
            this.m_sequence = sequence;
            this.m_isPlayClose = false;
            this.m_isPlayHide = false;
            this.m_isPlayOpen = false;
            this.m_isHided = false;
            this.m_isClosed = false;
            this.m_isActivied = true;
            windowState= enWindowState.Appear;
            this.SetDisplayOrder(openOrder);
            PlayAppearMusic();
            if (m_openAnimClip != null)
            {
                var animation = GetComponent<Animation>();
                if (animation != null)
                {
                    animation.Play(m_openAnimClip.name);
                    this.m_isPlayOpen = true;
                }
                else
                {
                    Debuger.LogError("设置了OpenAniClip，但是未找到 Animation组件！");
                }
            }
            if (this.m_canvas != null)
            {
                this.m_canvas.enabled = true;
            }
            this.TryEnableInput(true);
            AppearComponent();
            OnAppear(sequence, openOrder,context);
            Debug.Log("<color=#FFFF00>" + "Appear "+WindowInfo.Name +" ................."+ "</color>");
        }

        public void Hide( bool force,WindowContext context)
        {
            if (this.WindowInfo.AlwaysKeepVisible && force==false)
            {
                return;
            }
            if (IsHided())
            {
                return;
            }
            this.m_isHided = true;
            this.m_isClosed = false;
            this.m_isActivied = false;
            windowState = enWindowState.Hide;
            PlayHideMusic();
            m_hideAnimClipTime = 0;
            this.m_isPlayClose = false;
            this.m_isPlayHide = false;
            this.m_isPlayOpen = false;
            this.TryEnableInput(false);
            OnHide(context);
            Singleton<WindowManager>.GetInstance().RecycleWindow(this.WindowInfo.Priority,this);
            if (m_hideAnimClip != null)
            {
                var animation = GetComponent<Animation>();
                if (animation != null)
                {
                    animation.Play(m_hideAnimClip.name);
                    m_hideAnimClipTime = m_hideAnimClip.length;
                    this.m_isPlayHide = true;
                }
                else
                {
                    Debuger.LogError("设置了CloseAniClip，但是未找到 Animation组件！");
                }
            }
            else
            {
                HideWorker();
            }

            Debug.Log("<color=#FFFF00>" + "Hide " + WindowInfo.Name + " ................." + "</color>");
        }

        public void HideWorker()
        {
            HideComponent();
            if (this.m_canvas != null)
            {
                this.m_canvas.enabled = false;
            }
            //this.gameObject.SetActive(false);
        }

        public void Close(bool force ,WindowContext context)
        {
            if (this.WindowInfo.AlwaysKeepVisible && force==false)
            {
                return;
            }
            if (IsClosed())
            {
                return;
            }
            this.m_isHided = true;
            this.m_isClosed = true;
            this.m_isActivied = false;
            this.m_isPlayClose = false;
            this.m_isPlayHide = false;
            this.m_isPlayOpen = false;
            windowState = enWindowState.Close;
            PlayCloseMusic();
            OnClose(context);
            Singleton<WindowManager>.GetInstance().RecycleWindow(this.WindowInfo.Priority,this);
            this.m_closeAniClipTime = 0;
            if (m_closeAnimClip != null)
            {
                var animation = GetComponent<Animation>();
                if (animation != null)
                {
                    animation.Play(m_closeAnimClip.name);
                    m_closeAniClipTime = m_closeAnimClip.length;
                    this.m_isPlayClose = true;
                }
                else
                {
                    Debuger.LogError("设置了CloseAniClip，但是未找到 Animation组件！");
                }
            }
            else
            {
                CloseWorker();
            }
            Debug.Log("<color=#FFFF00>" + "Close " + WindowInfo.Name + " ................." + "</color>");
        }

        public void CloseWorker()
        {
            CloseComponent();
            //this.gameObject.SetActive(false);
            DestroyImmediate(CacheGameObject);
        }

        protected virtual void OnInit(Camera UICamera){}
        protected virtual void OnAppear(int sequence, int openOrder,WindowContext context){}
        protected virtual void OnHide(WindowContext context){}
        protected virtual void OnClose(WindowContext context){}

        #endregion
    
        #region 定义界面功能接口
    
        public virtual void PlayAppearMusic(){}
        public virtual void PlayHideMusic(){}
        public virtual void PlayCloseMusic(){}
    
        public void ColliderCallBack(GameObject go)
        {
            OnColliderCallBack();
        }
        protected  virtual void OnColliderCallBack(){}
        #endregion


        public int CompareTo(object obj)
        {
            WindowBase windowbase = obj as WindowBase;
            if (this.m_sortingOrder > windowbase.m_sortingOrder)
            {
                return 1;
            }
            if (this.m_sortingOrder == windowbase.m_sortingOrder)
            {
                return 0;
            }
            return -1;
        }

        public virtual void InitializeCanvas()
        {
            this.m_canvas = CacheGameObject.GetComponent<Canvas>();
            this.m_canvasScaler = CacheGameObject.GetComponent<CanvasScaler>();
            this.m_graphicRaycaster = CacheGameObject.GetComponent<GraphicRaycaster>();
            this.MatchScreen();
        }

        public void MatchScreen()
        {
            if (this.m_canvasScaler == null)
            {
                return;
            }
            this.m_canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            this.m_canvasScaler.referenceResolution= this.m_referenceResolution;
            this.m_canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            if ((float)Screen.width / this.m_canvasScaler.referenceResolution.x > (float)Screen.height / this.m_canvasScaler.referenceResolution.y)
            {
                if (this.WindowInfo.FullScreenBG)
                {
                    this.m_canvasScaler.matchWidthOrHeight = 0;
                }
                else
                {
                    this.m_canvasScaler.matchWidthOrHeight = 1;
                }
            }
            else if (this.WindowInfo.FullScreenBG)
            {
                this.m_canvasScaler.matchWidthOrHeight = 1;
            }
            else
            {
                this.m_canvasScaler.matchWidthOrHeight = 0;
            }
            
            if (this.m_canvasScaler != null)
            {
                this.m_canvasScaler.enabled = false;
                this.m_canvasScaler.enabled = true;
            }
            if (this.m_graphicRaycaster)
            {
                this.m_graphicRaycaster.enabled = !this.WindowInfo.DisableInput;
            }
        }

        public void ResetEventInput()
        {
            if (this.m_graphicRaycaster)
            {
                this.m_graphicRaycaster.enabled = !this.WindowInfo.DisableInput;
            }
        }

        public void SetCanvasMode(Camera camera)
        {
            if (this.m_canvas != null)
            {
                if (camera == null)
                {
                    this.m_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                }
                else 
                {
                    this.m_canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    this.m_canvas.worldCamera = camera;
                }
                this.m_canvas.pixelPerfect = false;
            }
        }

        #region 获取相关属性

        public int GetSequence()
        {
            return this.m_sequence;
        }

        public int GetSortingOrder()
        {
            return this.m_sortingOrder;
        }

        public int GetOpenOrder()
        {
            return this.m_openOrder;
        }

        public bool IsInitialized()
        {
            return this.m_isInitialized;
        }

        public bool IsClosed()
        {
            return m_isClosed;
        }

        public bool IsHided()
        {
            return m_isHided;
        }

        public bool IsActivied()
        {
            return m_isActivied;
        }

        public GraphicRaycaster GetGraphicRaycaster()
        {
            return this.m_graphicRaycaster;
        }

        public Camera GetCamera()
        {
            if (this.m_canvas == null || this.m_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return null;
            }
            return this.m_canvas.worldCamera;
        }

        public Vector2 GetReferenceResolution()
        {
            return (this.m_canvasScaler == null) ? Vector2.zero : this.m_canvasScaler.referenceResolution;
        }

        #endregion

        public void SetDisplayOrder(int openorder)
        {
            if (openorder <= 0)
            {
                Debuger.LogError("error openorder小于0");
                return;
            }
            this.m_openOrder = openorder;
            if (this.m_canvas != null)
            {
                this.m_sortingOrder = CalcSortingOrder(this.WindowInfo.Priority, openorder);
                this.m_canvas.sortingOrder = this.m_sortingOrder;
                if (this.m_canvas.enabled)
                {
                    this.m_canvas.enabled = false;
                    this.m_canvas.enabled = true;
                }
            }
            this.SetUIComponentSortingOrder(this.m_sortingOrder);
        }

        private void SetUIComponentSortingOrder(int order)
        {
            for (int i = 0; i < this.m_components.Count; i++)
            {
                this.m_components[i].SetSortingOrder(order);
            }
        }

        public int CalcSortingOrder(enWindowPriority priority, int openorder)
        {
            if (openorder >= 100)
            {
                openorder %= 100;
            }
            return (int) ((this.IsOverlay() ? 10000 : 0) + (int) priority * 1000 + openorder * 10);
        }

        public bool IsOverlay()
        {
            return !(this.m_canvas == null) && (this.m_canvas.renderMode == RenderMode.ScreenSpaceOverlay);
        }

        public bool IsCanvasEnabled()
        {
            return !(this.m_canvas == null) && this.m_canvas.enabled;
        }

        public void InitUIComponent(GameObject go)
        {
            UIComponent component = go.GetComponent<UIComponent>();
            if (component != null)
            {
                AddUIComponent(component);    
            }
            for (int i = 0; i < go.transform.childCount; i++)
            {
                InitUIComponent(go.transform.GetChild(i).gameObject);
            }
        }

        public void AddUIComponent(UIComponent component)
        {
            if (component != null && !this.m_components.Contains(component))
            {
                this.m_components.Add(component);
            }
        }

        public void RemoveUIComponent(UIComponent component)
        {
            if (this.m_components.Contains(component))
            {
                this.m_components.Remove(component);
            }
        }

        public void InitComponent()
        {
            for (int i = 0; i < this.m_components.Count; i++)
            {
                this.m_components[i].Init(this);
            }
        }

        public void CloseComponent()
        {
            for (int i = 0; i < this.m_components.Count; i++)
            {
                this.m_components[i].Close();
            }
        }

        public void HideComponent()
        {
            for (int i = 0; i < this.m_components.Count; i++)
            {
                this.m_components[i].Hide();
            }
        }

        public void AppearComponent()
        {
            for (int i = 0; i < this.m_components.Count; i++)
            {
                this.m_components[i].Appear();
            }
        }

        public void SetPriority(enWindowPriority priority)
        {
            if (this.WindowInfo.Priority == priority)
            {
                return;
            }
            this.WindowInfo.Priority = priority;
            this.SetDisplayOrder(this.m_openOrder);
            Singleton<WindowManager>.GetInstance().UpdateSortingOrder();
        }

        public void ResetPriority()
        {
            SetPriority(this.m_defaultPriority);
        }

        public void TryEnableInput(bool isEnable)
        {
            if (this.m_graphicRaycaster == null)
            {
                return;
            }
            if (!isEnable)
            {
                this.m_graphicRaycaster.enabled = false;
            }
            else if (!this.WindowInfo.DisableInput)
            {
                this.m_graphicRaycaster.enabled = true;
            }
        }
        
    }
}