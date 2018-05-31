using GameFrame.AssetManager;
using UnityEngine;
using UnityEngine.UI;

namespace GameFrame.Debug
{
    public class DebugAssetLoadedItem : MonoBehaviour
    {
        public Text index;
        public Text referenceCount;
        public Text path;
        
        public void SetResLoaded(ResLoaded resLoaded,int _index)
        {
            index.text = _index.ToString();
            referenceCount.text = resLoaded.ReferencedCount.ToString();
            path.text = "Type       " + resLoaded.ObjType + "    Path      " + resLoaded.path;
        }

        public void SetAssetLoaded(string assetname,AssetLoaded assetLoaded,int _index)
        {
            index.text = _index.ToString();
            referenceCount.text = assetLoaded.ReferencedCount.ToString();
            path.text = assetname;
        }
        
    }
}
