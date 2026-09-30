using UnityEngine;

namespace LightNShadows
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public class Obstacle : MonoBehaviour
    {
        [Header("Obstacle Dimension")]
        [SerializeField] private DimensionType dimension = DimensionType.Light;
        public DimensionType Dimension => dimension;

        [Header("Movement")]
        [SerializeField] private float baseSpeed = 7f;
        [SerializeField] private float offscreenX = -14f;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color lightObstacleColor = new Color(0.95f, 0.95f, 0.98f, 1f); // Bright white
        [SerializeField] private Color shadowObstacleColor = new Color(0.08f, 0.08f, 0.12f, 1f); // Dark silhouette
        [Range(0.1f, 1f)]
        [SerializeField] private float ghostAlpha = 0.25f;

        [Header("Particles / Feedback")]
        [SerializeField] private ParticleSystem phaseSuccessEffect;

        private float currentSpeed;

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            currentSpeed = baseSpeed;
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
                UpdateVisualState(DimensionManager.Instance.CurrentDimension);
            }
            else
            {
                UpdateVisualState(DimensionType.Light);
            }
        }

        public void Setup(DimensionType type, float speed)
        {
            dimension = type;
            currentSpeed = speed;
            if (DimensionManager.Instance != null)
            {
                UpdateVisualState(DimensionManager.Instance.CurrentDimension);
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                return;
            }

            // Move left towards the player
            transform.Translate(Vector3.left * currentSpeed * Time.deltaTime);

            // Destroy when far off-screen
            if (transform.position.x < offscreenX)
            {
                Destroy(gameObject);
            }
        }

        private void HandleDimensionChanged(DimensionType activeDimension)
        {
            UpdateVisualState(activeDimension);
        }

        private void UpdateVisualState(DimensionType activeDimension)
        {
            if (spriteRenderer == null) return;

            Color baseColor = (dimension == DimensionType.Light) ? lightObstacleColor : shadowObstacleColor;

            // If active dimension is the same as the obstacle, it is SOLID and FATAL!
            // If active dimension is opposite, it is GHOSTED (phasing is safe).
            if (activeDimension == dimension)
            {
                baseColor.a = 1.0f;
            }
            else
            {
                baseColor.a = ghostAlpha;
            }

            spriteRenderer.color = baseColor;
        }

        public void OnPlayerPhasedThrough()
        {
            if (phaseSuccessEffect != null)
            {
                phaseSuccessEffect.Play();
            }
        }
    }
}
