using System.Collections;
using System.Collections.Generic;
using GameFrame;
using UnityEngine;

namespace GameFrame
{
    public class AudioObject
    {
        public static float CacheTime = 10;
        public static float IntervalTime = 0.2f;
        public float lastusetime;
        public string path;
        public AudioClip Clip;
        public AssetBundle ab;
    }
    public class AudioSourcePool:Singleton<AudioSourcePool>
    {
        private const int MAXFREECOUNT = 6;
        private List<AudioSource> m_listAudioSource;
        private List<GameObject> m_listGameObject;
        public override void Init()
        {
            base.Init();
            m_listAudioSource = new List<AudioSource>();
            m_listGameObject = new List<GameObject>();
            for (int i = 0; i < MAXFREECOUNT; i++)
            {
                GameObject SoundObj = new GameObject();
                SoundObj.name = "SoundEffect";
                AudioSource source = SoundObj.AddComponent<AudioSource>();
                source.spatialBlend = 1.0f;
                source.rolloffMode = AudioRolloffMode.Custom;
                source.maxDistance = SingletonMono<AudioManager>.GetInstance().AudioSourceMaxDis;
                source.minDistance = SingletonMono<AudioManager>.GetInstance().AudioSourceMinDis;
                SoundObj.SetActive(false);
                Object.DontDestroyOnLoad(SoundObj);
                m_listAudioSource.Add(source);
                m_listGameObject.Add(SoundObj);
            }
        }

        public AudioSource GetFreeAudioSource(out GameObject obj)
        {
            for (int i = 0; i < m_listAudioSource.Count; i++)
            {
                if (!m_listAudioSource[i].isPlaying)
                {
                    obj = m_listGameObject[i];
                    return m_listAudioSource[i];
                }
            }
            obj = null;
            return null;
        }
    }

    class AudioPlayer
    {
        private AudioSource Source = null;
        private string audioName;
        
    }
    public class AudioManager : SingletonMono<AudioManager> {
        /// <summary>
        /// 缓存加载的Bundle
        /// </summary>
        private Dictionary<string,AudioClip> SoundCaches;

        public  int AudioSourceMaxDis = 1;
        public  int AudioSourceMinDis = 15;
        /// <summary>
        /// 背景音乐AudioSource
        /// </summary>
        static private AudioSource m_bgSource;
        public override void Init()
        {
            base.Init();
            Singleton<AudioSourcePool>.GetInstance().Init();
            SoundCaches = new Dictionary<string, AudioClip>();
        }
    }
}
