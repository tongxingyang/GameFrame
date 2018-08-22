using System.Collections.Generic;
using System.Linq;
using GameFrameDebuger;
using UnityEditor;

namespace GameFrame.Editor
{
    public enum AssetBundleExportType
    {
        Asset = 1,
        Root = 1 << 1,
    }

    public class AssetNode
    {
        public string path;//路径
        public System.IO.FileInfo file;
        public AssetBundleExportType exportType = AssetBundleExportType.Asset;
        public List<AssetNode> parents = new List<AssetNode>();
        public List<AssetNode> childs = new List<AssetNode>();
        public List<AssetNode> assets = new List<AssetNode>();
        public int assetCount
        {
            get
            {
                return assets.Count;
            }
        }

        public int childCount
        {
            get
            {
                return childs.Count;
            }
        }

        public int parentCount
        {
            get
            {
                return parents.Count;
            }
        }

        public bool isRoot
        {
            get
            {
                return parentCount == 0;
            }
        }
        
        public override string ToString()
        {
            return string.Format("{0}   \t[parentCount={1},    childCount={2},\t\tassetCount={3}]", path, parentCount, childCount, assetCount);
        }
        
        public void AddAsset(AssetNode node)
        {
            if (!assets.Contains(node))
            {
                assets.Add(node);
                node.parents.Add(this);
            }
        }

        public void AddDependencie(AssetNode node)
        {
            if (!childs.Contains(node))
            {
                childs.Add(node);
                node.parents.Add(this);
            }
        }

        public void RemoveDependencie(AssetNode node)
        {
            if (childs.Contains(node))
            {
                childs.Remove(node);
                if (node.parents.Contains(this))
                {
                    node.parents.Remove(this);
                }
            }
        }

        public void ForcedSetRoot()
        {
            for (int i = parents.Count - 1; i >= 0; i--)
            {
                AssetNode parent = parents[i];
                parent.RemoveDependencie(this);
            }
        }
        
        public void MergeAssetToParent(AssetNode pnode)
        {
            foreach(AssetNode node in assets)
            {
                pnode.assets.Add(node);
                node.parents.Remove(this);
                if (!node.parents.Contains(pnode))
                {
                    node.parents.Add(pnode);
                }
            }
        }
        
        public void RemoveParentShare()
        {
            List<AssetNode> list = new List<AssetNode>(childs.ToArray());
            foreach(AssetNode cnode in list)
            {
                foreach(AssetNode pnode in parents)
                {
                    if (pnode.childs.Contains(cnode))
                    {
                        pnode.RemoveDependencie(cnode);
                    }
                }
                cnode.RemoveParentShare();
            }
        }
        
        public void MergeParentCountOnce()
        {
            for(int i = childs.Count - 1;  i >= 0; i --)
            {
                AssetNode cnode = childs[i]; 
                cnode.MergeParentCountOnce();
                
                if (cnode.parentCount == 1 && cnode.CanMergeParent()) //如果孩子节点有parent数量大于2的就不能merge 会造成引用重复资源
                {
                    // 子节点 变为 包含资源
                    assets.Add(cnode);
                    childs.Remove(cnode);

                    // 包含的资源 合并到 父亲节点
                    cnode.MergeAssetToParent(this);
                }
            }
        }

        public bool CanMergeParent()
        {
            foreach (AssetNode assetNode in childs)
            {
                if (assetNode.parentCount >= 2)
                {
                    return false;
                }
            }
            return true;
        }
        
        public void GetRoots(Dictionary<string, AssetNode> dict)
        {
            if (isRoot)
            {
                if (!dict.ContainsKey(path))
                {
                    dict.Add(path, this);
                }
            }
            else
            {
                for (int i = 0; i < parents.Count; i++)
                {
                    parents[i].GetRoots(dict);
                }
            }
        }

