using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace GuiFramework
{
    [CreateAssetMenu(fileName = "New SceneAudioConfig", menuName = "CustomizedSO/SceneAudioConfig")]
    public class SceneAudioConfig : ScriptableObject
    {
        public string sceneName;
        
        [Header("背景音乐配置")]
        public AudioData defaultBGM;
        public List<AudioData> alternativeBGM;
        
        [Header("场景音效配置")]
        public List<AudioData> sceneSFX;
        
        [Header("音频混合设置")]
        public AudioMixerSnapshot defaultSnapshot;
        public float bgmVolume = 0.8f;
        public float sfxVolume = 0.8f;
    }
}