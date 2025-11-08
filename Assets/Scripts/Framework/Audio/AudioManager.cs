using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Linq;
using DG.Tweening;

namespace GuiFramework
{
    public class AudioManager : SingletonPersistent<AudioManager>
    {
        [Header("音频配置")]
        public AudioDatas audioDatas;
        public AudioMixer audioMixer;
        
        [Header("场景音频配置")]
        public SceneAudioConfig defaultSceneConfig;
        
        [Header("音频池设置")]
        public int maxAudioSources = 20;
        public bool enableAudioPooling = true;
        
        // 当前场景配置
        private SceneAudioConfig currentSceneConfig;
        
        // 统一使用分类字典管理所有音频
        private Dictionary<AudioCategory, List<AudioInfo>> categorizedAudio = new Dictionary<AudioCategory, List<AudioInfo>>();
        
        // 音频对象池（只用于SFX，BGM不参与对象池）
        private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
        private Transform audioPoolRoot;
        private Transform _bgmSourcesRootGO;
        private Transform _sfxSourcesRootGO;
        
        // 音量设置
        public float mainVolume { get; private set; }
        public float bgmVolumeFactor { get; private set; }
        public float sfxVolumeFactor { get; private set; }
        
        private const string BGM_VOLUME_PARAM = "BGM";
        private const string SFX_VOLUME_PARAM = "Sfx";

        protected override void Awake()
        {
            base.Awake();
            InitializeAudioSystem();
        }
        
        private void InitializeAudioSystem()
        {
            // 创建根节点
            _bgmSourcesRootGO = new GameObject("BGM_ROOT").transform;
            _sfxSourcesRootGO = new GameObject("SFX_ROOT").transform;
            _bgmSourcesRootGO.SetParent(transform);
            _sfxSourcesRootGO.SetParent(transform);
            
            // 初始化分类字典
            foreach (AudioCategory category in System.Enum.GetValues(typeof(AudioCategory)))
            {
                categorizedAudio[category] = new List<AudioInfo>();
            }
            
            // 创建对象池（只用于SFX）
            audioPoolRoot = new GameObject("AudioPool").transform;
            audioPoolRoot.SetParent(transform);
            
            for (int i = 0; i < maxAudioSources; i++)
            {
                CreatePooledAudioSource();
            }
            
            // 注册场景加载事件
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            // 加载音量设置
            LoadVolumeSettings();
            
            // 初始化当前场景音频
            LoadSceneAudioConfig(SceneManager.GetActiveScene().name);
        }
        
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            LoadSceneAudioConfig(scene.name);
        }

        #region 场景音频管理功能
        
        public void LoadSceneAudioConfig(string sceneName)
        {
            // 停止当前场景的所有非持久化音频
            StopNonPersistentAudio();
            
            // 加载新场景的音频配置
            string configPath = $"AudioConfigs/{sceneName}";
            currentSceneConfig = Resources.Load<SceneAudioConfig>(configPath);
            
            if (currentSceneConfig != null)
            {
                InitializeSceneAudio();
                Debug.Log($"Loaded audio config for scene: {sceneName}");
            }
            else
            {
                currentSceneConfig = defaultSceneConfig;
                if (currentSceneConfig != null)
                {
                    InitializeSceneAudio();
                    Debug.Log($"Using default audio config for scene: {sceneName}");
                }
                else
                {
                    Debug.LogWarning($"No audio config found for scene: {sceneName}");
                }
            }
        }
        
        private void StopNonPersistentAudio()
        {
            // 停止所有非BGM音频
            StopCategoryAudio(AudioCategory.UI, true);
            StopCategoryAudio(AudioCategory.Environment, true);
            StopCategoryAudio(AudioCategory.Character, true);
            StopCategoryAudio(AudioCategory.SpecialEffect, true);
            StopCategoryAudio(AudioCategory.Ambient, true);
        }
        
        private void InitializeSceneAudio()
        {
            // 播放默认BGM
            if (currentSceneConfig.defaultBGM != null)
            {
                PlayBgm(currentSceneConfig.defaultBGM.audioName, "", 2f, 1f);
            }
            
            // 设置音频混合
            if (currentSceneConfig.defaultSnapshot != null)
            {
                currentSceneConfig.defaultSnapshot.TransitionTo(1f);
            }
            
            // 设置场景特定的音量
            ChangeBgmVolume(currentSceneConfig.bgmVolume);
            ChangeSfxVolume(currentSceneConfig.sfxVolume);
        }
        
