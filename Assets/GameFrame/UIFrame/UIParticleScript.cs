﻿using System.Collections.Generic;
using GameFrame;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UIFrameWork
{
    /// <summary>
    /// 封装的ui粒子脚本
    /// </summary>
    public class UIParticleScript:UIComponent
    {
        private const int UIParticleLayer = (int) enLayer2Int.UIParticleLayer;
        public string m_resPath = string.Empty;
        public List<Renderer> m_renderList = new List<Renderer>();
        private Dictionary<Transform,ScaleData> scaleData = new Dictionary<Transform, ScaleData>();
        public int index = 0;//相对于界面的排序值
        public GameObject PartObj = null;
        public override void Init(WindowBase windowBase)
        {
            if (this.m_isInitialized)
            {
                return;
            }
            //如果资源名不为空就加在特效
            if (this.m_resPath != string.Empty)
            {
                GameObject obj =Resources.Load<GameObject>(this.m_resPath);
                if (obj != null && gameObject.transform.childCount == 0)
                {
                    GameObject ins = Object.Instantiate(obj) as GameObject;
                    ins.transform.SetParent(gameObject.transform);
                    ins.transform.localPosition = Vector3.zero;
                    ins.transform.localRotation = Quaternion.identity;
                    ins.transform.localScale = Vector3.one;
                    PartObj = ins;
                }
            }
            //获取特效的所有render组件
            InitRender(gameObject);
            SetParticlelLayer(gameObject,UIParticleLayer);
            this.m_belongedWindowBase = windowBase;
            if (this.m_belongedWindowBase != null)
            {
                if (this.m_belongedWindowBase.IsHided())
                {
                    Hide();
                }
            }
            this.m_isInitialized = true;
           
        }
        /// <summary>
        /// 获取所有的render组件
        /// </summary>
        public void InitRender(GameObject go)
        {
            Renderer component = go.GetComponent<Renderer>();
            if (component != null)
            {
                m_renderList.Add(component);
            }
            for (int i = 0; i < go.transform.childCount; i++)
            {
                InitRender(go.transform.GetChild(i).gameObject);
            }
        }
        public override void OnDestory()
        {
            base.OnDestory();
            this.m_renderList = null;
        }

        public override void Appear()
        {
            base.Appear();
            this.gameObject.SetActive(true);
            Refresh();
        }

        public override void Close()
        {
            base.Close();
            this.gameObject.SetActive(false);
        }

        public override void Hide()
        {
            base.Hide();
            this.gameObject.SetActive(false);
        }

        void Refresh()
        {
            float designScale = GameConfig.Resolution.x / GameConfig.Resolution.y;
            float scaleRate = (float) Screen.width / (float) Screen.height;
            foreach (ParticleSystem componentsInChild in transform.GetComponentsInChildren<ParticleSystem>(true))
            {
                if (!scaleData.ContainsKey(componentsInChild.transform))
                {
                    scaleData[componentsInChild.transform] = new ScaleData(){transform =  componentsInChild.transform,beginScale = componentsInChild.transform.localScale};
                }
            }
            foreach (KeyValuePair<Transform,ScaleData> keyValuePair in scaleData)
            {
                if (scaleRate < designScale)
                {
                    float scaleFactor = scaleRate / designScale;
                    keyValuePair.Value.transform.localScale = keyValuePair.Value.beginScale * scaleFactor;
                }
                else
                {
                    keyValuePair.Value.transform.localScale = keyValuePair.Value.beginScale;
                }
            }
        }

        void OnTransformChildrenChanged()
        {
            Refresh();
        }
        
        public override void SetSortingOrder(int sortingOrder)
        {
            base.SetSortingOrder(sortingOrder);
            if (this.m_renderList != null)
            {
                for (int i = 0; i < m_renderList.Count; i++)
                {
                    m_renderList[i].sortingOrder = sortingOrder + index;
                } 
            }
        }
        /// <summary>
        /// 加载粒子特效
        /// </summary>
        public void LoadParticle(string name)
        {
            if (this.m_isInitialized == false)
            {
                return;
            }
            this.m_resPath = name;
            ClearParticle();//先调用清除函数
            //如果资源名不为空就加在特效
            if (this.m_resPath != string.Empty)
            {
                GameObject obj =Resources.Load<GameObject>(this.m_resPath);
                if (obj != null && gameObject.transform.childCount == 0)
                {
                    GameObject ins = Object.Instantiate(obj) as GameObject;
                    ins.transform.SetParent(gameObject.transform);
                    ins.transform.localPosition = Vector3.zero;
                    ins.transform.localRotation = Quaternion.identity;
                    ins.transform.localScale = Vector3.one;
                    PartObj = ins;
                }
            }
            //获取特效的所有render组件
            InitRender(gameObject);
            SetParticlelLayer(gameObject,UIParticleLayer);
            if (this.m_belongedWindowBase != null)
            {
                this.SetSortingOrder(this.m_belongedWindowBase.GetSortingOrder()+index);
                if (this.m_belongedWindowBase.IsHided())
                {
                    Hide();
                }
            }
            Refresh();
        }
        /// <summary>
        /// 清除粒子特效
        /// </summary>
        public void ClearParticle()
        {
            this.m_renderList = null;
            if (gameObject.transform.childCount > 0)
            {
                Transform go = gameObject.transform.GetChild(0);
                if (go != null)
                {
                    go.SetParent(null);
                    DestroyImmediate(go.gameObject);
                }
            }
        }
        public void SetParticlelLayer(GameObject go,int layer)
        {
            go.layer = layer;
            for (int i = 0; i < go.transform.childCount; i++)
            {
                this.SetParticlelLayer(go.transform.GetChild(i).gameObject, layer);
            }
        }
    }
}