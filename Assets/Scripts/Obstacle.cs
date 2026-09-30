using UnityEngine;

namespace LightNShadows
{
    public enum ObstacleVisualType
    {
        CrystalSpike,
        CeilingSpire,
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
        [SerializeField] private Color neutralCrimsonColor = new Color(1.6f, 0.15f, 0.28f, 1f);
        [SerializeField] private Color lightSolarColor = new Color(1.4f, 0.85f, 0.2f, 1f); 
        [SerializeField] private Color shadowVoidColor = new Color(0.2f, 0.95f, 1.5f, 1f); 
        [Range(0.05f, 0.5f)]
        [SerializeField] private float ghostAlpha = 0.2f;

        private SpriteRenderer spriteRenderer;
        private float currentSpeed;
        private float animTimer = 0f;
        private Vector3 initialPosition;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            currentSpeed = baseSpeed;
            animTimer = Random.Range(0f, 10f);
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
            animTimer += Time.deltaTime;

            // 1. Floating Diamond Bobbing & Rotation
            if (visualType == ObstacleVisualType.FloatingDiamond)
            {
                transform.position = new Vector3(transform.position.x, initialPosition.y + Mathf.Sin(animTimer * 3.8f) * 0.22f, 0f);
            }
            // 2. Laser Gate Energy Pulse
            else if (visualType == ObstacleVisualType.LaserGate)
            {
                float pulse = 0.9f + Mathf.PingPong(animTimer * 3.5f, 0.25f);
                transform.localScale = new Vector3(pulse * 0.9f, transform.localScale.y, 1f);
            }

            // Recycle off-screen
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

            if (dimension == DimensionType.Neutral)
            {
                spriteRenderer.color = neutralCrimsonColor;
                return;
            }

            Color baseColor = (dimension == DimensionType.Light) ? lightSolarColor : shadowVoidColor;

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
            if (spriteRenderer != null && dimension != DimensionType.Neutral)
            {
                Color c = spriteRenderer.color;
                c.a = 0.65f;
                spriteRenderer.color = c;
            }
        }
    }
}