        public void PlaySceneSFX(string sfxName)
        {
            if (currentSceneConfig != null)
            {
                var sfxData = currentSceneConfig.sceneSFX.Find(x => x.audioName == sfxName);
                if (sfxData != null)
                {
                    PlaySfx(sfxName);
                }
                else
                {
                    Debug.LogWarning($"SFX {sfxName} not found in current scene config");
                }
            }
        }
        
        public SceneAudioConfig GetCurrentSceneConfig()
        {
            return currentSceneConfig;
        }
        
        public void SwitchToSceneConfig(SceneAudioConfig newConfig)
        {
            if (newConfig != null)
            {
                currentSceneConfig = newConfig;
                InitializeSceneAudio();
            }
        }

        #endregion

        #region 核心音频功能 - BGM控制
        
        public void PlayBgm(string fadeInMusicName, string fadeOutMusicName = "", float fadeInDuration = 0.5f,
            float fadeOutDuration = 0.5f, bool loop = true)
        {
            Sequence s = DOTween.Sequence();

            // 淡出指定的BGM
            if (!string.IsNullOrEmpty(fadeOutMusicName))
            {
                var fadeOutInfo = GetAudioInfo(fadeOutMusicName, AudioCategory.BackgroundMusic);
                if (fadeOutInfo != null)
                {
                    s.Append(fadeOutInfo.audioSource.DOFade(0, fadeOutDuration).OnComplete(() =>
                    {
                        fadeOutInfo.audioSource.Pause();
                    }));
                }
            }

            // 检查是否已存在需要播放的BGM
            var existingInfo = GetAudioInfo(fadeInMusicName, AudioCategory.BackgroundMusic);
            if (existingInfo != null)
            {
                existingInfo.audioSource.volume = 0;
                existingInfo.audioSource.Play();
                s.Append(existingInfo.audioSource.DOFade(mainVolume * bgmVolumeFactor, fadeInDuration));
                return;
            }

            // 从资源加载并播放新的BGM
            AudioData fadeInData = audioDatas.audioDataList.Find(x => x.audioName == fadeInMusicName);
            if (fadeInData == null)
            {
                Debug.LogWarning("未找到BGM：" + fadeInMusicName);
                return;
            }

            GameObject fadeInAudioGO = new GameObject(fadeInMusicName);
            fadeInAudioGO.transform.SetParent(_bgmSourcesRootGO);

            AudioSource fadeInAudioSource = fadeInAudioGO.AddComponent<AudioSource>();
            fadeInAudioSource.clip = Resources.Load<AudioClip>(fadeInData.audioPath);
            fadeInAudioSource.loop = loop;
            fadeInAudioSource.volume = fadeInDuration > 0 ? 0 : mainVolume * bgmVolumeFactor;

            fadeInAudioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Master")[1];
            fadeInAudioSource.Play();

            if (fadeInDuration > 0)
            {
                s.Append(fadeInAudioSource.DOFade(mainVolume * bgmVolumeFactor, fadeInDuration));
            }

            // 添加到分类系统
            AudioInfo info = new AudioInfo(fadeInMusicName, fadeInAudioSource, AudioCategory.BackgroundMusic);
            categorizedAudio[AudioCategory.BackgroundMusic].Add(info);
            StartCoroutine(DetectingAudioPlayState(info));
        }

        public void PauseBgm(string pauseBgmName, float fadeOutDuration = 0.5f)
        {
            var audioInfo = GetAudioInfo(pauseBgmName, AudioCategory.BackgroundMusic);
            if (audioInfo != null)
            {
                Sequence s = DOTween.Sequence();
                s.Append(audioInfo.audioSource.DOFade(0, fadeOutDuration).OnComplete(() =>
                {
                    audioInfo.audioSource.Pause();
                }));
            }
            else
            {
                Debug.LogWarning("未找到BGM：" + pauseBgmName);
            }
        }

