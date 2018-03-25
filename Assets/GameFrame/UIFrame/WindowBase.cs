﻿using System.Collections;
using System.Collections.Generic;
using UIFrameWork;
using UnityEngine;

namespace UIFrameWork
{
    
    public class /*abstract*/ WindowBase : MonoBehaviour {
        #region 缓存当前组件的gameobject 与 transform
    
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

        private WindowInfo _windowInfo = null;
        public WindowInfo windowInfo { set { _windowInfo = value; } get { return _windowInfo; } }
        private WindowInfo prevousWindowInfo = null;

        public WindowInfo PrevousWindowInfo
        {
            get { return prevousWindowInfo; }
            set { prevousWindowInfo = value; }
        }

        #endregion
        private UIState uiState = UIState.None;
        public event UIStateChangeEvent UIStateChanged;
        private bool _isActivied = false;
        public bool IsActivied
        {
            get { return _isActivied; }
        }
    
        /// <summary>
        /// 当设置UIState不同时触发事件回调函数
        /// </summary>
        public UIState State
        {
            get { return uiState; }
            set
            {
                if (value != uiState)
                {
                    UIState last = uiState;
                    uiState = value;
                    if (UIStateChanged != null)
                    {
                        UIStateChanged(this, value, last);
                    }
                }
            }
        }
        //抽象方法 子类重写
        //public abstract WindowType GetWindowType();
        protected void SetDepthToTop()
        {
            //CacheGameObject.transform.parent.
        }
    
        #region 调用Mono接口会相应调用虚方法
    
        void Awake()
        {
            OnAwake();
        }
    
        void Start()
        {
            OnStart();
        }
    
        void Update()
        {
            OnUpdate(Time.deltaTime);
        }
    
        void OnDestroy()
        {
            Destory();
        }
        protected virtual void OnAwake(){}
        protected virtual void OnStart(){}
        protected virtual void OnUpdate(float t){}
        protected virtual void Destory(){}
        #endregion
    
        #region 自定义界面相关事件
    
        public void Initantiate()
        {
            OnInitantiate();
        }
    
        public void Enter(WindowContext context = null)
        {
            if (IsActivied)
            {
                return;
            }
            this._CacheGameObject.SetActive(true);
            _isActivied = true;
            OnEnter(context);
        }
    
        public void Exit(WindowContext context = null)
        {
            OnExit(context);
            this._CacheGameObject.SetActive(false);
            this._isActivied = false;
        }
    
        public void Pause(WindowContext context = null)
        {
            if (this._isActivied == false)
            {
                return;
            }
            this._CacheGameObject.SetActive(false);
            this._isActivied = false;
            OnPause(context);
        }
    
        public void Resume(WindowContext context = null)
        {
            if (this._isActivied)
            {
                return;
            }
            this._CacheGameObject.SetActive(true);
            this._isActivied = true;
            OnResume(context);
        }
        protected virtual void OnInitantiate(){}
        protected virtual void OnEnter(WindowContext context){}
        protected virtual void OnExit(WindowContext context){}
        protected virtual void OnPause(WindowContext context){}
        protected virtual void OnResume(WindowContext context){}
        #endregion
    
        #region 定义界面功能接口
    
        public virtual void PlayEnterAnim(){}
        public virtual void PlayExitAnim(){}
        public virtual void PlayOpenMusic(){}
        public virtual void PlayExitMusic(){}
    
        #endregion

        /// <summary>
        /// collider 的回调方法
        /// </summary>
        public void ColliderCallBack()
        {
            OnColliderCallBack();
        }
        /// <summary>
        /// 如果需要自定义背景collider的处理时间 重写此方法即可
        /// </summary>
        protected  virtual void OnColliderCallBack(){}
    }

}
