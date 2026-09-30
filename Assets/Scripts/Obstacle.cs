using UnityEngine;

namespace LightNShadows
{
    public enum ObstacleVisualType
    {
        CrystalSpike,
        LaserGate,
        FloatingDiamond
    }

    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public class Obstacle : MonoBehaviour
    {
        [Header("Dimension State")]
        [SerializeField] private DimensionType dimension = DimensionType.Light;
        public DimensionType Dimension => dimension;

        [Header("Visual Subtype")]
        [SerializeField] private ObstacleVisualType visualType = ObstacleVisualType.CrystalSpike;

        [Header("Movement")]
        [SerializeField] private float baseSpeed = 7f;
        [SerializeField] private float offscreenX = -14f;

        [Header("Stylized Colors")]
        // Neutral Hazard: Electric Crimson Neon Red (Fatal in BOTH realms!)
        [SerializeField] private Color neutralCrimsonColor = new Color(1.6f, 0.15f, 0.28f, 1f);
        // Solar / Light: Radiant Amber & Gold
        [SerializeField] private Color lightSolarColor = new Color(1.4f, 0.85f, 0.2f, 1f); 
        // Void / Shadow: Electric Cyan Glow
        [SerializeField] private Color shadowVoidColor = new Color(0.2f, 0.95f, 1.5f, 1f); 
        [Range(0.05f, 0.5f)]
        [SerializeField] private float ghostAlpha = 0.22f;

        private SpriteRenderer spriteRenderer;
        private float currentSpeed;
        private float hoverTimer = 0f;
        private Vector3 initialPosition;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            currentSpeed = baseSpeed;
            hoverTimer = Random.Range(0f, 10f);
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
            initialPosition = transform.position;
            if (DimensionManager.Instance != null)
            {
                UpdateVisualState(DimensionManager.Instance.CurrentDimension);
            }
        }

        public void Setup(DimensionType type, float speed, ObstacleVisualType vType = ObstacleVisualType.CrystalSpike)
        {
            dimension = type;
            currentSpeed = speed;
            visualType = vType;

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

            transform.Translate(Vector3.left * currentSpeed * Time.deltaTime, Space.World);

            if (visualType == ObstacleVisualType.FloatingDiamond)
            {
                hoverTimer += Time.deltaTime * 3.5f;
                transform.position = new Vector3(transform.position.x, initialPosition.y + Mathf.Sin(hoverTimer) * 0.2f, 0f);
            }

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

            // Neutral hazards are ALWAYS solid crimson red
            if (dimension == DimensionType.Neutral)
            {
                spriteRenderer.color = neutralCrimsonColor;
                return;
            }

            Color baseColor = (dimension == DimensionType.Light) ? lightSolarColor : shadowVoidColor;

            // Solid and deadly if matching active dimension
            if (activeDimension == dimension)
            {
                baseColor.a = 1.0f;
            }
            else
            {
                // Holographic ghost: safe to phase through
                baseColor.a = ghostAlpha;
            }

            spriteRenderer.color = baseColor;
        }

        public void OnPlayerPhasedThrough()
        {
            if (spriteRenderer != null && dimension != DimensionType.Neutral)
            {
                Color c = spriteRenderer.color;
                c.a = 0.6f;
                spriteRenderer.color = c;
            }
        }
    }
}