        public void StopBgm(string stopBgmName, float fadeOutDuration = 0.5f)
        {
            var audioInfo = GetAudioInfo(stopBgmName, AudioCategory.BackgroundMusic);
            if (audioInfo != null)
            {
                Sequence s = DOTween.Sequence();
                s.Append(audioInfo.audioSource.DOFade(0, fadeOutDuration).OnComplete(() =>
                {
                    audioInfo.audioSource.Stop();
                    Destroy(audioInfo.audioSource.gameObject);
                    categorizedAudio[AudioCategory.BackgroundMusic].Remove(audioInfo);
                }));
            }
            else
            {
                Debug.LogWarning("未找到BGM：" + stopBgmName);
            }
        }

        public void StopAllBGM(float fadeOutDuration = 0.5f)
        {
            var bgmList = categorizedAudio[AudioCategory.BackgroundMusic].ToArray();
            foreach (var audioInfo in bgmList)
            {
                StopBgm(audioInfo.audioName, fadeOutDuration);
            }
        }

        #endregion

        #region 核心音频功能 - SFX控制
        
        public void PlaySfx(string sfxName, bool loop = false)
        {
            AudioData sfxData = audioDatas.audioDataList.Find(x => x.audioName == sfxName);
            if (sfxData == null)
            {
                Debug.LogWarning("未找到sfx：" + sfxName);
                return;
            }

            AudioSource sfxAudioSource = GetPooledAudioSource();
            if (sfxAudioSource == null) return;

            sfxAudioSource.transform.SetParent(_sfxSourcesRootGO);
            sfxAudioSource.name = sfxName;
            sfxAudioSource.clip = Resources.Load<AudioClip>(sfxData.audioPath);
            sfxAudioSource.loop = loop;
            sfxAudioSource.volume = mainVolume * sfxVolumeFactor;
            sfxAudioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Master")[2];
            sfxAudioSource.Play();

            // 根据音效名称判断分类（简单实现，可根据需要扩展）
            AudioCategory category = GetSFXCategory(sfxName);
            AudioInfo info = new AudioInfo(sfxName, sfxAudioSource, category);
            categorizedAudio[category].Add(info);
            
            StartCoroutine(DetectingAudioPlayState(info));
        }

        public void PauseSfx(string pauseSfxName)
        {
            var audioInfo = FindAudioInfo(pauseSfxName);
            if (audioInfo != null)
            {
                audioInfo.audioSource.Pause();
            }
            else
            {
                Debug.LogWarning("未找到sfx：" + pauseSfxName);
            }
        }

        public void StopSfx(string stopSfxName)
        {
            var audioInfo = FindAudioInfo(stopSfxName);
            if (audioInfo != null && audioInfo.category != AudioCategory.BackgroundMusic)
            {
                audioInfo.audioSource.Stop();
                ReturnAudioSourceToPool(audioInfo.audioSource);
                categorizedAudio[audioInfo.category].Remove(audioInfo);
            }
            else
            {
                Debug.LogWarning("未找到sfx：" + stopSfxName);
            }
        }

        #endregion

        #region 分类音频控制
        
        public void PlayCategorizedAudio(string audioName, AudioCategory category, Vector3 position = default)
        {
            AudioData audioData = audioDatas.audioDataList.Find(x => x.audioName == audioName);
            if (audioData == null)
            {
                Debug.LogWarning("未找到音频：" + audioName);
                return;
            }

            // BGM和SFX使用不同的创建方式
            if (category == AudioCategory.BackgroundMusic)
            {
                PlayBgm(audioName);
                return;
            }

            AudioSource audioSource = GetPooledAudioSource();
            if (audioSource == null) return;

            audioSource.transform.position = position;
            audioSource.transform.SetParent(_sfxSourcesRootGO);
            audioSource.name = audioName;

            audioSource.clip = Resources.Load<AudioClip>(audioData.audioPath);
            audioSource.volume = mainVolume * GetCategoryVolumeFactor(category);
            audioSource.loop = category == AudioCategory.Ambient; // 只有环境音循环
            audioSource.outputAudioMixerGroup = GetMixerGroupForCategory(category);
            audioSource.Play();

            AudioInfo audioInfo = new AudioInfo(audioName, audioSource, category);
            categorizedAudio[category].Add(audioInfo);
            
            StartCoroutine(DetectingAudioPlayState(audioInfo));
        }
        
