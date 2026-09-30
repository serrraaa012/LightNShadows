using UnityEngine;
using System;

namespace LightNShadows
{
    public class DimensionManager : MonoBehaviour
    {
        public static DimensionManager Instance { get; private set; }

        [Header("State")]
        [SerializeField] private DimensionType currentDimension = DimensionType.Light;
        public DimensionType CurrentDimension => currentDimension;

        [Header("Camera & Background")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Color lightBackgroundColor = new Color(0.93f, 0.93f, 0.96f); // Soft bright white
        [SerializeField] private Color shadowBackgroundColor = new Color(0.06f, 0.06f, 0.09f); // Deep dark obsidian
        [SerializeField] private float colorTransitionSpeed = 12f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip switchSound;

        public static event Action<DimensionType> OnDimensionChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        private void Start()
        {
            ApplyDimensionImmediate(currentDimension);
        }

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            {
                return;
            }

            // One-button toggle: Spacebar, Left Click, or Screen Tap
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                ToggleDimension();
            }

            // Smoothly interpolate camera background color
            if (targetCamera != null)
            {
                Color targetColor = (currentDimension == DimensionType.Light) ? lightBackgroundColor : shadowBackgroundColor;
                targetCamera.backgroundColor = Color.Lerp(targetCamera.backgroundColor, targetColor, Time.deltaTime * colorTransitionSpeed);
            }
        }

        public void ToggleDimension()
        {
            DimensionType newDimension = (currentDimension == DimensionType.Light) 
                ? DimensionType.Shadow 
                : DimensionType.Light;

            SetDimension(newDimension);
        }

        public void SetDimension(DimensionType newDimension)
        {
            currentDimension = newDimension;

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySwap();
            }
            else if (audioSource != null && switchSound != null)
            {
                audioSource.PlayOneShot(switchSound);
            }

            OnDimensionChanged?.Invoke(currentDimension);
        }

        private void ApplyDimensionImmediate(DimensionType dimension)
        {
            currentDimension = dimension;
            if (targetCamera != null)
            {
                targetCamera.backgroundColor = (dimension == DimensionType.Light) ? lightBackgroundColor : shadowBackgroundColor;
            }
            OnDimensionChanged?.Invoke(dimension);
        }
    }
}
