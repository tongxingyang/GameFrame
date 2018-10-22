using UnityEngine;

namespace UIFrameWork
{
    public class UIComponent:Component
    {
        [HideInInspector]
        public WindowBase m_belongedWindowBase;
        public bool m_isInitialized;

        public virtual void Init(WindowBase windowBase)
        {
            if (this.m_isInitialized)
            {
                return;
            }
            this.m_belongedWindowBase = windowBase;
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
    }
}