using UnityEngine;

namespace LightNShadows
{
    public class OrbitRing : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 160f;
        [SerializeField] private SpriteRenderer ringRenderer;
        [SerializeField] private Color lightDimensionColor = new Color(0.95f, 0.6f, 0.1f, 1f); // Solar Amber
        [SerializeField] private Color shadowDimensionColor = new Color(0.1f, 0.95f, 1.4f, 1f); // Neon Cyan Glow

        private void Awake()
        {
            if (ringRenderer == null) ringRenderer = GetComponent<SpriteRenderer>();
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
                HandleDimensionChanged(DimensionManager.Instance.CurrentDimension);
            }
        }

        private void Update()
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }

        private void HandleDimensionChanged(DimensionType dim)
        {
            if (ringRenderer != null)
            {
                ringRenderer.color = (dim == DimensionType.Light) ? lightDimensionColor : shadowDimensionColor;
            }
        }
    }
}
