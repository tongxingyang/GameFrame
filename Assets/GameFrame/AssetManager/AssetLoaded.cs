using UnityEngine;

namespace GameFrame.AssetManager
{
    public class AssetLoaded
    {
        public string url;
        public float lastTime = 0;
        private int m_referencedCount = 0;

        public int ReferencedCount
        {
            get { return m_referencedCount; }
            set {m_referencedCount = value;}
        }

        public AssetBundle AssetBundle;

        public AssetLoaded(AssetBundle assetBundle,string url)
        {
            this.url = url;
            this.AssetBundle = assetBundle;
            this.ReferencedCount = 1;
        }
    }
}