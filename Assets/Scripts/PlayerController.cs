using UnityEngine;
using System.Collections;

namespace LightNShadows
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color lightDimensionColor = new Color(0.08f, 0.08f, 0.12f);
        [SerializeField] private Color shadowDimensionColor = new Color(1.3f, 1.35f, 1.5f, 1f);
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private Sprite shockwaveSprite;

        [Header("Movement & Jump")]
        [SerializeField] private float jumpForce = 13.5f;
        [SerializeField] private float gravity = 35f;
        [SerializeField] private float groundY = -2.4f;
        [SerializeField] private bool allowJump = true;

        private float verticalVelocity = 0f;
        private bool isGrounded = true;
        private Vector3 baseScale;
        private Coroutine squashCoroutine;
        private bool isAlive = true;

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            baseScale = transform.localScale;
            if (baseScale == Vector3.zero) baseScale = new Vector3(0.85f, 0.85f, 1f);

            Vector3 pos = transform.position;
            pos.x = -5f;
            pos.y = groundY;
            pos.z = 0f;
            transform.position = pos;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterPlayer(this);
            }

            if (shockwaveSprite == null)
            {
                shockwaveSprite = Resources.Load<Sprite>("Sprites/ShockwaveRing");
#if UNITY_EDITOR
                if (shockwaveSprite == null) shockwaveSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ShockwaveRing.png");
#endif
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

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterPlayer(this);
            }

            if (DimensionManager.Instance != null)
            {
                ApplyVisuals(DimensionManager.Instance.CurrentDimension);
            }
        }

        private void Update()
        {
            if (!isAlive || GameManager.Instance == null || (!GameManager.Instance.IsPlaying || GameManager.Instance.IsGameOver))
            {
                return;
            }

            HandleJumpPhysics();
        }

        private void HandleJumpPhysics()
        {
            // Jump Trigger
            if (allowJump && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))
            {
                if (isGrounded)
                {
                    verticalVelocity = jumpForce;
                    isGrounded = false;
                    if (SoundManager.Instance != null) SoundManager.Instance.PlayJump();

                    // Jump Stretch Animation (Taller, thinner)
                    ApplySquashStretch(new Vector3(0.78f, 1.32f, 1f), 0.18f);
                }
            }

            // Airborne physics
            if (!isGrounded)
            {
                verticalVelocity -= gravity * Time.deltaTime;
                Vector3 pos = transform.position;
                pos.y += verticalVelocity * Time.deltaTime;

                // Check landing
                if (pos.y <= groundY)
                {
                    pos.y = groundY;
                    verticalVelocity = 0f;
                    isGrounded = true;

                    // Landing Squash Animation (Wider, flatter)
                    ApplySquashStretch(new Vector3(1.35f, 0.72f, 1f), 0.16f);
                }

                transform.position = pos;
            }
        }

        private void ApplySquashStretch(Vector3 targetScaleMultiplier, float duration)
        {
            if (squashCoroutine != null) StopCoroutine(squashCoroutine);
            squashCoroutine = StartCoroutine(SquashRoutine(targetScaleMultiplier, duration));
        }

        private IEnumerator SquashRoutine(Vector3 targetScaleMultiplier, float duration)
        {
            Vector3 targetScale = Vector3.Scale(baseScale, targetScaleMultiplier);
            float elapsed = 0f;
            float halfDur = duration * 0.45f;

            // Push to squash/stretch target
            while (elapsed < halfDur)
            {
                transform.localScale = Vector3.Lerp(baseScale, targetScale, elapsed / halfDur);
                elapsed += Time.deltaTime;
                yield return null;
            }

            elapsed = 0f;
            float returnDur = duration * 0.55f;
            // Spring back to normal scale
            while (elapsed < returnDur)
            {
                transform.localScale = Vector3.Lerp(targetScale, baseScale, elapsed / returnDur);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localScale = baseScale;
            squashCoroutine = null;
        }

        private void HandleDimensionChanged(DimensionType newDimension)
        {
            ApplyVisuals(newDimension);

            // Expanding Shockwave Pulse on Dimension Shift
            Color waveColor = (newDimension == DimensionType.Light) 
                ? new Color(0.95f, 0.6f, 0.1f) 
                : new Color(0.2f, 0.95f, 1.5f);

            if (shockwaveSprite != null)
            {
                ShockwavePulse.Create(transform.position, waveColor, shockwaveSprite);
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

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Obstacle obstacle = collision.GetComponent<Obstacle>();
            if (obstacle == null) return;

            DimensionType currentDim = DimensionManager.Instance != null 
                ? DimensionManager.Instance.CurrentDimension 
                : DimensionType.Light;

            // 1. Neutral Crimson Hazards: FATAL in BOTH realms! MUST BE JUMPED OVER!
            if (obstacle.Dimension == DimensionType.Neutral)
            {
                Die();
                return;
            }

            // 2. Realm Gates: Fatal if matching your realm, phase safely if opposite
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

                // Spawn floating popup
                if (FloatingTextManager.Instance != null)
                {
                    Color textCol = (currentDim == DimensionType.Light) 
                        ? new Color(0.1f, 0.9f, 1.4f) 
                        : new Color(1.3f, 0.85f, 0.2f);

                    FloatingTextManager.Instance.SpawnPopup(transform.position, "+1", textCol);
                }
            }
        }

        public void Die()
        {
            if (!isAlive) return;
            isAlive = false;

            if (SoundManager.Instance != null) SoundManager.Instance.PlayDeath();

            Color deathColor = (spriteRenderer != null) ? spriteRenderer.color : Color.white;
            Sprite deathSprite = (spriteRenderer != null) ? spriteRenderer.sprite : null;
            ShardExplosion.Create(transform.position, deathColor, deathSprite);

            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(0.4f, 0.45f);
            }

            if (PostProcessEffects.Instance != null)
            {
                PostProcessEffects.Instance.PulseChromaticAberration(0.75f, 0.35f);
            }

            gameObject.SetActive(false);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
        }

        public void ResetPlayer()
        {
            gameObject.SetActive(true);
            isAlive = true;

            if (squashCoroutine != null)
            {
                StopCoroutine(squashCoroutine);
                squashCoroutine = null;
            }

            if (baseScale == Vector3.zero)
            {
                baseScale = new Vector3(0.85f, 0.85f, 1f);
            }
            transform.localScale = baseScale;

            Vector3 pos = transform.position;
            pos.x = -5f;
            pos.y = groundY;
            pos.z = 0f;
            transform.position = pos;

            verticalVelocity = 0f;
            isGrounded = true;

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }

            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = true;
            }

            if (trailRenderer != null)
            {
                trailRenderer.enabled = true;
                trailRenderer.Clear();
            }

            Transform halo = transform.Find("OrbitHalo");
            if (halo != null)
            {
                halo.gameObject.SetActive(true);
            }

            DimensionType startDim = (DimensionManager.Instance != null)
                ? DimensionManager.Instance.CurrentDimension
                : DimensionType.Light;
            ApplyVisuals(startDim);
        }
    }
}
