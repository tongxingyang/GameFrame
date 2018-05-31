using System;
using System.Collections.Generic;
using System.IO;
using GameFrame.Debug;
using UnityEngine;

namespace GameFrame.AssetManager
{
    public class DebugAssetLoadedPanel:MonoBehaviour
    {
        public DebugAssetLoadedItem prefabItem;
        public RectTransform content;
        List<DebugAssetLoadedItem> items = new List<DebugAssetLoadedItem>();
        public int tabIndex = 0;

        public void SetTab(int index)
        {
            tabIndex = index;
            ShowList();
        }

        public void ShowList()
        {
            if (tabIndex == 0)
            {
                ShowListResource();
            }
            else
            {
                ShowListAssetBundle();
            }
        }

        public void ShowListResource()
        {
            Dictionary<Type, Dictionary<string, ResLoaded>> LoadRes = AssetManager.GetInstance().LoadedResources;
            int index = 0;
            foreach (var loadRe in LoadRes)
            {
                foreach (var resLoaded in loadRe.Value)
                {
                    DebugAssetLoadedItem item;
                    if (index < items.Count)
                    {
                        item = items[index];
                    }
                    else
                    {
                        GameObject go = Instantiate(prefabItem.gameObject);
                        item = go.GetComponent<DebugAssetLoadedItem>();
                        item.transform.SetParent(content,false);
                        items.Add(item);
                    }
                    item.gameObject.SetActive(true);
                    item.SetResLoaded(resLoaded.Value,index);
                    index++;
                }
            }
            for (int i = index; i < items.Count; i++)
            {
                DebugAssetLoadedItem item = items[i];
                item.gameObject.SetActive(false);
            }
        }

        public void ShowListAssetBundle()
        {
            Dictionary<string, AssetLoaded> LoadedAsset = AssetManager.GetInstance().LoadedAssetBundles;
            int index = 0;
            if (LoadedAsset != null)
            {
                foreach (var assetLoaded in LoadedAsset)
                {
                    DebugAssetLoadedItem item;
                    if (index < items.Count)
                    {
                        item = items[index];
                    }
                    else
                    {
                        GameObject go = Instantiate(prefabItem.gameObject);
                        item = go.GetComponent<DebugAssetLoadedItem>();
                        item.transform.SetParent(content.transform,false);
                        items.Add(item);
                    }
                    item.gameObject.SetActive(true);
                    item.SetAssetLoaded(assetLoaded.Key,assetLoaded.Value,index);
                    index++;
                }
            }
            for (int i = index; i < items.Count; i++)
            {
                DebugAssetLoadedItem item = items[index];
                item.gameObject.SetActive(false);
            }
        }

        public void OnEnable()
        {
            ShowList();
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}