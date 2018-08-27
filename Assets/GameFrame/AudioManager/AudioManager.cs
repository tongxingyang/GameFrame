using System;
using System.Collections;
using System.Collections.Generic;
using GameFrame;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking.NetworkSystem;

namespace GameFrame
{
    /// <summary>
    /// Music转换的类型
    /// </summary>
    public enum MusicTransType
    {    
        /// <summary>
        /// 快速切换
        /// </summary>
        Swift,
        /// <summary>
        /// 线性减弱
        /// </summary>
        LinearFade,
        /// <summary>
        /// 淡入淡出
        /// </summary>
        CrossFade
    }
    
    public struct BackgroundMusic
    {
        /// <summary>
        /// 当前BG Clip
        /// </summary>
        public AudioClip CurrentClip;
        /// <summary>
        /// 下次播放的Clip
        /// </summary>
        public AudioClip NextClip;
        /// <summary>
        /// 背景转换类型
        /// </summary>
        public MusicTransType TransType;
        /// <summary>
        /// 转换持续时间
        /// </summary>
        public float TransitionDuration;
    }

    /// <summary>
    /// AudioManager
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : SingletonMono<AudioManager>
    {

        [Header("背景音乐的属性")] 
        //背景音乐开关
        [SerializeField] private bool musicOn = true;
        //背景音量
        [Range(0,1)]
        [SerializeField] private float musicVolume = 1f;
        //是否使用MusicValume的值作为背景音量
        [SerializeField] private bool useMusicOnStart = false;
        //混音器
        [SerializeField] private AudioMixerGroup musicMixerGroup = null;
            
        [SerializeField] private string volumeOfMusicMixer = string.Empty;
        
        [Space(4)] 
        
        [Header("音效的属性")] 
        //音效的开关
        [SerializeField] private bool soundEffectOn = true;

        [Range(0, 1)] [SerializeField] private float soundEffectVolume = 1f;
        [SerializeField] private bool useSoundEffectOnStart = false;
        [SerializeField] private AudioMixerGroup soundEffectMixerGroup = null;
        [SerializeField] string volumeOfSoundEffectMixer = String.Empty;
        Dictionary<string,AudioClip> audioClipDic = new Dictionary<string, AudioClip>();
        
        private List<SoundEffect> soundEffectPool = new List<SoundEffect>();
        
        private static int sequence = 1;
        private static bool alive = true;
        private static BackgroundMusic backgroundMusic;
        private static AudioSource musicSource = null;
        private static AudioSource crossFadeSource = null;
        private static float currentMusicVol = 0, currentSoundEffectVol = 0;
        private static float musicVolCap = 0;
        private static float pitch = 1f, transitionTime;

        public AudioClip CurrentMusicClip
        {
            get { return backgroundMusic.CurrentClip; }
        }

        public List<SoundEffect> SoundEffectPool
        {
            get { return soundEffectPool; }
        }

        public Dictionary<string, AudioClip> AudioClipDic
        {
            get { return audioClipDic; }
        }

        public bool IsMusicPlaying
        {
            get { return musicSource != null && musicSource.isPlaying; }
        }

        public float MusicVolume
        {
            get { return musicVolume; }
            set {SetBackgroundVolume(value);}
        }

        public float SoundEffectVolume
        {
            get { return soundEffectVolume; }
            set {SetSoundEffectVolume(value);}
        }

        public bool IsMusicOn
        {
            get { return musicOn; }
            set{ToggleBackground(value);}
        }

        public bool IsSoundEffectOn
        {
            get { return soundEffectOn; }
            set {ToggleSoundEffect(value);}
        }
        
        private void SetBackgroundVolume(float vol)
        {
            vol = Mathf.Clamp01(vol);
            musicSource.volume = currentMusicVol = musicVolume = vol;
            musicSource.mute = !musicOn;
            if (musicMixerGroup != null && !string.IsNullOrEmpty(volumeOfMusicMixer.Trim()))
            {
                musicMixerGroup.audioMixer.SetFloat(volumeOfMusicMixer, NormaliseVolume(vol, false));
            }
        }

        private void SetSoundEffectVolume(float vol)
        {
            vol = Mathf.Clamp01(vol);
            soundEffectVolume = currentSoundEffectVol = vol;
            foreach (SoundEffect soundEffect in SoundEffectPool)
            {
                soundEffect.Source.volume = soundEffectVolume * soundEffect.OriginalVolume;
                soundEffect.Source.mute = !soundEffectOn;
            }
            if (soundEffectMixerGroup != null && string.IsNullOrEmpty(volumeOfSoundEffectMixer.Trim()))
            {
                soundEffectMixerGroup.audioMixer.SetFloat(volumeOfSoundEffectMixer, NormaliseVolume(vol, false));
            }
        }

        private void ToggleBackground(bool ison)
        {
            musicOn = ison;
            musicSource.mute = !musicOn;
        }

        private void ToggleSoundEffect(bool ison)
        {
            soundEffectOn = ison;
            foreach (SoundEffect soundEffect in soundEffectPool)
            {
                soundEffect.Source.mute = !soundEffectOn;
            }
        }
        
        static int GetSequence()
        {
            var ret = sequence;
            sequence++;
            return ret;
        }

        AudioSource InitAudioSource(AudioSource audioSource)
        {
            audioSource.outputAudioMixerGroup = musicMixerGroup;// todo txy
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0;
            audioSource.rolloffMode  = AudioRolloffMode.Linear;
            audioSource.loop = true;
            audioSource.volume = LoadBackgroundVolume();
            audioSource.mute = !musicOn;
            return audioSource;
        }
        
        public SoundEffect GetSoundEffectBySequence(int id)
        {
            for (int i = 0; i < soundEffectPool.Count; i++)
            {
                if (soundEffectPool[i].Sequence == id)
                {
                    return soundEffectPool[i];
                }
            }
            return null;
        }
		
        public void DeleteSoundEffectBySequence(int id)
        {
            var sfx = GetSoundEffectBySequence(id);
            if (sfx != null)
            {
                soundEffectPool.Remove(sfx);
                Destroy(sfx.gameObject);
            }
        }
        
        private float NormaliseVolume(float vol,bool b)
        {
            if (b)
            {
                vol += 80f;
                vol /= 100f;
            }
            else
            {
                vol *= 100f;
                vol -= 80f;
            }
        
            return vol;
        }

        #region CacheAudioClip

        public void ClearCacheDic()
        {
            audioClipDic.Clear();
        }

        public AudioClip GetCilpFromDic(string name)
        {
            foreach (KeyValuePair<string,AudioClip> keyValuePair in audioClipDic)
            {
                if (keyValuePair.Key == name)
                {
                    return keyValuePair.Value;
                }
            }
            return null;
        }
        
        public void AddToCache(AudioClip clip)
        {
            if (clip != null && !audioClipDic.ContainsKey(clip.name))
            {
                audioClipDic.Add(clip.name,clip);
            }
        }

        public void RemoveFromCache(string name)
        {
            if (GetCilpFromDic(name))
            {
                audioClipDic.Remove(name);
            }
        }

        #endregion

        #region Prefs Functions

        /// <summary>
        /// 从本地数据获取如果没有返回musicVolume
        /// </summary>
        /// <returns></returns>
        private float LoadBackgroundVolume()
        {
            return PlayerPrefsUtil.HasKey(PlayerPrefsKey.BgMusicVolKey)
                ? PlayerPrefsUtil.GetFloatSimple(PlayerPrefsKey.BgMusicVolKey)
                : musicVolume;
        }

        private float LoadSoundEffectVolume()
        {
            return PlayerPrefsUtil.HasKey(PlayerPrefsKey.SoundFxVolKey)
                ? PlayerPrefsUtil.GetFloatSimple(PlayerPrefsKey.SoundFxVolKey)
                : soundEffectVolume;
        }

        private bool IntToBool(int i)
        {
            return i != 0;
        }

        private bool LoadBackgroundStatus()
        {
            return PlayerPrefsUtil.HasKey(PlayerPrefsKey.BgMusicMuteKey)
                ? IntToBool(PlayerPrefsUtil.GetIntSimple(PlayerPrefsKey.BgMusicMuteKey))
                : musicOn;
        }
        
        private bool LoadSoundEffectStatus()
        {
            return PlayerPrefsUtil.HasKey(PlayerPrefsKey.SoundFxMuteKey)
                ? IntToBool(PlayerPrefsUtil.GetIntSimple(PlayerPrefsKey.SoundFxMuteKey))
                : soundEffectOn;
        }

        public void SaveBackgroundPrefs()
        {
            PlayerPrefsUtil.SetIntSimple(PlayerPrefsKey.BgMusicMuteKey,musicOn?1:0);
            PlayerPrefsUtil.SetFloatSimple(PlayerPrefsKey.BgMusicVolKey,musicVolume);
        }
        
        public void SaveSoundEffectPrefs()
        {
            PlayerPrefsUtil.SetIntSimple(PlayerPrefsKey.SoundFxMuteKey,soundEffectOn?1:0);
            PlayerPrefsUtil.SetFloatSimple(PlayerPrefsKey.SoundFxVolKey,soundEffectVolume);
        }

        public void ClearAllPerfs()
        {
            PlayerPrefsUtil.DeleteKey(PlayerPrefsKey.BgMusicMuteKey);
            PlayerPrefsUtil.DeleteKey(PlayerPrefsKey.BgMusicVolKey);
            PlayerPrefsUtil.DeleteKey(PlayerPrefsKey.SoundFxMuteKey);
            PlayerPrefsUtil.DeleteKey(PlayerPrefsKey.SoundFxVolKey);
            PlayerPrefsUtil.Save();
        }

        public void SaveAllPrefs()
        {
            SaveBackgroundPrefs();
            SaveSoundEffectPrefs();
        }
        
        #endregion


        #region SoundEffect Functions

        private GameObject CreateSoundEffect(AudioClip clip,Vector2 localpos)
        {
            GameObject go = new GameObject("SoundAudio:"+clip.name);
            go.transform.position = localpos;
            go.transform.SetParent(transform);
            go.AddComponent<SoundEffect>();
            AudioSource audioSource = go.AddComponent<AudioSource>();// todo txy
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSource.outputAudioMixerGroup = soundEffectMixerGroup;
            audioSource.clip = clip;
            audioSource.mute = !soundEffectOn;
            return go;
        }

        public SoundEffect GetSoundEffect(string name)
        {
            for (int i = 0; i < soundEffectPool.Count; i++)
            {
                if (soundEffectPool[i].Name == name && soundEffectPool[i].Singleton == true)
                {
                    return soundEffectPool[i];
                }
            }
            return null;
        }

        public AudioSource PlaySoundEffect(AudioClip clip, Vector2 localtion, float duration, float volume,
            bool singleton = false, float pitch = 1f, Action callback = null)
        {
            if (duration <= 0 || clip == null)
            {
                return null;
            }
            SoundEffect soundEffect = GetSoundEffect(clip.name);
            if (soundEffect != null)
            {
                soundEffect.Duration = soundEffect.Time = duration;
                return soundEffect.Source;
            }
            GameObject go = null;
            AudioSource source = null;
            go = CreateSoundEffect(clip, localtion);
            source = go.GetComponent<AudioSource>();
            source.loop = duration > clip.length;
            source.volume = soundEffectVolume * volume;
            source.pitch = pitch;    //todo txy
            SoundEffect effect = go.GetComponent<SoundEffect>();
            effect.Sequence = GetSequence();
            effect.Singleton = singleton;
            effect.Source = source;
            effect.OriginalVolume = volume;
            effect.Duration = effect.Time = duration;
            effect.Callback = callback;
            soundEffectPool.Add(effect);
            source.Play();
            return source;
        }
        
        public AudioSource PlaySoundEffect(AudioClip clip, Vector2 location, float duration, bool singleton = false, Action callback = null)
        {
            return PlaySoundEffect(clip, location, duration, soundEffectVolume, singleton, 1f, callback);
        }

        public AudioSource PlaySoundEffect(AudioClip clip, float duration, bool singleton = false, Action callback = null)
        {
            return PlaySoundEffect(clip, Vector2.zero, duration, soundEffectVolume, singleton, 1f, callback);
        }

        public AudioSource RepeatPlaySoundEffect(AudioClip clip, Vector2 location, int repeat, float volume,
            bool singleton = false, float pitch = 1f, Action callback = null)
        {
            if (clip == null)
            {
                return null;
            }
            if (repeat != 0)
            {
                SoundEffect soundEffect = GetSoundEffect(clip.name);
                if (soundEffect != null)
                {
                    soundEffect.Duration = soundEffect.Time = repeat > 0 ? clip.length * repeat : float.PositiveInfinity;
                    soundEffect.Source.loop = true;
                    return soundEffect.Source;
                }
                GameObject go = CreateSoundEffect(clip, location);
                AudioSource source = go.GetComponent<AudioSource>();
                source.loop = repeat != 0;
                source.volume = soundEffectVolume * volume;
                source.pitch = pitch;
                SoundEffect effect = go.GetComponent<SoundEffect>();
                effect.Sequence = GetSequence();
                effect.Singleton = singleton;
                effect.Source = source;
                effect.OriginalVolume = volume;
                effect.Duration = effect.Time = repeat > 0 ? clip.length * repeat : float.PositiveInfinity;
                effect.Callback = callback;
                soundEffectPool.Add(effect);
                source.Play();
				
                return source;
            }
            return PlayOneShot(clip, location, volume, pitch, callback);
        }
		
        public AudioSource RepeatPlaySoundEffect(AudioClip clip, Vector2 location, int repeat, bool singleton = false, Action callback = null)
        {
            return RepeatPlaySoundEffect(clip, location, repeat, soundEffectVolume, singleton, 1f, callback);
        }

        public AudioSource RepeatPlaySoundEffect(AudioClip clip, int repeat, bool singleton = false, Action callback = null)
        {
            return RepeatPlaySoundEffect(clip, Vector2.zero, repeat, soundEffectVolume, singleton, 1f, callback);
        }

        public AudioSource PlayOneShot(AudioClip clip, Vector2 location, float volume, float pitch = 1f,
            Action callback = null)
        {
            if (clip == null)
            {
                return null;
            }
            
            GameObject go = CreateSoundEffect(clip, location);
            AudioSource source = go.GetComponent<AudioSource>();
            source.loop = false;
            source.volume = soundEffectVolume * volume;
            source.pitch = pitch;
            SoundEffect effect = go.GetComponent<SoundEffect>();
            effect.Sequence = GetSequence();
            effect.Singleton = false;
            effect.Source = source;
            effect.OriginalVolume = volume;
            effect.Duration = effect.Time = clip.length;
            effect.Callback = callback;
            soundEffectPool.Add(effect);
            source.Play();
				
            return source;
        }
        
        public AudioSource PlayOneShot(AudioClip clip, Vector2 location, Action callback = null)
        {
            return PlayOneShot(clip, location, soundEffectVolume, 1f, callback);
        }

        public AudioSource PlayOneShot(AudioClip clip, Action callback = null)
        {
            return PlayOneShot(clip, Vector2.zero, soundEffectVolume, 1f, callback);
        }

        public void PauseAllSounfEffect()
        {
            foreach (SoundEffect soundEffect in soundEffectPool)
            {
                if (soundEffect != null)
                {
                    if (soundEffect.Source.isPlaying)
                    {
                        soundEffect.Source.Pause();
                    }
                }
            }
        }
        
        public void ResumeAllSounfEffect()
        {
            foreach (SoundEffect soundEffect in soundEffectPool)
            {
                if (soundEffect != null)
                {
                    if (soundEffect.Source.isPlaying)
                    {
                        soundEffect.Source.UnPause();
                    }
                }
            }
        }
        
        public void StopAllSounfEffect()
        {
            foreach (SoundEffect soundEffect in soundEffectPool)
            {
                if (soundEffect != null)
                {
                    if (soundEffect.Source.isPlaying)
                    {
                        soundEffect.Source.Stop();
                        DestroyImmediate(transform.gameObject);
                    }
                }
            }
            soundEffectPool.Clear();
        }
        
        #endregion

        
        
        #region Background Functions

        private void PlayBackgroundMusic(ref AudioSource audioSource, AudioClip cilp, float position, float pitch)
        {
            audioSource.clip = cilp;
            audioSource.time = position;
            audioSource.pitch = Mathf.Clamp(pitch, -3f, 3f);
            audioSource.Play();
        }

        private void PlayBackgroundMusic(AudioClip clip, float position, float pitch)
        {
            PlayBackgroundMusic(ref musicSource,clip,position,pitch);
            backgroundMusic.NextClip = null;
            backgroundMusic.CurrentClip = clip;
            if (crossFadeSource != null)
            {
                DestroyImmediate(crossFadeSource);
                crossFadeSource = null;
            }
        }

        public void PlayBackgroundMusic(AudioClip clip, MusicTransType transType, float duration, float volume,
            float pitch, float position = 0)
        {
            if (clip == null || backgroundMusic.CurrentClip == clip)
            {
                return;
            }
            if (backgroundMusic.CurrentClip == null || duration <= 0)
            {
                transType = MusicTransType.Swift;
            }
            if (transType == MusicTransType.Swift)
            {
                PlayBackgroundMusic(clip, position, pitch);
                SetBackgroundVolume(volume);
            }
            else
            {
                if (backgroundMusic.NextClip != null)
                {
                    return;
                }
                backgroundMusic.TransType = transType;
                transitionTime = backgroundMusic.TransitionDuration = duration;
                musicVolCap = musicVolume;
                backgroundMusic.NextClip = clip;
                if (backgroundMusic.TransType == MusicTransType.CrossFade)
                {
                    if (crossFadeSource != null)
                    {
                        return;
                    }
                    crossFadeSource = InitAudioSource(gameObject.AddComponent<AudioSource>());
                    crossFadeSource.volume = Mathf.Clamp01(musicVolCap - currentMusicVol);
                    crossFadeSource.priority = 0;
                    PlayBackgroundMusic(ref crossFadeSource,backgroundMusic.NextClip,0,pitch);
                }
            }
        }
        
        public void PlayBackgroundMusic(AudioClip clip, MusicTransType transition, float transition_duration, float volume)
        {
            PlayBackgroundMusic(clip, transition, transition_duration, volume, 1f);
        }

        public void PlayBackgroundMusic(AudioClip clip, MusicTransType transition, float transition_duration)
        {
            PlayBackgroundMusic(clip, transition, transition_duration, musicVolume, 1f);
        }

        public void PlayBackgroundMusic(AudioClip clip, MusicTransType transition)
        {
            PlayBackgroundMusic(clip, transition, 1f, musicVolume, 1f);
        }

        public void PlayBackgroundMusic(AudioClip clip)
        {
            PlayBackgroundMusic(clip, MusicTransType.Swift, 1f, musicVolume, 1f);
        }

        public void PlayBackgroundMusic(string clippath, MusicTransType transition, float duration, float volume, float pitch, float playback_position = 0)
        {
            PlayBackgroundMusic (LoadClip(clippath), transition, duration, volume, pitch, playback_position);
        }

        public void PlayBackgroundMusic(string clippath, MusicTransType transition, float duration, float volume)
        {
            PlayBackgroundMusic (LoadClip(clippath), transition, duration, volume, 1f);
        }
		
        public void PlayBackgroundMusic(string clippath, MusicTransType transition, float duration)
        {
            PlayBackgroundMusic(LoadClip(clippath), transition, duration, musicVolume, 1f);
        }

        public void PlayBackgroundMusic(string clippath, MusicTransType transition)
        {
            PlayBackgroundMusic(LoadClip(clippath), transition, 1f, musicVolume, 1f);
        }

        public void PlayBackgroundMusic(string clippath)
        {
            PlayBackgroundMusic(LoadClip(clippath), MusicTransType.Swift, 1f, musicVolume, 1f);
        }

        public void StopBackgroundMusic()
        {
            if (musicSource.isPlaying)
            {
                musicSource.Stop();
            }
        }

        public void PauseBackgroundMusic()
        {
            if (musicSource.isPlaying)
            {
                musicSource.Pause();
            }
        }

        public void ResumeBackgroundMusic()
        {
            if (!musicSource.isPlaying)
            {
                musicSource.UnPause();
            }
        }
        
        #endregion
        

        #region Load AudioClip

        private string GetAudioCilpName(string path)
        {
            string[] arr = path.Split('/');
            string fullname = arr[arr.Length - 1];
            return fullname;
        }
		
        public AudioClip LoadClip(string path, bool add_to_playlist = false)
        {
			
            AudioClip clip = null;
            clip = GetCilpFromDic(GetAudioCilpName(path));
            if (clip == null)
            {
                clip = Resources.Load(path) as AudioClip;
            }
            if (clip == null)
            {
                Debug.LogError (string.Format ("AudioClip '{0}' not found at location {1}", path, System.IO.Path.Combine (Application.dataPath, "/Resources/"+path)));
                return null;
            }

            if (add_to_playlist)
            {
                AddToCache(clip);
            }

            return clip;
        }

        public void LoadClip(string path, AudioType audio_type, bool add_to_playlist, Action<AudioClip> callback)
        {
            AudioClip clip = null;
            clip = GetCilpFromDic(GetAudioCilpName(path));
            if (clip == null)
            {
                StartCoroutine(LoadAudioClipFromUrl(path, audio_type, (downloadedContent) =>
                {
                    if (downloadedContent != null && add_to_playlist)
                    {
                        AddToCache(downloadedContent);
                    }

                    callback.Invoke(downloadedContent);
                }));
            }
            else
            {
                if (add_to_playlist)
                {
                    AddToCache(clip);
                }
                callback.Invoke(clip);
            }
			
        }

        IEnumerator LoadAudioClipFromUrl(string audio_url, AudioType audio_type, Action<AudioClip> callback)
        {
            using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(audio_url, audio_type))
            {
                yield return www.Send();

                if (www.isNetworkError)
                {
                    Debug.Log(string.Format("Error downloading audio clip at {0} : ", audio_url, www.error));
                }

                callback.Invoke(UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www));
            }
        }

        #endregion

        public override void Init()
        {
            base.Init();
            musicOn = LoadBackgroundStatus();
            musicVolume = useMusicOnStart ? musicVolume : LoadBackgroundVolume();
            soundEffectOn = LoadSoundEffectStatus();
            soundEffectVolume = useSoundEffectOnStart ? soundEffectVolume : LoadSoundEffectVolume();
            if (musicSource == null)
            {
                musicSource = gameObject.GetComponent<AudioSource>();
                if (musicSource == null)
                {
                    musicSource = gameObject.AddComponent<AudioSource>();
                }
            }
            musicSource = InitAudioSource(musicSource);
        }

        public void OnUpdate()
        {
            while (alive)
            {
                ManageSoundEffects();
                if (IsMusicAltered())
                {
                    ToggleBackground(!musicOn);
                    if (currentMusicVol != musicVolume)
                    {
                        currentMusicVol = musicVolume;
                    }
                    if (musicMixerGroup != null && !string.IsNullOrEmpty(volumeOfMusicMixer.Trim()))
                    {
                        float vol;
                        musicMixerGroup.audioMixer.GetFloat(volumeOfMusicMixer, out vol);
                        vol = NormaliseVolume(vol, true);
                        currentMusicVol = vol;
                    }
                    SetBackgroundVolume(currentMusicVol);
                }
                if (IsSoundEffectAltered())
                {
                    ToggleSoundEffect(!soundEffectOn);
                    if (currentSoundEffectVol != soundEffectVolume)
                    {
                        currentSoundEffectVol = soundEffectVolume;
                    }
                    if (soundEffectMixerGroup != null && !string.IsNullOrEmpty(volumeOfSoundEffectMixer.Trim()))
                    {
                        float vol;
                        soundEffectMixerGroup.audioMixer.GetFloat(volumeOfSoundEffectMixer, out vol);
                        vol = NormaliseVolume(vol, true);
                        currentSoundEffectVol = vol;
                    }
                    SetSoundEffectVolume(currentSoundEffectVol);
                }
                if (crossFadeSource != null)
                {
                    CrossFadeBackgroundMusic();
                }
                else if (backgroundMusic.NextClip != null)
                {
                    FadeOutFadeInBackgroundMusic();
                }
            }
        }

        private List<int> remIndex = new List<int>();
        private void ManageSoundEffects()
        {
            remIndex.Clear();
            for (int i = 0; i < soundEffectPool.Count; i++)
            {
                SoundEffect soundEffect = soundEffectPool[i];
                if (soundEffect.Source.isPlaying && !float.IsPositiveInfinity(soundEffect.Time))
                {
                    soundEffect.Time -= Time.deltaTime;
                }
                if (soundEffect.Time <= 0.0f)
                {
                    soundEffect.Source.Stop();
                    if (soundEffect.Callback != null)
                    {
                        soundEffect.Callback.Invoke();
                    }
                    remIndex.Add(i);
                }
            }
            if (remIndex.Count > 0)
            {
                for (int i = 0; i < remIndex.Count; i++)
                {
                    Destroy(soundEffectPool[remIndex[i]].gameObject);
                    soundEffectPool.RemoveAt(remIndex[i]);
                }
            }
        }

        private bool IsMusicAltered()
        {
            bool flag = musicOn != !musicSource.mute || currentMusicVol != musicVolume;
            if (musicMixerGroup != null && !string.IsNullOrEmpty(volumeOfMusicMixer.Trim()))
            {
                float vol;
                musicMixerGroup.audioMixer.GetFloat(volumeOfMusicMixer, out vol);
                vol = NormaliseVolume(vol, true);
                return flag || currentMusicVol != vol;
            }
            return flag;
        }

        private bool IsSoundEffectAltered()
        {
            bool flag = currentSoundEffectVol != soundEffectVolume;
            if (soundEffectMixerGroup != null && !string.IsNullOrEmpty(volumeOfSoundEffectMixer.Trim()))
            {
                float vol;
                soundEffectMixerGroup.audioMixer.GetFloat(volumeOfSoundEffectMixer, out vol);
                vol = NormaliseVolume(vol, true);
                return flag || currentSoundEffectVol != vol;
            }
            return flag;
        }

        private void CrossFadeBackgroundMusic()
        {
            if (backgroundMusic.TransType == MusicTransType.CrossFade)
            {
                if (musicSource.clip.name != backgroundMusic.NextClip.name)
                {
                    transitionTime -= Time.deltaTime;
                    musicSource.volume =
                        Mathf.Lerp(0, musicVolCap, transitionTime / backgroundMusic.TransitionDuration);
                    crossFadeSource.volume = Mathf.Clamp01(musicVolCap - musicSource.volume);
                    crossFadeSource.mute = musicSource.mute;
                    if (musicSource.volume <= 0.0f)
                    {
                        SetBackgroundVolume(musicVolCap);
                        PlayBackgroundMusic(backgroundMusic.NextClip,crossFadeSource.time,crossFadeSource.pitch);
                    }
                }
            }
        }

        private void FadeOutFadeInBackgroundMusic()
        {
            if (backgroundMusic.TransType == MusicTransType.LinearFade)
            {
                //FadeIn
                if (musicSource.clip.name == backgroundMusic.NextClip.name)
                {
                    transitionTime += Time.deltaTime;
                    musicSource.volume =
                        Mathf.Lerp(0, musicVolCap, transitionTime / backgroundMusic.TransitionDuration);
                    if (musicSource.volume >= musicVolCap)
                    {
                        SetBackgroundVolume(musicVolCap);
                        PlayBackgroundMusic(backgroundMusic.NextClip,musicSource.time,musicSource.pitch);
                    }
                }
                else
                //FadeOut
                {
                    transitionTime -= Time.deltaTime;
                    musicSource.volume =
                        Mathf.Lerp(0, musicVolCap, transitionTime / backgroundMusic.TransitionDuration);
                    if (musicSource.volume <= 0.0f)
                    {
                        musicSource.volume = transitionTime = 0;
                        PlayBackgroundMusic(ref musicSource,backgroundMusic.NextClip,0,musicSource.pitch);
                    }
                }
            }
        }

        public void SetAlive(bool b)
        {
            alive = b;
        }
        
        public override void OnDestory()
        {
            base.OnDestory();
            alive = false;
            SaveAllPrefs();
        }
        
    }
}
