using System.Collections;
using System.Collections.Generic;
using System.IO;
using GameFrame;
using LuaInterface;
using UnityEngine;

namespace GameFrame
{
    public class LuaManager : SingletonMono<LuaManager>
    {
        private LuaState lua;
        private LuaLooper loop = null;
        
        public override void Init()
        {
            base.Init();
            lua = new LuaState();
            this.OpenLibs();
            lua.LuaSetTop(0);
            
            LuaBinder.Bind(lua);
            DelegateFactory.Init();
            LuaCoroutine.Register(lua,this);
        }
        /// <summary>
        /// 外部調用接口
        /// </summary>
        public void InitStart()
        {
            InitLuaPath();
            InitLuaBundle();
            this.lua.Start();
            StartMain();
            StartLooper();
        }
    
        void InitLuaPath()
        {
            if (LuaConst.LuaBundleMode)
            {
                lua.AddSearchPath(LuaConst.luaResDir);
            }
            else
            {
                lua.AddSearchPath(LuaConst.luaDir);
                lua.AddSearchPath(LuaConst.toluaDir);
            }
        }
        void InitLuaBundle() {
            if (LuaConst.LuaBundleMode) {
                AutoAddLuaBundle();
            }
        }
        /// <summary>
        /// 自动加载lua bundle
        /// </summary>
        void AutoAddLuaBundle()
        {
            string[] files = Directory.GetFiles(LuaConst.luaResDir, "*" + Platform.AssetBundleExt);
            foreach (string file in files)
            {
                var filename = file.Replace(LuaConst.luaResDir+"/", "");
                AddBundle(filename);
            }
        }
        void AddBundle(string bundleName)
        {
            string url = LuaConst.luaResDir+"/"+ bundleName.ToLower();
            if (File.Exists(url))
            {
                AssetBundle bundle = AssetBundle.LoadFromFile(url);
                if (bundle != null)
                {
                    bundleName = bundleName.Replace(Platform.AssetBundleExt, "");
                    LuaFileUtils.Instance.AddSearchBundle(bundleName.ToLower(), bundle);
                }
            }
        }
        
        public void DoFile(string filename)
        {
            lua.DoFile(filename);
        }
    
        public void LuaGC()
        {
            lua.LuaGC(LuaGCOptions.LUA_GCCOLLECT);
        }
        

        public void Close()
        {
            loop.Destroy();
            loop = null;
            lua.Dispose();
            lua = null;
        }
    
        public object[] CallFunction(string funcName,params  object[] args)
        {
            LuaFunction function = lua.GetFunction(funcName);
            if (function != null)
            {
                return function.LazyCall(args);
            }
            return null;
        }
        
    
        void StartMain()
        {
            lua.DoFile("Test/Main.lua",false);
            LuaFunction main = lua.GetFunction("Main");
            main.Call();
            main.Dispose();
            main = null;
        }
        
        void StartLooper()
        {
            loop = gameObject.AddComponent<LuaLooper>();
            loop.luaState = lua;
        }
        /// <summary>
        /// 初始化加载第三方库
        /// </summary>
        void OpenLibs() {
            lua.OpenLibs(LuaDLL.luaopen_pb);      
            lua.OpenLibs(LuaDLL.luaopen_lpeg);
            lua.OpenLibs(LuaDLL.luaopen_bit);
            lua.OpenLibs(LuaDLL.luaopen_socket_core);
    
            this.OpenCJson();
        }
        //cjson 比较特殊，只new了一个table，没有注册库，这里注册一下
        protected void OpenCJson() {
            lua.LuaGetField(LuaIndexes.LUA_REGISTRYINDEX, "_LOADED");
            lua.OpenLibs(LuaDLL.luaopen_cjson);
            lua.LuaSetField(-2, "cjson");
    
            lua.OpenLibs(LuaDLL.luaopen_cjson_safe);
            lua.LuaSetField(-2, "cjson.safe");
        }
    }


}
