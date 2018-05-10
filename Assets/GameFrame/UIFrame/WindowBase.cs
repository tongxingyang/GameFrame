using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UIFrameWork;
using UnityEngine;
using UnityEngine.UI;

namespace UIFrameWork
{
    /// <summary>
    /// 窗口基础类
    /// </summary>
    public class  WindowBase : MonoBehaviour,IComparable
    {
        /// <summary>
        /// 窗口信息
        /// </summary>
        public WindowInfo WindowInfo;
        /// <summary>
        /// canvas设计分辨率
        /// </summary>
        public Vector2 m_referenceResolution = new Vector2(960f,640f);
        /// <summary>
        /// 是否使用缓存
        /// </summary>
        public bool m_isUsePool;
        /// <summary>
        /// 是否渲染ui模型
        /// </summary>
        public bool m_isModel;
        private bool m_isInitialized;
        private enWindowPriority m_defaultPriority;
        private bool m_isClosed;
        private bool m_isHided;
        private bool m_isActivied;
        /// <summary>
        /// 当前界面的canvas
        /// </summary>
        private Canvas m_canvas;
        /// <summary>
        /// 当前界面的缩放组件
        /// </summary>
        public CanvasScaler m_canvasScaler;
        /// <summary>
        /// 当前界面的m_graphicRaycaster
        /// </summary>
        private GraphicRaycaster m_graphicRaycaster;
        /// <summary>
        /// 窗口的openorder
        /// </summary>
        private int m_openOrder;
        /// <summary>
        /// 窗口的sortingorder
        /// </summary>
        private int m_sortingOrder;
        /// <summary>
        /// 窗口的打开序列 根据此回去相对openorder
        /// </summary>
        private int m_sequence;

        /// <summary>
        /// 窗口隐藏标志
        /// </summary>
        private int m_hideFlags;

        /// <summary>
        /// 打开时是否隐藏下面的界面
        /// </summary>
        public bool m_hideUnderUIs;
        /// <summary>
        /// 窗口是否可以接受事件
        /// </summary>
        public bool m_enableInput;
        private List<UIComponent> m_components;
        private List<GameObject> m_relatedScens;
        private List<List<Camera>> m_relatedCamera;
        private enWindowState windowState = enWindowState.None;
        public WindowStateChangeEvent WindowStateChange;

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
        /// <summary>
        /// 由manager统一调用update
        /// </summary>
        void CustomUpdate()
        {
            OnWindowCustomUpdate();
        }
        /// <summary>
        /// 由manager统一调用lateupdate
        /// </summary>
        void CustomLateUpdate()
        {
            OnWindowLateCustomUpdate();
        }
        void OnDestroy()
        {
            this.m_components = null;
            this.m_relatedScens = null;
            this.m_relatedCamera = null;
            OnWindowDestory();
        }
        protected virtual void OnWindowAwake(){}
        protected virtual void OnWindowStart(){}
        protected virtual void OnWindowCustomUpdate(){}
        protected virtual void OnWindowLateCustomUpdate(){}
        protected virtual void OnWindowDestory(){}
        #endregion
        
