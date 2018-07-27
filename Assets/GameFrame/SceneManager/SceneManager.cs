using UIFrameWork;
using UnityEngine;

namespace GameFrame.Scene
{
    public class SceneManager:Singleton<SceneManager>
    {
        private AssetBundle sceneAssetBundle;
        
        public void LoadScene(string sencename)
        {
            Singleton<WindowManager>.GetInstance().OpenWindow("Loading",true,true,new LoadingContent(sencename));
        }
    }
}