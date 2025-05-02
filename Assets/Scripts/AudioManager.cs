using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class Sound
    {
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
    }
    
    [Header("Sound Effects")]
    [SerializeField] private Sound cardFlipSound;
    [SerializeField] private Sound cardMatchSound;
    [SerializeField] private Sound cardMismatchSound;
    [SerializeField] private Sound gameOverSound;
    
    [Header("Audio Pool Settings")]
    [SerializeField] private int audioSourcePoolSize = 5;

    // Audio source pool for efficient audio playback
    private List<AudioSource> audioSourcePool = new List<AudioSource>();
    
    private void Awake()
    {
        InitializeAudioSourcePool();
    }
    
    private void InitializeAudioSourcePool()
    {
        // Create audio source pool
        for (int i = 0; i < audioSourcePoolSize; i++)
        {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSourcePool.Add(audioSource);
        }
    }
    
    private AudioSource GetAvailableAudioSource()
    {
        // Find an available audio source that's not playing
        foreach (AudioSource source in audioSourcePool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        
        // If all sources are playing, use the oldest one (first in the list)
        AudioSource oldestSource = audioSourcePool[0];
        
        // Move this source to the end of the list for next time
        audioSourcePool.RemoveAt(0);
        audioSourcePool.Add(oldestSource);
        
        oldestSource.Stop(); // Stop it before reusing
        return oldestSource;
    }
    
    public void PlayCardFlip()
    {
        PlaySound(cardFlipSound);
    }
    
    public void PlayCardMatch()
    {
        PlaySound(cardMatchSound);
    }
    
    public void PlayCardMismatch()
    {
        PlaySound(cardMismatchSound);
    }
    
    public void PlayGameOver()
    {
        PlaySound(gameOverSound);
    }
    
    private void PlaySound(Sound sound)
    {
        if (sound == null || sound.clip == null)
            return;
            
        AudioSource audioSource = GetAvailableAudioSource();
        audioSource.clip = sound.clip;
        audioSource.volume = sound.volume;
        audioSource.Play();
    }

    // Optimize memory on mobile platforms
    public void OptimizeForMobile()
    {
        #if UNITY_ANDROID || UNITY_IOS
        // Reduce buffer size for mobile
        foreach (AudioSource source in audioSourcePool)
        {
            // Lower buffer size to save memory
            AudioSettings.SetDSPBufferSize(512, 2);
        }
        #endif
    }
    
    private void OnDestroy()
    {
        // Clean up for memory management
        foreach (AudioSource source in audioSourcePool)
        {
            if (source != null)
            {
                source.Stop();
                source.clip = null;
            }
        }
        audioSourcePool.Clear();
    }
}