using UnityEngine;

namespace GuiFramework
{
    [System.Serializable]
    public class AudioInfo
    {
        public string audioName;
        public AudioSource audioSource;
        public AudioCategory category;
        public bool isPlaying => audioSource != null && audioSource.isPlaying;
        
        // 淡入淡出相关
        public Coroutine fadeCoroutine;
        public float originalVolume;
        
        public AudioInfo(string name, AudioSource source, AudioCategory cat = AudioCategory.UI)
        {
            audioName = name;
            audioSource = source;
            category = cat;
            if (source != null)
            {
                originalVolume = source.volume;
            }
        }
    }
}