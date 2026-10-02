using UnityEngine;

namespace LightNShadows
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        [Header("Background Music")]
        [SerializeField] private AudioClip backgroundMusic;
        [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.5f;

        [Header("Custom Sound Effects")]
        [SerializeField] private AudioClip customButtonClick;

        private AudioSource audioSource; // Compatibility alias
        private AudioClip swapClip;
        private AudioClip jumpClip;
        private AudioClip phaseClip;
        private AudioClip deathClip;
        private AudioClip clickClip;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitAudioSources();
            GenerateProceduralAudioClips();
            InitButtonClick();
            InitMusic();
        }

        private void InitAudioSources()
        {
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
            audioSource = sfxSource;

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = true;
                musicSource.volume = musicVolume;
            }
        }

        private void InitButtonClick()
        {
            if (customButtonClick != null)
            {
                clickClip = customButtonClick;
                return;
            }

            AudioClip loaded = Resources.Load<AudioClip>("Audio/ButtonClick");
#if UNITY_EDITOR
            if (loaded == null)
            {
                loaded = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/ButtonClick.mp3");
            }
#endif
            if (loaded != null)
            {
                clickClip = loaded;
                Debug.Log("<color=green>[SoundManager]</color> Custom button click audio clip loaded!");
                return;
            }

            StartCoroutine(LoadButtonClickFallback());
        }

        private System.Collections.IEnumerator LoadButtonClickFallback()
        {
            string[] clickPaths = new string[]
            {
                System.IO.Path.Combine(Application.dataPath, "Audio/ButtonClick.mp3"),
                System.IO.Path.Combine(Application.dataPath, "Resources/Audio/ButtonClick.mp3"),
                @"C:\Users\sarah\.gemini\antigravity\brain\dd098baa-b450-4d87-950b-5edbdecc7adb\.user_uploaded\uploaded_media_0_1790928409215.mp3"
            };

            foreach (string p in clickPaths)
            {
                if (System.IO.File.Exists(p))
                {
                    using (var uwr = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip("file://" + p, AudioType.MPEG))
                    {
                        yield return uwr.SendWebRequest();
                        if (uwr.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                        {
                            AudioClip clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(uwr);
                            if (clip != null)
                            {
                                clickClip = clip;
                                Debug.Log("<color=green>[SoundManager]</color> Successfully loaded custom button click from file!");
                                yield break;
                            }
                        }
                    }
                }
            }
        }

        private void InitMusic()
        {
            AudioClip customMusic = Resources.Load<AudioClip>("Audio/BackgroundMusic");

#if UNITY_EDITOR
            if (customMusic == null)
            {
                customMusic = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BackgroundMusic.mp3");
            }
#endif

            if (customMusic != null)
            {
                backgroundMusic = customMusic;
                StartMusic(backgroundMusic);
                return;
            }

            if (backgroundMusic != null)
            {
                StartMusic(backgroundMusic);
                return;
            }

            StartCoroutine(LoadMusicFallback());
        }

        private System.Collections.IEnumerator LoadMusicFallback()
        {
            string[] musicPaths = new string[]
            {
                System.IO.Path.Combine(Application.dataPath, "Audio/BackgroundMusic.mp3"),
                System.IO.Path.Combine(Application.dataPath, "Resources/Audio/BackgroundMusic.mp3"),
                @"C:\Users\sarah\.gemini\antigravity\brain\dd098baa-b450-4d87-950b-5edbdecc7adb\.user_uploaded\uploaded_media_1_1790928409215.mp3",
                System.IO.Path.Combine(Application.dataPath, "Audio/Music_LightNShadows.wav"),
                System.IO.Path.Combine(Application.dataPath, "Resources/Audio/Music_LightNShadows.wav")
            };

            foreach (string p in musicPaths)
            {
                if (System.IO.File.Exists(p))
                {
                    AudioType aType = p.EndsWith(".mp3", System.StringComparison.OrdinalIgnoreCase) 
                        ? AudioType.MPEG 
                        : AudioType.WAV;

                    using (var uwr = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip("file://" + p, aType))
                    {
                        yield return uwr.SendWebRequest();
                        if (uwr.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                        {
                            AudioClip clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(uwr);
                            if (clip != null)
                            {
                                backgroundMusic = clip;
                                StartMusic(backgroundMusic);
                                yield break;
                            }
                        }
                    }
                }
            }
        }

        private void StartMusic(AudioClip clip)
        {
            if (musicSource != null && clip != null)
            {
                musicSource.clip = clip;
                musicSource.loop = true;
                musicSource.volume = musicVolume;
                musicSource.Play();
                Debug.Log("<color=cyan>[LightNShadows]</color> Background music started playing!");
            }
        }

        public void PlayMusic()
        {
            if (musicSource != null && !musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }

        public void PauseMusic()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Pause();
            }
        }

        public void ResumeMusic()
        {
            if (musicSource != null)
            {
                musicSource.UnPause();
            }
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
            }
        }

        public void PlaySwap()
        {
            if (sfxSource != null && swapClip != null)
            {
                sfxSource.pitch = Random.Range(0.95f, 1.05f);
                sfxSource.PlayOneShot(swapClip, 0.65f);
            }
        }

        public void PlayJump()
        {
            if (sfxSource != null && jumpClip != null)
            {
                sfxSource.pitch = Random.Range(0.98f, 1.02f);
                sfxSource.PlayOneShot(jumpClip, 0.5f);
            }
        }

        public void PlayPhase()
        {
            if (sfxSource != null && phaseClip != null)
            {
                sfxSource.pitch = Random.Range(0.95f, 1.05f);
                sfxSource.PlayOneShot(phaseClip, 0.6f);
            }
        }

        public void PlayDeath()
        {
            if (sfxSource != null && deathClip != null)
            {
                sfxSource.pitch = 1.0f;
                sfxSource.PlayOneShot(deathClip, 0.9f);
            }
        }

        public void PlayButtonClick()
        {
            if (sfxSource != null && clickClip != null)
            {
                sfxSource.pitch = 1.0f;
                sfxSource.PlayOneShot(clickClip, 0.90f);
            }
        }

        private void GenerateProceduralAudioClips()
        {
            int sampleRate = 44100;

            // 1. Swap Realm: Smooth futuristic frequency sweep (whoosh/laser)
            swapClip = CreateClip("Swap", 0.14f, sampleRate, (t, duration) =>
            {
                float progress = t / duration;
                float freq = Mathf.Lerp(300f, 900f, progress);
                float env = Mathf.Sin(progress * Mathf.PI); // Envelope
                return Mathf.Sin(2f * Mathf.PI * freq * t) * env;
            });

            // 2. Jump: Punchy upward blip
            jumpClip = CreateClip("Jump", 0.12f, sampleRate, (t, duration) =>
            {
                float progress = t / duration;
                float freq = Mathf.Lerp(180f, 520f, progress);
                float env = 1f - progress;
                return Mathf.Sin(2f * Mathf.PI * freq * t) * env;
            });

            // 3. Phase Bonus: Bell chime harmony
            phaseClip = CreateClip("Phase", 0.2f, sampleRate, (t, duration) =>
            {
                float progress = t / duration;
                float env = Mathf.Exp(-progress * 8f);
                float tone1 = Mathf.Sin(2f * Mathf.PI * 1046.5f * t); // C6
                float tone2 = Mathf.Sin(2f * Mathf.PI * 1318.5f * t); // E6
                return (tone1 + tone2) * 0.5f * env;
            });

            // 4. Death: Crunchy low-fi impact explosion
            deathClip = CreateClip("Death", 0.35f, sampleRate, (t, duration) =>
            {
                float progress = t / duration;
                float env = Mathf.Exp(-progress * 6f);
                float noise = (Random.value * 2f - 1f);
                float lowBoom = Mathf.Sin(2f * Mathf.PI * 75f * t);
                return (noise * 0.6f + lowBoom * 0.4f) * env;
            });

            // 5. Button Click: Celestial Crystal Prism Chime (Snappy, ethereal holographic ping)
            clickClip = CreateClip("ButtonClick", 0.11f, sampleRate, (t, duration) =>
            {
                float progress = t / duration;
                // Fast anti-pop attack + exponential crystalline ringdown
                float attack = Mathf.Clamp01(t / 0.003f);
                float env = attack * Mathf.Exp(-progress * 22f);

                // Transient: holographic crystal blip at start
                float transient = 0f;
                if (progress < 0.16f)
                {
                    float transProgress = progress / 0.16f;
                    float sweep = Mathf.Lerp(3400f, 1320f, transProgress);
                    transient = Mathf.Sin(2f * Mathf.PI * sweep * t) * (1f - transProgress) * 0.35f;
                }

                // Crystalline dual-realm harmonics: E6 (1318.5Hz fundamental) + B6 (1975.5Hz fifth) + E7 (2637Hz shimmer) + E5 (659.25Hz warm glow)
                float shimmer = 1f + 0.15f * Mathf.Sin(2f * Mathf.PI * 32f * t);
                float f1 = Mathf.Sin(2f * Mathf.PI * 1318.5f * t);
                float f2 = Mathf.Sin(2f * Mathf.PI * 1975.5f * t) * 0.45f * shimmer;
                float f3 = Mathf.Sin(2f * Mathf.PI * 2637.0f * t) * 0.20f;
                float fWarm = Mathf.Sin(2f * Mathf.PI * 659.25f * t) * 0.30f;

                float sample = (transient + (f1 * 0.55f + f2 + f3 + fWarm)) * env;
                return Mathf.Clamp(sample * 0.85f, -1f, 1f);
            });
        }

        private AudioClip CreateClip(string name, float duration, int sampleRate, System.Func<float, float, float> sampleFunc)
        {
            int sampleCount = Mathf.CeilToInt(duration * sampleRate);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                samples[i] = sampleFunc(t, duration);
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
