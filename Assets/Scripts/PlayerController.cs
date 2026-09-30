using UnityEngine;
using System.Collections;

namespace LightNShadows
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color lightDimensionColor = new Color(0.08f, 0.08f, 0.12f); // Dark silhouette in Light
        [SerializeField] private Color shadowDimensionColor = new Color(1f, 1f, 1f);          // Pure luminous white in Shadow
        [SerializeField] private TrailRenderer trailRenderer;

        [Header("Movement & Jump")]
        [SerializeField] private float jumpForce = 13f;
        [SerializeField] private float gravity = 35f;
        [SerializeField] private float groundY = -2.4f;
        [SerializeField] private bool allowJump = true;

        [Header("Juice & Effects")]
        [SerializeField] private ParticleSystem phaseParticles;
        [SerializeField] private ParticleSystem deathParticles;
        [SerializeField] private float pulseScale = 1.3f;

        private float verticalVelocity = 0f;
        private bool isGrounded = true;
        private Vector3 originalScale;
        private Coroutine pulseCoroutine;

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            originalScale = transform.localScale;

            // Ensure player stays at ground level on startup
            Vector3 pos = transform.position;
            pos.y = groundY;
            transform.position = pos;
        }

        private void OnEnable()
        {
            DimensionManager.OnDimensionChanged += HandleDimensionChanged;
        }

        private void OnDisable()
        {
            DimensionManager.OnDimensionChanged -= HandleDimensionChanged;
        }

        private void Start()
        {
            if (DimensionManager.Instance != null)
            {
                ApplyVisuals(DimensionManager.Instance.CurrentDimension);
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                return;
            }

            HandleJump();
        }

        private void HandleJump()
        {
            // Jump trigger
            if (allowJump && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))
            {
                if (isGrounded)
                {
                    verticalVelocity = jumpForce;
                    isGrounded = false;
                    if (SoundManager.Instance != null) SoundManager.Instance.PlayJump();
                }
            }

            // Bulletproof jump physics (cannot fall through ground)
            if (!isGrounded)
            {
                verticalVelocity -= gravity * Time.deltaTime;
                Vector3 pos = transform.position;
                pos.y += verticalVelocity * Time.deltaTime;

                if (pos.y <= groundY)
                {
                    pos.y = groundY;
                    verticalVelocity = 0f;
                    isGrounded = true;
                }

                transform.position = pos;
            }
        }

        private void HandleDimensionChanged(DimensionType newDimension)
        {
            ApplyVisuals(newDimension);
            TriggerPulseEffect();

            if (phaseParticles != null)
            {
                phaseParticles.Play();
            }
        }

        private void ApplyVisuals(DimensionType dimension)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = (dimension == DimensionType.Light) ? lightDimensionColor : shadowDimensionColor;
            }

            if (trailRenderer != null)
            {
                trailRenderer.startColor = (dimension == DimensionType.Light) ? lightDimensionColor : shadowDimensionColor;
                Color endCol = trailRenderer.startColor;
                endCol.a = 0f;
                trailRenderer.endColor = endCol;
            }
        }

        private void TriggerPulseEffect()
        {
            if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
            pulseCoroutine = StartCoroutine(PulseRoutine());
        }

        private IEnumerator PulseRoutine()
        {
            Vector3 peakScale = originalScale * pulseScale;
            float elapsed = 0f;
            float duration = 0.12f;

            while (elapsed < duration)
            {
                transform.localScale = Vector3.Lerp(originalScale, peakScale, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < duration)
            {
                transform.localScale = Vector3.Lerp(peakScale, originalScale, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localScale = originalScale;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Obstacle obstacle = collision.GetComponent<Obstacle>();
            if (obstacle == null) return;

            DimensionType currentDim = DimensionManager.Instance != null 
                ? DimensionManager.Instance.CurrentDimension 
                : DimensionType.Light;

            // If active dimension matches obstacle, it is FATAL!
            if (obstacle.Dimension == currentDim)
            {
                Die();
            }
            else
            {
                // Phased safely through opposite dimension!
                obstacle.OnPlayerPhasedThrough();
                if (SoundManager.Instance != null) SoundManager.Instance.PlayPhase();
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.RegisterPhaseSuccess();
                }
            }
        }

        public void Die()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayDeath();

            Color deathColor = (spriteRenderer != null) ? spriteRenderer.color : Color.white;
            Sprite deathSprite = (spriteRenderer != null) ? spriteRenderer.sprite : null;
            ShardExplosion.Create(transform.position, deathColor, deathSprite);

            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(0.4f, 0.45f);
            }

            gameObject.SetActive(false);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
        }
    }
}