        public void StopCategoryAudio(AudioCategory category, bool fadeOut = false)
        {
            if (categorizedAudio.ContainsKey(category))
            {
                foreach (var audioInfo in categorizedAudio[category].ToArray())
                {
                    if (fadeOut && category == AudioCategory.BackgroundMusic)
                    {
                        StartCoroutine(FadeOutAndStop(audioInfo, 0.5f));
                    }
                    else
                    {
                        StopAudioImmediate(audioInfo);
                    }
                }
            }
        }
        
        public void PauseCategoryAudio(AudioCategory category)
        {
            if (categorizedAudio.ContainsKey(category))
            {
                foreach (var audioInfo in categorizedAudio[category])
                {
                    if (audioInfo.audioSource != null && audioInfo.audioSource.isPlaying)
                    {
                        audioInfo.audioSource.Pause();
                    }
                }
            }
        }
        
        public void ResumeCategoryAudio(AudioCategory category)
        {
            if (categorizedAudio.ContainsKey(category))
            {
                foreach (var audioInfo in categorizedAudio[category])
                {
                    if (audioInfo.audioSource != null && !audioInfo.audioSource.isPlaying)
                    {
                        audioInfo.audioSource.Play();
                    }
                }
            }
        }

        #endregion

        #region 音量控制（保持不变）
        
        public void ChangeMainVolume(float volume)
        {
            mainVolume = volume;
            PlayerPrefs.SetFloat("MainVolume", mainVolume);

            // 更新所有音频源音量
            foreach (var categoryList in categorizedAudio.Values)
            {
                foreach (var audioInfo in categoryList)
                {
                    if (audioInfo.audioSource != null)
                    {
                        audioInfo.audioSource.volume = mainVolume * GetCategoryVolumeFactor(audioInfo.category);
                    }
                }
            }

            ChangeBgmVolume(bgmVolumeFactor);
            ChangeSfxVolume(sfxVolumeFactor);
        }

        public void ChangeBgmVolume(float factor)
        {
            bgmVolumeFactor = factor;
            bgmVolumeFactor = Mathf.Clamp(bgmVolumeFactor, 0f, 1f);
            PlayerPrefs.SetFloat("BgmVolumeFactor", bgmVolumeFactor);

            if (bgmVolumeFactor == 0)
            {
                audioMixer.SetFloat(BGM_VOLUME_PARAM, -80f);
            }
            else
            {
                audioMixer.SetFloat(BGM_VOLUME_PARAM, Mathf.Log10(mainVolume * bgmVolumeFactor) * 20);
            }
        }

        public void ChangeSfxVolume(float factor)
        {
            sfxVolumeFactor = factor;
            sfxVolumeFactor = Mathf.Clamp(sfxVolumeFactor, 0f, 1f);
            PlayerPrefs.SetFloat("SfxVolumeFactor", sfxVolumeFactor);

            if (sfxVolumeFactor == 0)
            {
                audioMixer.SetFloat(SFX_VOLUME_PARAM, -80f);
            }
            else
            {
                audioMixer.SetFloat(SFX_VOLUME_PARAM, Mathf.Log10(mainVolume * sfxVolumeFactor) * 20);
            }
        }

        #endregion

        #region 内部辅助方法
        
        private void LoadVolumeSettings()
        {
            mainVolume = PlayerPrefs.GetFloat("MainVolume", 1f);
            bgmVolumeFactor = PlayerPrefs.GetFloat("BgmVolumeFactor", 0.8f);
            sfxVolumeFactor = PlayerPrefs.GetFloat("SfxVolumeFactor", 0.8f);
            
            ChangeBgmVolume(bgmVolumeFactor);
            ChangeSfxVolume(sfxVolumeFactor);
        }
        
        private void CreatePooledAudioSource()
        {
            GameObject audioGO = new GameObject("PooledAudioSource");
            audioGO.transform.SetParent(audioPoolRoot);
            
            AudioSource audioSource = audioGO.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.Stop();
            
            audioSourcePool.Enqueue(audioSource);
        }
        
        private AudioSource GetPooledAudioSource()
        {
            if (audioSourcePool.Count > 0)
            {
                return audioSourcePool.Dequeue();
            }
            
            if (enableAudioPooling)
            {
                CreatePooledAudioSource();
                return audioSourcePool.Dequeue();
            }
            
            return null;
        }
        
