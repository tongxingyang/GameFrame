using System;
using UnityEngine;

namespace GameFrame.AssetManager
{
    public class ResLoaded
    {
        public string path;
        public Type ObjType;
        public float lastTime = 0;
        private int m_referencedCount = 0;

        public int ReferencedCount
        {
            get { return m_referencedCount; }
            set {
                if (value != 0 && value > m_referencedCount)
                {
                    lastTime = Time.unscaledTime;
                }
                m_referencedCount = value;
            }
        }

        public UnityEngine.Object obj;

        public ResLoaded(string path,Type objtype)
        {
            this.path = path;
            ObjType = objtype;
            this.ReferencedCount = 1;
        }
    }
}