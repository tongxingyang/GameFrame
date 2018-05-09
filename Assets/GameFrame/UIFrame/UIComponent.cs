using UnityEngine;

namespace UIFrameWork
{
    /// <summary>
    /// rawimage 粒子 基类
    /// </summary>
    public class UIComponent:Component
    {
        [HideInInspector]
        public WindowBase m_belongedWindowBase;
        /// <summary>
        /// 当前是否已经初始化了
        /// </summary>
        public bool m_isInitialized;

        public virtual void Init(WindowBase windowBase)
        {
            if (this.m_isInitialized)
            {
                return;
            }
            this.m_belongedWindowBase = windowBase;
            if (this.m_belongedWindowBase != null)
            {
                this.SetSortingOrder(this.m_belongedWindowBase.GetSortingOrder());
            }
            this.m_isInitialized = true;
        }

        public virtual void Appear()
        {
            
        }

        public virtual void Hide()
        {
            
        }

        public virtual void Close()
        {
            
        }

        public virtual void OnDestory()
        {
            this.m_belongedWindowBase = null;
        }

        public virtual void SetSortingOrder(int sortingOrder)
        {
            
        }

        protected T GetComponentInChildren<T>(GameObject go) where T : Component
        {
            T t = go.GetComponent<T>();
            if (t != null)
            {
                return t;
            }
            for (int i = 0; i < go.transform.childCount; i++)
            {
                t = this.GetComponentInChildren<T>(go.transform.GetChild(i).gameObject);
                if (t != null)
                {
                    return t;
                }
            }
            return (T)((object)null);
        }
    }
}