        private void ReturnAudioSourceToPool(AudioSource audioSource)
        {
            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.clip = null;
                audioSource.outputAudioMixerGroup = null;
                audioSourcePool.Enqueue(audioSource);
            }
        }
        
        private float GetCategoryVolumeFactor(AudioCategory category)
        {
            return category == AudioCategory.BackgroundMusic ? bgmVolumeFactor : sfxVolumeFactor;
        }
        
        private AudioMixerGroup GetMixerGroupForCategory(AudioCategory category)
        {
            var groups = audioMixer.FindMatchingGroups("Master");
            return category == AudioCategory.BackgroundMusic ? 
                (groups.Length > 1 ? groups[1] : null) : 
                (groups.Length > 2 ? groups[2] : null);
        }
        
        private AudioInfo GetAudioInfo(string audioName, AudioCategory category)
        {
            return categorizedAudio[category].Find(x => x.audioName == audioName);
        }
        
        private AudioInfo FindAudioInfo(string audioName)
        {
            foreach (var categoryList in categorizedAudio.Values)
            {
                var audioInfo = categoryList.Find(x => x.audioName == audioName);
                if (audioInfo != null) return audioInfo;
            }
            return null;
        }
        
        private AudioCategory GetSFXCategory(string sfxName)
        {
            // 简单的分类逻辑，可根据需要扩展
            if (sfxName.Contains("UI") || sfxName.Contains("Button")) return AudioCategory.UI;
            if (sfxName.Contains("Env") || sfxName.Contains("Nature")) return AudioCategory.Environment;
            if (sfxName.Contains("Char") || sfxName.Contains("Player")) return AudioCategory.Character;
            if (sfxName.Contains("Effect") || sfxName.Contains("Magic")) return AudioCategory.SpecialEffect;
            return AudioCategory.UI; // 默认
        }

        #endregion

        #region 协程方法
        
        private IEnumerator DetectingAudioPlayState(AudioInfo info)
        {
            AudioSource audioSource = info.audioSource;
            while (audioSource != null && audioSource.isPlaying)
            {
                yield return null;
            }

            if (audioSource != null)
            {
                categorizedAudio[info.category].Remove(info);
                
                if (info.category == AudioCategory.BackgroundMusic)
                {
                    Destroy(audioSource.gameObject);
                }
                else
                {
                    ReturnAudioSourceToPool(audioSource);
                }
            }
        }
        
        private IEnumerator FadeOutAndStop(AudioInfo audioInfo, float duration)
        {
            AudioSource audioSource = audioInfo.audioSource;
            if (audioSource == null) yield break;
            
            float startVolume = audioSource.volume;
            float timer = 0f;
            
            while (timer < duration && audioSource != null)
            {
                timer += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
                yield return null;
            }
            
            if (audioSource != null)
            {
                StopAudioImmediate(audioInfo);
            }
        }
        
        private void StopAudioImmediate(AudioInfo audioInfo)
        {
            if (audioInfo.audioSource != null)
            {
                audioInfo.audioSource.Stop();
                categorizedAudio[audioInfo.category].Remove(audioInfo);
                
                if (audioInfo.category == AudioCategory.BackgroundMusic)
                {
                    Destroy(audioInfo.audioSource.gameObject);
                }
                else
                {
                    ReturnAudioSourceToPool(audioInfo.audioSource);
                }
            }
        }

        #endregion

        #region 工具方法
        
        public List<string> GetActiveBgmList()
        {
            return categorizedAudio[AudioCategory.BackgroundMusic]
                .Select(x => x.audioName).ToList();
        }
        
        public List<string> GetActiveSfxList()
        {
            var sfxList = new List<string>();
            foreach (var category in categorizedAudio.Keys)
            {
                if (category != AudioCategory.BackgroundMusic)
                {
                    sfxList.AddRange(categorizedAudio[category].Select(x => x.audioName));
                }
            }
            return sfxList;
        }
        
        public void ForceCleanup()
        {
            StopAllBGM(0.1f);
            foreach (var category in categorizedAudio.Keys.ToList())
            {
                if (category != AudioCategory.BackgroundMusic)
                {
                    StopCategoryAudio(category);
                }
            }
        }

        #endregion
    }
}