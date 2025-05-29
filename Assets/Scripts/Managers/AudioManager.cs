using UnityEngine;
using System.Collections.Generic;

namespace Lineage.Ancestral.Legacies.Managers
{
    /// <summary>
    /// Manages audio sources, sound effects, music, and audio settings.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource ambientSource;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip[] backgroundMusic;
        [SerializeField] private AudioClip buttonClickSFX;
        [SerializeField] private AudioClip popSelectSFX;
        [SerializeField] private AudioClip popMoveSFX;
        [SerializeField] private AudioClip miracleSFX;
        [SerializeField] private AudioClip forageSFX;
        [SerializeField] private AudioClip ambientForest;

        [Header("Settings")]
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float musicVolume = 0.7f;
        [Range(0f, 1f)] public float sfxVolume = 0.8f;
        [Range(0f, 1f)] public float ambientVolume = 0.5f;

        private int currentMusicIndex = 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudio();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            PlayBackgroundMusic();
            PlayAmbientSounds();
        }

        private void InitializeAudio()
        {
            // Create audio sources if they don't exist
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }

            if (ambientSource == null)
            {
                ambientSource = gameObject.AddComponent<AudioSource>();
                ambientSource.loop = true;
                ambientSource.playOnAwake = false;
            }

            UpdateVolumes();
        }

        public void UpdateVolumes()
        {
            if (musicSource != null)
                musicSource.volume = masterVolume * musicVolume;
            
            if (sfxSource != null)
                sfxSource.volume = masterVolume * sfxVolume;
            
            if (ambientSource != null)
                ambientSource.volume = masterVolume * ambientVolume;
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }

        public void PlayButtonClick()
        {
            PlaySFX(buttonClickSFX);
        }

        public void PlayPopSelect()
        {
            PlaySFX(popSelectSFX);
        }

        public void PlayPopMove()
        {
            PlaySFX(popMoveSFX);
        }

        public void PlayMiracle()
        {
            PlaySFX(miracleSFX);
        }

        public void PlayForage()
        {
            PlaySFX(forageSFX);
        }

        public void PlayBackgroundMusic()
        {
            if (backgroundMusic != null && backgroundMusic.Length > 0 && musicSource != null)
            {
                musicSource.clip = backgroundMusic[currentMusicIndex];
                musicSource.Play();
            }
        }

        public void NextTrack()
        {
            if (backgroundMusic != null && backgroundMusic.Length > 0)
            {
                currentMusicIndex = (currentMusicIndex + 1) % backgroundMusic.Length;
                PlayBackgroundMusic();
            }
        }

        public void PlayAmbientSounds()
        {
            if (ambientForest != null && ambientSource != null)
            {
                ambientSource.clip = ambientForest;
                ambientSource.Play();
            }
        }

        public void StopMusic()
        {
            if (musicSource != null)
                musicSource.Stop();
        }

        public void StopAmbient()
        {
            if (ambientSource != null)
                ambientSource.Stop();
        }

        // Called when music track ends
        private void Update()
        {
            if (musicSource != null && !musicSource.isPlaying && backgroundMusic != null && backgroundMusic.Length > 1)
            {
                NextTrack();
            }
        }
    }
}
