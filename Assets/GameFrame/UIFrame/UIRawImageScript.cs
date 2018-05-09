using UnityEngine;

namespace UIFrameWork
{
    /// <summary>
    /// rawimage封装脚本 用于显示ui模型
    /// </summary>
    public class UIRawImageScript:UIComponent
    {
        private const int UIRawLayer = (int) enLayer2Int.UIRawLayer;
        private Camera m_renderCamera;
        private GameObject m_gameobject;
        public override void Init(WindowBase windowBase)
        {
            if (this.m_isInitialized)
            {
                return;
            }
            base.Init(windowBase);
            this.m_renderCamera = GetComponentInChildren<Camera>(gameObject);
            if (this.m_renderCamera != null)
            {
                Transform root = this.m_renderCamera.gameObject.transform.Find("RawRoott");
                if (root != null)
                {
                    this.m_gameobject = root.gameObject;
                }
            }
            
        }

        public override void OnDestory()
        {
            this.m_gameobject = null;
            this.m_renderCamera = null;
            base.OnDestory();
        }

        public override void Appear()
        {
            base.Appear();
            this.m_renderCamera.enabled = true;
            this.m_gameobject.SetActive(true);
        }

        public override void Close()
        {
            base.Close();
            this.m_renderCamera.enabled = false;
            this.m_gameobject.SetActive(false);
        }

        public override void Hide()
        {
            base.Hide();
            this.m_renderCamera.enabled = false;
            this.m_gameobject.SetActive(false);
        }

        public void AddModel(string name,GameObject obj,Vector3 position,Quaternion quaternion,Vector3 scaler)
        {
            if (obj == null || this.m_gameobject==null)
            {
                return;
            }
            SetModelLayer(obj,UIRawLayer);
            obj.name = name;
            obj.transform.SetParent(m_gameobject.transform);
            obj.transform.localPosition = position;
            obj.transform.localScale = scaler;
        }

        public GameObject GetModel(string name)
        {
            if (this.m_gameobject == null)
            {
                return null;
            }
            GameObject res = null;
            for (int i = 0; i < this.m_gameobject.transform.childCount; i++)
            {
                GameObject go = this.m_gameobject.transform.GetChild(i).gameObject;
                if (go.name == name)
                {
                    res = go;
                    break;
                }
            }
            return res;
        }

        public GameObject RemoveModel(string name)
        {
            if (this.m_gameobject == null)
            {
                return null;
            }
            GameObject res = null;
            res = GetModel(name);
            if (res != null)
            {
                res.transform.SetParent(null);
            }
            return res;
        }

        public void SetModelLayer(GameObject go,int layer)
        {
            go.layer = layer;
            for (int i = 0; i < go.transform.childCount; i++)
            {
                this.SetModelLayer(go.transform.GetChild(i).gameObject, layer);
            }
        }
    }
}