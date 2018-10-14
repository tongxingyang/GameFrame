using System;
using GameFrame.Common;
using UnityEngine;

namespace GameFrame
{
    /// <summary>
    /// 音效类
    /// </summary>
    public class SoundEffect:MonoBehaviour
    {
        /// <summary>
        /// 当前序列号
        /// </summary>
        private int sequence;
        /// <summary>
        /// AudioSource
        /// </summary>
        private AudioSource audioSource;
        /// <summary>
        /// 原始音量
        /// </summary>
        private float originalVolume;
        /// <summary>
        /// 持续时间
        /// </summary>
        private float duration;
        /// <summary>
        /// 开始时间
        /// </summary>
        private float time;
        /// <summary>
        /// 播放回调
        /// </summary>
        private Action callback;
        /// <summary>
        /// 唯一标志
        /// </summary>
        private bool singleton;
        /// <summary>
        /// 是否跟随物体
        /// </summary>
        private bool isFollow;
        /// <summary>
        /// 跟随的目标
        /// </summary>
        private Transform target;


        public string Name
        {
            get { return audioSource.clip.name; }
        }
	
        public float Length
        {
            get { return audioSource.clip.length; }
        }

        public float PlaybackPosition
        {
            get { return audioSource.time; }
        }

        public int Sequence
        {
            get { return sequence; }
            set { sequence = value; }
        }
        public AudioSource Source
        {
            get{ return audioSource; }
            set { audioSource = value; }
        }

        public float OriginalVolume
        {
            get{ return originalVolume; }
            set { originalVolume = value; }
        }

        public float Duration
        {
            get{ return duration; }
            set { duration = value; }
        }
        public bool IsFollow
        {
            get { return isFollow; }
            set { isFollow = value; }
        }
        public float Time
        {
            get{ return time; }
            set { time = value; }
        }

        public float NormalisedTime
        {
            get{ return Time / Duration; }
        }

        public Action Callback
        {
            get{ return callback; }
            set { callback = value; }
        }

        public bool Singleton
        {
            get{ return singleton; }
            set { singleton = value; }
        }
        public Transform Target
        {
            get { return target; }
            set { target = value; }
        }
    }
}