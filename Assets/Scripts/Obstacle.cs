using UnityEngine;
using System.Collections.Generic;

namespace LightNShadows
{
    public enum ObstacleVisualType
    {
        CrystalSpike,
        CeilingSpire,
        LaserGate,
        FloatingDiamond,
        NeonHoop
    }

    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public class Obstacle : MonoBehaviour
    {
        // Global registry of all active onscreen obstacles for radar lookahead & zero-alloc queries
        public static readonly List<Obstacle> ActiveObstacles = new List<Obstacle>();

        [Header("Dimension State")]
        [SerializeField] private DimensionType dimension = DimensionType.Light;
        public DimensionType Dimension => dimension;

        [Header("Visual Subtype")]
        [SerializeField] private ObstacleVisualType visualType = ObstacleVisualType.LaserGate;
        public ObstacleVisualType VisualType => visualType;

        [Header("Movement")]
        [SerializeField] private float baseSpeed = 7f;
        [SerializeField] private float offscreenX = -14f;

        [Header("Electric Neon Colors")]
        [SerializeField] private Color neutralCrimsonColor = new Color(1.8f, 0.2f, 0.35f, 1f);
        [SerializeField] private Color lightSolarColor = new Color(1.5f, 0.9f, 0.2f, 1f); 
        [SerializeField] private Color shadowVoidColor = new Color(0.2f, 1.1f, 1.6f, 1f); 
        [Range(0.05f, 0.5f)]
        [SerializeField] private float ghostAlpha = 0.25f;

        private SpriteRenderer spriteRenderer;
        private float currentSpeed;
        private float animTimer = 0f;
        private Vector3 initialPosition;
        private Transform playerTrans;
        private bool hasScored = false;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            currentSpeed = baseSpeed;
            animTimer = Random.Range(0f, 10f);
        }

        private void OnEnable()
        {
            if (!ActiveObstacles.Contains(this))
            {
                ActiveObstacles.Add(this);
            }
            DimensionManager.OnDimensionChanged += HandleDimensionChanged;
        }

        private void OnDisable()
        {
            ActiveObstacles.Remove(this);
            DimensionManager.OnDimensionChanged -= HandleDimensionChanged;
        }

        private void OnDestroy()
        {
            ActiveObstacles.Remove(this);
        }

        private void Start()
        {
            initialPosition = transform.position;
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null) playerTrans = playerObj.transform;

            if (DimensionManager.Instance != null)
            {
                UpdateVisualState(DimensionManager.Instance.CurrentDimension);
            }
        }

        public void Setup(DimensionType type, float speed, ObstacleVisualType vType = ObstacleVisualType.LaserGate)
        {
            dimension = type;
            currentSpeed = speed;
            visualType = vType;

            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            BoxCollider2D box = GetComponent<BoxCollider2D>();

            // Setup high-tech neon obstacle visuals & colliders
            if (visualType == ObstacleVisualType.NeonHoop)
            {
                Sprite hoopSprite = SpriteHelper.GetNeonHoopSprite();
                if (hoopSprite != null && spriteRenderer != null)
                {
                    spriteRenderer.sprite = hoopSprite;
                }
                transform.localScale = new Vector3(1.1f, 1.1f, 1f);

                if (box != null)
                {
                    box.size = new Vector2(0.85f, 1.6f);
                    box.offset = Vector2.zero;
                }
            }
            else if (visualType == ObstacleVisualType.LaserGate)
            {
                Sprite laserSprite = SpriteHelper.GetLaserGateSprite();
                if (laserSprite != null && spriteRenderer != null)
                {
                    spriteRenderer.sprite = laserSprite;
                }
                transform.localScale = new Vector3(0.95f, 1.25f, 1f);

                if (box != null)
                {
                    box.size = new Vector2(0.75f, 3.4f);
                    box.offset = Vector2.zero;
                }
            }
            else if (visualType == ObstacleVisualType.FloatingDiamond)
            {
                transform.localScale = new Vector3(0.95f, 0.95f, 1f);
                if (box != null)
                {
                    box.size = new Vector2(1.1f, 1.1f);
                    box.offset = Vector2.zero;
                }
            }
            else if (visualType == ObstacleVisualType.CrystalSpike)
            {
                transform.localScale = new Vector3(0.9f, 0.9f, 1f);
                if (box != null)
                {
                    box.size = new Vector2(0.95f, 1.3f);
                    box.offset = new Vector2(0f, 0f);
                }
            }
            else if (visualType == ObstacleVisualType.CeilingSpire)
            {
                transform.localScale = new Vector3(0.9f, 0.9f, 1f);
                if (box != null)
                {
                    box.size = new Vector2(0.95f, 1.4f);
                    box.offset = new Vector2(0f, 0f);
                }
            }

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

            // 1. Neon Laser Gate Energy Pulse
            if (visualType == ObstacleVisualType.LaserGate)
            {
                float pulseX = 0.92f + Mathf.PingPong(animTimer * 6.5f, 0.16f);
                transform.localScale = new Vector3(pulseX, 1.25f, 1f);
            }
            // 2. Neon Phase Hoop Hover & Energetic Respiration
            else if (visualType == ObstacleVisualType.NeonHoop)
            {
                float hoverY = initialPosition.y + Mathf.Sin(animTimer * 3.8f) * 0.16f;
                float hoopPulse = 1.05f + Mathf.Sin(animTimer * 5.2f) * 0.08f;
                transform.position = new Vector3(transform.position.x, hoverY, 0f);
                transform.localScale = new Vector3(hoopPulse, hoopPulse, 1f);
            }
            // 3. Floating Neon Prism Diamond Drone Bobbing & Spinning
            else if (visualType == ObstacleVisualType.FloatingDiamond)
            {
                float floatY = initialPosition.y + Mathf.Sin(animTimer * 3.6f) * 0.24f;
                transform.position = new Vector3(transform.position.x, floatY, 0f);
                transform.rotation = Quaternion.Euler(0f, 0f, animTimer * 48f);
            }

            // Award score when player successfully clears / passes the obstacle
            if (!hasScored && GameManager.Instance != null && GameManager.Instance.IsPlaying)
            {
                if (playerTrans == null)
                {
                    GameObject p = GameObject.Find("Player");
                    if (p != null) playerTrans = p.transform;
                }

                if (playerTrans != null && playerTrans.gameObject.activeInHierarchy)
                {
                    // Trigger once obstacle moves safely behind the player
                    if (transform.position.x < playerTrans.position.x - 0.5f)
                    {
                        hasScored = true;
                        GameManager.Instance.AddScore(1);

                        if (FloatingTextManager.Instance != null)
                        {
                            Vector3 popupPos = new Vector3(playerTrans.position.x + 0.3f, playerTrans.position.y + 1.2f, 0f);
                            FloatingTextManager.Instance.SpawnPopup(popupPos, "+1", new Color(1f, 0.9f, 0.25f));
                        }
                    }
                }
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

            // In matching realm: solid vibrant neon barrier!
            // In opposite realm: glowing translucent holographic phase portal!
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
            hasScored = true; // Mark as scored so passing behind player doesn't double-award

            if (spriteRenderer != null && dimension != DimensionType.Neutral)
            {
                Color c = spriteRenderer.color;
                c.a = 0.65f;
                spriteRenderer.color = c;
            }
        }
    }
}
