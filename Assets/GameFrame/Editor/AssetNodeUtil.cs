﻿using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace GameFrame.Editor
{
    public class AssetNodeUtil
    {
        public static bool isLog = true;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="resList"></param>
        /// <param name="filterDirList">过滤的目录</param>
        /// <param name="filterExts">过滤的扩展名</param>
        /// <param name="imageExts"></param>
        /// <param name="isSpriteTag"></param>
        /// <param name="assetbundleExt"></param>
        /// <returns></returns>
        public static Dictionary<string, AssetNode> GenerateAllNode(List<string> resList, string[] filterDirList, List<string> filterExts, List<string> imageExts, bool isSpriteTag = false, string assetbundleExt = ".assetbundle")
        {
    
            Dictionary<string, AssetNode> nodeDict = new Dictionary<string, AssetNode>();
            Dictionary<string, AssetNode>   spriteTagPackageDict = new Dictionary<string, AssetNode>();
            List<AssetNode>                 spriteTagPackageList = new List<AssetNode>();

            string[] dependencies = AssetDatabase.GetDependencies(resList.ToArray());
            int count = dependencies.Length;
    
            for(int i = 0; i < count; i ++)
            {
                EditorUtility.DisplayProgressBar("生成所有节点",  i + "/" + count, 1f * i / count);
                string path = dependencies[i];
                bool isFilterDir = false;
                foreach(string filterDir in filterDirList)
                {
                    if(path.IndexOf(filterDir) != -1)
                    {
                        isFilterDir = true;
                        break;
                    }
                }
                if(isFilterDir) continue;
                string ext = Path.GetExtension(path).ToLower();
                if (filterExts.IndexOf(ext) != -1) continue;
                AssetImporter assetImporter = AssetImporter.GetAtPath(path);
                if (!string.IsNullOrEmpty(assetImporter.assetBundleName))
                {
                    if (assetImporter.assetBundleName.IndexOf(assetbundleExt) == -1)
                    {
                        continue;
                    }
                }
                if(isSpriteTag && imageExts.IndexOf(ext) != -1)
                {
                    TextureImporter importer = TextureImporter.GetAtPath(path) as TextureImporter;
                    if(importer.textureType == TextureImporterType.Sprite && !string.IsNullOrEmpty(importer.spritePackingTag))
                    {
                        importer.assetBundleName = importer.spritePackingTag + assetbundleExt;
                        AssetNode spriteTagPackageNode;
    
                        if (!spriteTagPackageDict.TryGetValue(importer.spritePackingTag, out spriteTagPackageNode))
                        {
                            spriteTagPackageNode = new AssetNode();
                            spriteTagPackageNode.path = importer.spritePackingTag;
                            spriteTagPackageDict.Add(spriteTagPackageNode.path, spriteTagPackageNode);
                            spriteTagPackageList.Add(spriteTagPackageNode);
                        }
    
                        AssetNode spriteTagNode = new AssetNode();
                        spriteTagNode.path = path;
                        spriteTagPackageNode.AddAsset(spriteTagNode);
                        continue;
                    }
                }
    
                AssetNode node = new AssetNode();
                node.path = path;
                nodeDict.Add(node.path, node);
            }
    
            if(isLog) AssetNode.PrintNodeDict(nodeDict, "nodeDict");
            if(isLog) AssetNode.PrintNodeTree(spriteTagPackageList, "图集");
            EditorUtility.ClearProgressBar();
            return nodeDict;
        }
        //生成每个节点依赖的节点
        public static void GenerateNodeDependencies(Dictionary<string, AssetNode> nodeDict)
        {
            int count = nodeDict.Count;
            int index = 0;

            foreach(var kvp in nodeDict)
            {
                EditorUtility.DisplayProgressBar("生成每个节点依赖的节点",  index + "/" + count, 1f * (index++) / count);

                AssetNode node = kvp.Value;

                string[]  dependencies = AssetDatabase.GetDependencies(node.path);

                for (int i = 0; i < dependencies.Length; i++)
                {
                    string path = dependencies[i];
                    if (path == node.path)
                        continue;

                    AssetNode depNode;
                    if(nodeDict.TryGetValue(path, out depNode))
                    {
                        node.AddDependencie(depNode);
                    }
                }
            }

            EditorUtility.ClearProgressBar();
            if(isLog) AssetNode.PrintNodeDict(nodeDict, "生成每个节点依赖的节点 nodeDict");
        }
        /// <summary>
        /// resource强制设置为root节点
        /// </summary>
        /// <param name="nodeDict"></param>
        /// <returns></returns>
        public static List<string> GenerateForcedRoots(Dictionary<string,AssetNode> nodeDict)
        {
            int count = nodeDict.Count;
            int index = 0;
            List<string> forceRootList = new List<string>();
            foreach (KeyValuePair<string,AssetNode> keyValuePair in nodeDict)
            {
                EditorUtility.DisplayProgressBar("生成要强制设置Root的节点",  index + "/" + count, 1f * (index++) / count);
                string path = keyValuePair.Value.path;
                if (path.IndexOf("Resources/") != 1)
                {
                    forceRootList.Add(path);
                }
            }
            EditorUtility.ClearProgressBar();
            return forceRootList;
        }

        public static void ForcedSetRoots(Dictionary<string, AssetNode> nodeDict, List<string> forceRootList)
        {
            int count = forceRootList.Count;
            int index = 0;
            foreach (string path in forceRootList)
            {
                EditorUtility.DisplayProgressBar("强制设置某些节点为Root节点，删掉被依赖",  index + "/" + count, 1f * (index++) / count);
                if (nodeDict.ContainsKey(path))
                {
                    AssetNode node = nodeDict[path];
                    node.ForcedSetRoot();
                }
            }
            if(isLog) AssetNode.PrintNodeDict(nodeDict, "强制设置某些节点为Root节点，删掉被依赖");

            EditorUtility.ClearProgressBar();
        }

        public static List<AssetNode> FindRoots(Dictionary<string, AssetNode> nodeDict)
        {
            int count = nodeDict.Count;
            int index = 0;
            List<AssetNode> roots = new List<AssetNode>();
            foreach (KeyValuePair<string,AssetNode> kvp in nodeDict)
            {
                EditorUtility.DisplayProgressBar("寻找入度为0的节点",  index + "/" + count, 1f * (index++) / count);
                AssetNode node = kvp.Value;
                if (node.isRoot)
                {
                    roots.Add(node);
                }
            }
            if(isLog) AssetNode.PrintNodeTree(roots, "寻找入度为0的节点");
            EditorUtility.ClearProgressBar();
            return roots;
        }

        public static void RemoveParentShare(List<AssetNode> roots)
        {
            int count = roots.Count;
            int index = 0;
            foreach(AssetNode node in roots)
            {
                EditorUtility.DisplayProgressBar("寻找入度为0的节点",  index + "/" + count, 1f * (index++) / count);
                node.RemoveParentShare();
            }
    
            if(isLog) AssetNode.PrintNodeTree(roots, "移除父节点的依赖和自己依赖相同的节点");
            EditorUtility.ClearProgressBar();
        }
    
        public static void MergeParentCountOnce(List<AssetNode> roots)
        {
            int count = roots.Count;
            int index = 0;
            foreach(AssetNode node in roots)
            {
                EditorUtility.DisplayProgressBar("入度为1的节点自动打包到上一级节点",  index + "/" + count, 1f * (index++) / count);
                node.MergeParentCountOnce();
            }
    
            if(isLog) AssetNode.PrintNodeTree(roots, "入度为1的节点自动打包到上一级节点");
            EditorUtility.ClearProgressBar();
        }

        public static void FilterDotNeedNode(Dictionary<string, AssetNode> needDict, List<AssetNode> needRoots)
        {
            int count = needRoots.Count;
            for(int i = 0; i < needRoots.Count; i ++)
            {
                EditorUtility.DisplayProgressBar("过滤不需要打包的节点",  i + "/" + count, 1f * i / count);
                needRoots[i].FilterDotNeedNode(needDict);
            }
    
            if(isLog) AssetNode.PrintNodeTree(needRoots, "过滤不需要打包的节点");
            EditorUtility.ClearProgressBar();
        }
    
        public static Dictionary<string, AssetNode> GenerateAssetBundleNodes(List<AssetNode> roots)
        {
            Dictionary<string, AssetNode> nodeDict = new Dictionary<string, AssetNode>();
            int count = roots.Count;
            for(int i = 0; i < roots.Count; i ++)
            {
                EditorUtility.DisplayProgressBar("生成需要设置AssetBundleName的节点",  i + "/" + count, 1f * i / count);
                roots[i].GenerateAssetBundleNodes(nodeDict);
            }
    
            if(isLog) AssetNode.PrintNodeDict(nodeDict, "生成需要设置AssetBundleName的节点");
            EditorUtility.ClearProgressBar();
            return nodeDict;
        }
    
        public static void SetAssetBundleNames(Dictionary<string, AssetNode> nodeDict, string ext)
        {
            int count = nodeDict.Count;
            int i = 0;
            foreach(var kvp in nodeDict)
            { 
                i++;
                EditorUtility.DisplayProgressBar("设置AssetBundleNames",  i + "/" + count, 1f * i / count);
                kvp.Value.SetAssetBundleName(ext);
            }
            EditorUtility.ClearProgressBar();
        }
        /// <summary>
        /// 设置资源导出类型
        /// </summary>
        public static void SetAssetExportType( Dictionary<string, AssetNode> assetDict)
        {
            foreach (KeyValuePair<string,AssetNode> keyValuePair in assetDict)
            {
                if (keyValuePair.Value.isRoot)
                {
                     keyValuePair.Value.exportType = AssetBundleExportType.Root;
                }
                else if(keyValuePair.Value.childCount==0)
                {
                    keyValuePair.Value.exportType = AssetBundleExportType.Asset;
                }
            }
        }
    }
}