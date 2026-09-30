using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LightNShadows
{
    public class PostProcessEffects : MonoBehaviour
    {
        public static PostProcessEffects Instance { get; private set; }

        [SerializeField] private Volume volume;
        private ChromaticAberration chromaticAberration;
        private Bloom bloom;
        private Coroutine pulseCoroutine;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            if (volume == null) volume = GetComponent<Volume>();

            if (volume != null && volume.profile != null)
            {
                volume.profile.TryGet(out chromaticAberration);
                volume.profile.TryGet(out bloom);
            }
        }

        private void OnEnable()
        {
            DimensionManager.OnDimensionChanged += HandleDimensionChanged;
        }

        private void OnDisable()
        {
            DimensionManager.OnDimensionChanged -= HandleDimensionChanged;
        }

        private void HandleDimensionChanged(DimensionType dim)
        {
            // Flash a quick chromatic optical warp when ripping through realms
            PulseChromaticAberration(0.35f, 0.12f);
        }

        public void PulseChromaticAberration(float peakIntensity = 0.45f, float duration = 0.15f)
        {
            if (chromaticAberration == null) return;
            if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
            pulseCoroutine = StartCoroutine(ChromaticPulseRoutine(peakIntensity, duration));
        }

        private IEnumerator ChromaticPulseRoutine(float peak, float dur)
        {
            float elapsed = 0f;
            float baseIntensity = 0.06f;

            // Spike up
            while (elapsed < dur * 0.4f)
            {
                chromaticAberration.intensity.value = Mathf.Lerp(baseIntensity, peak, elapsed / (dur * 0.4f));
                elapsed += Time.deltaTime;
                yield return null;
            }

            elapsed = 0f;
            // Ease back down
            while (elapsed < dur * 0.6f)
            {
                chromaticAberration.intensity.value = Mathf.Lerp(peak, baseIntensity, elapsed / (dur * 0.6f));
                elapsed += Time.deltaTime;
                yield return null;
            }

            chromaticAberration.intensity.value = baseIntensity;
            pulseCoroutine = null;
        }
    }
}