        public string GetAssetBundleName(string ext)
        {
            string assetBundleName;
            string[] strs = path.Split('/');
            assetBundleName = strs[strs.Length - 1];
            assetBundleName = PathResolver.ChangeExtension(assetBundleName, ext).ToLower();
            return assetBundleName;
        }
        
        public void SetAssetBundleName(string ext)
        {
            AssetImporter importer = AssetImporter.GetAtPath(path);
            importer.assetBundleName = GetAssetBundleName( ext);
        }

        public void GenerateAssetBundleNodes(Dictionary<string,AssetNode>bundleDict)
        {
            if (!bundleDict.ContainsKey(path))
            {
                bundleDict.Add(path,this);
            }
            foreach (AssetNode node in childs)
            {
                node.GenerateAssetBundleNodes(bundleDict);
            }
        }

        public void GetNeedParentNodeList(Dictionary<string,AssetNode>needDict,Dictionary<string,AssetNode>needParentDict)
        {
            for (int i = parents.Count - 1; i >= 0; i--)
            {
                AssetNode parent = parents[i];
                if (needDict.ContainsKey(parent.path))
                {
                    if (!needParentDict.ContainsKey(parent.path))
                    {
                        needParentDict.Add(parent.path, parent);
                    }
                }
                else
                {
                    parent.GetNeedParentNodeList(needDict, needParentDict);
                }
            }
        }
        
        public void FilterDotNeedNode(Dictionary<string, AssetNode> needDict)
        {
            for(int i = childs.Count - 1; i >= 0; i--)
            {
                AssetNode child = childs[i];
                child.FilterDotNeedNode(needDict);
            }

            if (!needDict.ContainsKey(path))
            {
                for (int i = parents.Count - 1; i >= 0; i--)
                {
                    AssetNode parent = parents[i];
                    parent.RemoveDependencie(this);
                }
            }
            else
            {
                Dictionary<string, AssetNode> needParentDict = new Dictionary<string, AssetNode>();

                GetNeedParentNodeList(needDict, needParentDict);

                foreach(var kvp in needParentDict)
                {
                    kvp.Value.AddDependencie(this);
                }

                for (int i = parents.Count - 1; i >= 0; i--)
                {
                    AssetNode parent = parents[i];
                    if(!needParentDict.ContainsKey(parent.path))
                    {
                        parent.RemoveDependencie(this);
                    }
                }
            }
        }
        
        #region DebugInfo

        public string GetTreeInfo(int layout = 0, string cprestr = "-- ", string aprestr = "   ")
        {
            string cpre = "";
            string apre = "";
            for(int i = 0; i < layout; i ++)
            {
                cpre += cprestr;
                apre += aprestr;
            }
            string str = "";
            if (layout == 0)
            {
                str += cpre + this + "\n";
            }
            layout++;
            cpre += cprestr;
            apre += aprestr;
            for(int i = 0; i < assets.Count; i ++)
            {
                str += apre + assets[i] + "\n";
            }
            for(int i = 0; i < childs.Count; i ++)
            {
                str += cpre + childs[i] + "\n";
                str += childs[i].GetTreeInfo(layout, cprestr);
            }
            return str;
        }

        public void PrintTreeInfo()
        {
            Debuger.Log(GetTreeInfo(0, "-- "));
        }

        public static void PrintNodeTree(List<AssetNode> roots, string destr="")
        {
            // 打印信息树
            string str = "[" + destr + "]\n";
            for(int i = 0; i < roots.Count; i ++)
            {
                AssetNode node = roots[i];
                str += node.GetTreeInfo(0, "-- ");
                str += "========================\n\n";
            }
            Debuger.Log(str);
        }


        public static void PrintNodeDict(Dictionary<string, AssetNode> dict, string destr="")
        {
            // 打印信息树
            string str = "[" + destr + "]\n";
            foreach(var kvp in dict)
            {
                AssetNode node = kvp.Value;
                str += node + "\n";
            }
            Debuger.Log(str);
        }

        #endregion
    }
}