using UnityEngine;
using System.Collections.Generic;

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
        // Global registry of all active onscreen obstacles
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

        [Header("Stylized Colors")]
        [SerializeField] private Color neutralCrimsonColor = new Color(1.8f, 0.2f, 0.35f, 1f);
        [SerializeField] private Color lightSolarColor = new Color(1.4f, 0.85f, 0.2f, 1f); 
        [SerializeField] private Color shadowVoidColor = new Color(0.2f, 0.95f, 1.5f, 1f); 
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

            // 1. Floating Diamond Bobbing & Spin
            if (visualType == ObstacleVisualType.FloatingDiamond)
            {
                transform.position = new Vector3(transform.position.x, initialPosition.y + Mathf.Sin(animTimer * 3.8f) * 0.18f, 0f);
            }
            // 2. Tall Laser Gate Horizontal Energy Pulse (preserving full 3.8f vertical height)
            else if (visualType == ObstacleVisualType.LaserGate)
            {
                float pulseX = 0.9f + Mathf.PingPong(animTimer * 3.5f, 0.22f);
                transform.localScale = new Vector3(pulseX, 3.8f, 1f);
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
            hasScored = true;

            if (spriteRenderer != null && dimension != DimensionType.Neutral)
            {
                Color c = spriteRenderer.color;
                c.a = 0.65f;
                spriteRenderer.color = c;
            }
        }
    }
}
