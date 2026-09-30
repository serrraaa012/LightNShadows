using UnityEngine;

namespace LightNShadows
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public class PrismOrb : MonoBehaviour
    {
        [SerializeField] private float speed = 7f;
        [SerializeField] private float offscreenX = -14f;
        [SerializeField] private int scoreValue = 100;

        private float hoverTimer;
        private Vector3 startPos;
        private SpriteRenderer sr;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            hoverTimer = Random.Range(0f, 10f);
        }

        private void Start()
        {
            startPos = transform.position;
            ApplyRealmColor();
            DimensionManager.OnDimensionChanged += HandleDimensionChanged;
        }

        private void OnDestroy()
        {
            DimensionManager.OnDimensionChanged -= HandleDimensionChanged;
        }

        public void Setup(float moveSpeed)
        {
            speed = moveSpeed;
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

            transform.Translate(Vector3.left * speed * Time.deltaTime, Space.World);

            // Gentle floating bob & spin
            hoverTimer += Time.deltaTime * 4f;
            transform.position = new Vector3(transform.position.x, startPos.y + Mathf.Sin(hoverTimer) * 0.18f, 0f);
            transform.Rotate(0, 0, 90f * Time.deltaTime);

            if (transform.position.x < offscreenX)
            {
                Destroy(gameObject);
            }
        }

        private void HandleDimensionChanged(DimensionType dim)
        {
            ApplyRealmColor();
        }

        private void ApplyRealmColor()
        {
            if (sr == null) return;
            bool isLight = DimensionManager.Instance != null && DimensionManager.Instance.CurrentDimension == DimensionType.Light;
            sr.color = isLight ? new Color(1.3f, 0.85f, 0.2f, 1f) : new Color(0.2f, 0.95f, 1.5f, 1f);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player == null) return;

            // Collect!
            if (SoundManager.Instance != null) SoundManager.Instance.PlayPhase();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreValue);
            }

            if (FloatingTextManager.Instance != null)
            {
                Color col = (DimensionManager.Instance != null && DimensionManager.Instance.CurrentDimension == DimensionType.Light)
                    ? new Color(1.2f, 0.75f, 0.1f)
                    : new Color(0.2f, 0.95f, 1.4f);
                FloatingTextManager.Instance.SpawnPopup(transform.position, $"+{scoreValue}", col);
            }

            Destroy(gameObject);
        }
    }
}