        #region 自定义界面周期函数
        /// <summary>
        /// 初始化  获取组件相关处理
        /// </summary>
        public void Init(Camera UICamera, int sequence, WindowContext context)
        {
            if (this.m_isInitialized)
            {
                return;
            }
            this.InitializeCanvas();
            this.SetCanvasMode(UICamera);
            this.gameObject.SetActive(true);
            this.m_components = new List<UIComponent>();
            this.m_relatedScens = new List<GameObject>();
            this.m_relatedCamera = new List<List<Camera>>();
            this.m_isClosed = true;
            this.m_isHided = true;
            this.m_isActivied = false;
            windowState= enWindowState.Init;
            this.m_defaultPriority = this.WindowInfo.Priority;
            this.m_sequence = sequence;
            this.InitUIComponent(gameObject);
            this.InitComponent();
            OnInit(UICamera, sequence,context);
            this.m_isInitialized = true;
            
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public void Appear(enWindowHideFlag hideFlag , bool dispatchVisibleChangedEvent , bool exist, int openOrder,WindowContext context)
        {            
            if (IsActivied() == true)
            {
                return;
            }
            this.m_hideFlags &= (int)(~(int)hideFlag);
            if (this.m_hideFlags != 0 || !this.m_isHided)
            {
                return;
            }
            this.gameObject.SetActive(true);
            this.m_isHided = false;
            this.m_isClosed = false;
            this.m_isActivied = true;
            windowState= enWindowState.Appear;
            this.SetDisplayOrder(openOrder);
            PlayAppearAnim();
            PlayAppearMusic();
            if (!exist)
            {
                //派发窗口打开事件
                this.DispatchWindowEvent(enWindowEventType.Open);
            }
            if (this.m_canvas != null)
            {
                this.m_canvas.enabled = true;
                this.m_canvas.sortingOrder = this.m_sortingOrder;
            }
            this.TryEnableInput(true);
            for (int i = 0; i < this.m_relatedScens.Count; i++)
            {
                this.m_relatedScens[i].SetActive(true);
                this.SetSceneCameraEnable(i, true);
            }
            AppearComponent();
            this.DispatchRevertVisibleWindowEvent();
            if (dispatchVisibleChangedEvent)
            {
                this.DispatchVisibleChangedEvent();
            }
            OnAppear(hideFlag, dispatchVisibleChangedEvent,exist, openOrder,context);
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public void Hide(enWindowHideFlag hideFlag , bool dispatchVisibleChangedEvent ,WindowContext context)
        {
            if (this.WindowInfo.AlwaysKeepVisible)
            {
                return;
            }
            if (IsHided() == true)
            {
                return;
            }
            this.m_hideFlags |= (int)hideFlag;
            if (this.m_hideFlags == 0 || this.m_isHided)
            {
                return;
            }
            this.gameObject.SetActive(false);
            this.m_isHided = true;
            this.m_isClosed = false;
            this.m_isActivied = false;
            windowState= enWindowState.Hide;
            HideComponent();
            PlayHideAnim();
            PlayHideMusic();
            if (this.m_canvas != null)
            {
                this.m_canvas.enabled = false;
            }
            this.TryEnableInput(false);
            for (int i = 0; i < this.m_relatedScens.Count; i++)
            {
                //this.m_relatedScens[i].GetComponent<WindowBase>().Hide(hideFlag,dispatchVisibleChangedEvent,context);
                this.m_relatedScens[i].SetActive(false);
                this.SetSceneCameraEnable(i, false);
            }
            if (dispatchVisibleChangedEvent)
            {
                //派发事件
                this.DispatchVisibleChangedEvent();
            }
            OnHide(hideFlag,dispatchVisibleChangedEvent,context);
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public void Close(WindowContext context)
        {
            if (this.WindowInfo.AlwaysKeepVisible)
            {
                return;
            }
            if (IsClosed() == true)
            {
                return;
            }
            this.gameObject.SetActive(false);
            this.m_isHided = true;
            this.m_isClosed = true;
            this.m_isActivied = false;
            windowState= enWindowState.Close;
            CloseComponent();
            PlayCloseAnim();
            PlayCloseMusic();
            OnClose(context);
        }

        protected virtual void OnInit(Camera UICameraint, int sequence, WindowContext context){}
        protected virtual void OnAppear(enWindowHideFlag hideFlag, bool dispatchVisibleChangedEvent, bool exist, int openOrder,WindowContext context){}
        protected virtual void OnHide(enWindowHideFlag hideFlag , bool dispatchVisibleChangedEvent ,WindowContext context){}
        protected virtual void OnClose(WindowContext context){}

        #endregion
    
        #region 定义界面功能接口
    
        public virtual void PlayAppearAnim(){}
        public virtual void PlayHideAnim(){}
        public virtual void PlayCloseAnim(){}
        public virtual void PlayAppearMusic(){}
        public virtual void PlayHideMusic(){}
        public virtual void PlayCloseMusic(){}
    
        /// <summary>
        /// collider 的回调方法
        /// </summary>
        public void ColliderCallBack()
        {
            OnColliderCallBack();
        }
        /// <summary>
        /// 如果需要自定义collider的处理事件 重写此方法即可
        /// </summary>
        protected  virtual void OnColliderCallBack(){}
        #endregion

       
        /// <summary>
        /// 根据sortingorder从小到大排序
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 初始化窗口canvas节点
        /// </summary>
        public virtual void InitializeCanvas()
        {
            this.m_canvas = CacheGameObject.GetComponent<Canvas>();
            this.m_canvasScaler = CacheGameObject.GetComponent<CanvasScaler>();
            this.m_graphicRaycaster = CacheGameObject.GetComponent<GraphicRaycaster>();
            this.MatchScreen();
        }
        /// <summary>
        /// 窗口的适配方案
        /// </summary>
        public void MatchScreen()
        {
            if (this.m_canvasScaler == null)
            {
                return;
            }
            this.m_canvasScaler.referenceResolution= this.m_referenceResolution;
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
        }

        public void SetCanvasMode(Camera camera)
        {
            if (this.m_canvas != null)
            {
                this.m_canvas.worldCamera = camera;
                //设置界面的渲染方式
                if (camera == null)
                {
                    if (this.m_canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                    {
                        this.m_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    }
                }
                else if (this.m_canvas.renderMode != RenderMode.ScreenSpaceCamera)
                {
                    this.m_canvas.renderMode = RenderMode.ScreenSpaceCamera;
                }
                this.m_canvas.pixelPerfect = true;
            }
            //刷新缩放显示
            if (this.m_canvasScaler != null)
            {
                this.m_canvasScaler.enabled = false;
                this.m_canvasScaler.enabled = true;
            }
            if (this.m_graphicRaycaster)
            {
                this.m_graphicRaycaster.enabled = this.m_enableInput;
            }
        }
        #region 获取相关属性

        /// <summary>
        /// 获取窗口的打开序列
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// 设置显示层级 设置UIcomponent的sortingorder
        /// </summary>
        /// <param name="openorder"></param>
        public void SetDisplayOrder(int openorder)
        {
            if (openorder <= 0)
            {
                Debug.LogError("error openorder小于0");
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
        /// <summary>
        /// 设置组件order
        /// </summary>
        /// <param name="order"></param>
        private void SetUIComponentSortingOrder(int order)
        {
            for (int i = 0; i < this.m_components.Count; i++)
            {
                this.m_components[i].SetSortingOrder(order);
            }
        }
        /// <summary>
        /// 计算sortingorder
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="openorder"></param>
        /// <returns></returns>
        public int CalcSortingOrder(enWindowPriority priority, int openorder)
        {
            if (openorder >= 100)
            {
                openorder %= 100;
            }
            return (int) ((this.IsOverlay() ? 10000 : 0) + (int) priority * 1000 + openorder * 10);
        }
        /// <summary>
        /// 判断canvas的渲染模式
        /// </summary>
        /// <returns></returns>
        public bool IsOverlay()
        {
            return !(this.m_canvas == null) && (this.m_canvas.renderMode == RenderMode.ScreenSpaceOverlay);
        }
        /// <summary>
        /// 获取canvas是否可用
        /// </summary>
        /// <returns></returns>
        public bool IsCanvasEnabled()
        {
            return !(this.m_canvas == null) && this.m_canvas.enabled;
        }
        /// <summary>
        /// 初始化component组件
        /// </summary>
        /// <param name="go"></param>
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
        /// <summary>
        /// 添加组件
        /// </summary>
        /// <param name="component"></param>
        public void AddUIComponent(UIComponent component)
        {
            if (component != null && !this.m_components.Contains(component))
            {
                this.m_components.Add(component);
            }
        }
        /// <summary>
        /// 移除组件
        /// </summary>
        /// <param name="component"></param>
        public void RemoveUIComponent(UIComponent component)
        {
            if (this.m_components.Contains(component))
            {
                this.m_components.Remove(component);
            }
        }
        /// <summary>
        /// 初始化component
        /// </summary>
        public void InitComponent()
        {
            for (int i = 0; i < this.m_components.Count; i++)
            {
                this.m_components[i].Init(this);
            }
        }
        /// <summary>
        /// 关闭component 
        /// </summary>
        public void CloseComponent()
        {
            for (int i = 0; i < this.m_components.Count; i++)
            {
                this.m_components[i].Close();
            }
        }
        /// <summary>
        /// hidecomponent 
        /// </summary>
        public void HideComponent()
        {
            for (int i = 0; i < this.m_components.Count; i++)
            {
                this.m_components[i].Hide();
            }
        }
        /// <summary>
        /// appear component 
        /// </summary>
        public void AppearComponent()
        {
            for (int i = 0; i < this.m_components.Count; i++)
            {
                this.m_components[i].Appear();
            }
        }
        /// <summary>
        /// 设置隐藏下面的窗口属性
        /// </summary>
        /// <param name="isHideUnderForm"></param>
        public void SetHideUnderForm(bool isHideUnderForm)
        {
            this.m_hideUnderUIs = isHideUnderForm;
            //Singleton<CUIManager>.instance.ResetAllFormHideOrShowState(); todo
        }
        /// <summary>
        /// 获取当前屏幕适配的缩放值
        /// </summary>
        /// <returns></returns>
        public float GetScreenScaleValue()
        {
            float result = 1f;
            RectTransform component = base.GetComponent<RectTransform>();
            if (component && (this.m_canvasScaler.matchWidthOrHeight >= 0f && this.m_canvasScaler.matchWidthOrHeight < 0.01f ))
            {
                result = component.rect.width / component.rect.height / (this.m_canvasScaler.referenceResolution.x / this.m_canvasScaler.referenceResolution.y);
            }
            return result;
        }
        /// <summary>
        /// 设置优先级
        /// </summary>
        /// <param name="priority"></param>
        public void SetPriority(enWindowPriority priority)
        {
            if (this.WindowInfo.Priority == priority)
            {
                return;
            }
            this.WindowInfo.Priority = priority;
            this.SetDisplayOrder(this.m_openOrder);
            this.DispatchChangeWindowPriorityEvent();
        }
        /// <summary>
        /// 默认优先级
        /// </summary>
        public void ResetPriority()
        {
            SetPriority(this.m_defaultPriority);
        }
        /// <summary>
        /// 设置是否可以接受点击事件
        /// </summary>
        /// <param name="isEnable"></param>
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
            else if (isEnable && this.m_enableInput)
            {
                this.m_graphicRaycaster.enabled = true;
            }
        }
        /// <summary>
        /// 添加关联场景
        /// </summary>
        public void AddRelatedScene(GameObject scene,string sceneName)
        {
            scene.name = sceneName;
            this.m_relatedScens.Add(scene);
            this.m_relatedCamera.Add(new List<Camera>());
            this.AddRelatedSceneCamera(this.m_relatedCamera.Count - 1, scene);
        }
        /// <summary>
        /// 添加关联的摄像机
        /// </summary>
        /// <param name="index"></param>
        /// <param name="go"></param>
        public void AddRelatedSceneCamera(int index, GameObject go)
        {
            if (index < 0 || index >= this.m_relatedCamera.Count || go == null)
            {
                return;
            }
            Camera component = go.GetComponent<Camera>();
            if (component != null)
            {
                this.m_relatedCamera[index].Add(component);
            }
            for (int i = 0; i < go.transform.childCount; i++)
            {
                this.AddRelatedSceneCamera(index, go.transform.GetChild(i).gameObject);
            }
        } 
        /// <summary>
        /// 关联界面会否已经存在
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        public bool IsRelatedSceneExist(string sceneName)
        {
            for (int i = 0; i < this.m_relatedScens.Count; i++)
            {
                if (string.Equals(sceneName, this.m_relatedScens[i].name))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// /设置当前界面关联camera的可见
        /// </summary>
        /// <param name="index"></param>
        /// <param name="bEnable"></param>
        public void SetSceneCameraEnable(int index, bool bEnable)
        {
            if (index < 0 || index >= this.m_relatedCamera.Count || this.m_relatedCamera[index] == null)
            {
                return;
            }
            for (int i = 0; i < this.m_relatedCamera[index].Count; i++)
            {
                if (this.m_relatedCamera[index][i] != null)
                {
                    this.m_relatedCamera[index][i].enabled = bEnable;
                }
            }
        }
      
        #region 派发函数 跟事件派发整合

        private void DispatchWindowEvent(enWindowEventType windowEventType)
        {
            //todo
        }

        private void DispatchChangeWindowPriorityEvent()
        {
            //todo
        }

        private void DispatchVisibleChangedEvent()
        {
            //todo
        }

        private void DispatchRevertVisibleWindowEvent()
        {
            //todo
        }

        #endregion
        
        
    }
}
    

