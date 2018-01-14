using System;
using System.Collections;
using System.Collections.Generic;
using GameFrame;
using UnityEngine;
using UnityEngine.UI;
namespace GameFrame
{
    //public class UpdateManager
    public class UpdateManager : Singleton<UpdateManager>
    {
        /// <summary>
        /// 获取组件
        /// </summary>
        private Text AlertContextText;
        private Text StatusText;
        private Text ProgressText;
        private Text LocalResVersionText;
        private Text OnlineResVersionText;
        private Text AppVersion;

        private Button SureButton;
        private Button CancelButton;

        private Slider ProgressSliber;

        private GameObject UpdateGameObject;
        private GameObject AlertObject;

        private GameObject CanvasObj;


        private Action<bool> UpdateCallback;
        private bool m_isBeginUpdate = false;
        public override void Init()
        {
            base.Init();
            CanvasObj = GameObject.Find("Canvas");
        }

        public void SetUpdateCallback(Action<bool> action)
        {
            UpdateCallback = action;
        }

        public void StartCheckUpdate(Action<bool> action)
        {
            UpdateCallback = action;
            //加载update prefab
            GameObject obj = Resources.Load<GameObject>("UpdatePrefab");
            UpdateGameObject = GameObject.Instantiate(obj);
            if (UpdateGameObject != null)
            {
                UpdateGameObject.transform.SetParent(CanvasObj.transform);
                UpdateGameObject.transform.localPosition = Vector3.zero;
                UpdateGameObject.transform.localScale = Vector3.one;
                UpdateGameObject.GetComponent<RectTransform>().offsetMax = Vector2.zero;
                UpdateGameObject.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            }
            //给组件赋值
            AlertContextText = UpdateGameObject.transform.Find("Alert/Content/Image/Text").GetComponent<Text>();
            StatusText = UpdateGameObject.transform.Find("Text/Status").GetComponent<Text>();
            ProgressText = UpdateGameObject.transform.Find("Text/Progress").GetComponent<Text>();
            LocalResVersionText = UpdateGameObject.transform.Find("TopText/LocalVersion").GetComponent<Text>();
            OnlineResVersionText = UpdateGameObject.transform.Find("TopText/OnlineVersion").GetComponent<Text>();
            AppVersion = UpdateGameObject.transform.Find("TopText/APPVersion").GetComponent<Text>();

            AlertObject = UpdateGameObject.transform.Find("Alert").gameObject;

            SureButton = UpdateGameObject.transform.Find("Alert/Sure").GetComponent<Button>();
            CancelButton = UpdateGameObject.transform.Find("Alert/Cancel").GetComponent<Button>();

            ProgressSliber = UpdateGameObject.transform.Find("Slider").GetComponent<Slider>();

            m_isBeginUpdate = true;
        }
    }

}
