using UnityEngine;

namespace LightNShadows
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        private AudioSource audioSource;
        private AudioClip swapClip;
        private AudioClip jumpClip;
        private AudioClip phaseClip;
        private AudioClip deathClip;

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

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            GenerateProceduralAudioClips();
        }

        public void PlaySwap()
        {
            if (audioSource != null && swapClip != null)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(swapClip, 0.65f);
            }
        }

        public void PlayJump()
        {
            if (audioSource != null && jumpClip != null)
            {
                audioSource.pitch = Random.Range(0.98f, 1.02f);
                audioSource.PlayOneShot(jumpClip, 0.5f);
            }
        }

        public void PlayPhase()
        {
            if (audioSource != null && phaseClip != null)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(phaseClip, 0.6f);
            }
        }

        public void PlayDeath()
        {
            if (audioSource != null && deathClip != null)
            {
                audioSource.pitch = 1.0f;
                audioSource.PlayOneShot(deathClip, 0.9f);
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
