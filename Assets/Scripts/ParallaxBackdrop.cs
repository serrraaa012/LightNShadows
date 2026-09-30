using UnityEngine;

namespace LightNShadows
{
    public class ParallaxBackdrop : MonoBehaviour
    {
        [SerializeField] private int monolithCount = 8;
        [SerializeField] private Sprite monolithSprite;
        [SerializeField] private float scrollSpeed = 1.2f;

        private GameObject[] monoliths;
        private SpriteRenderer[] renderers;

        private void Start()
        {
            if (monolithSprite == null)
            {
                monolithSprite = Resources.Load<Sprite>("Sprites/Monolith");
            }

            monoliths = new GameObject[monolithCount];
            renderers = new SpriteRenderer[monolithCount];

            for (int i = 0; i < monolithCount; i++)
            {
                GameObject m = new GameObject($"Monolith_{i}");
                m.transform.SetParent(transform);

                float x = -12f + i * 3.5f + Random.Range(-0.5f, 0.5f);
                float y = -2.8f;
                m.transform.position = new Vector3(x, y, 0f);

                float width = Random.Range(1.2f, 2.5f);
                float height = Random.Range(3.5f, 6.5f);
                m.transform.localScale = new Vector3(width, height, 1f);

                SpriteRenderer sr = m.AddComponent<SpriteRenderer>();
                sr.sprite = monolithSprite;
                sr.sortingOrder = -5; // Far in background
                renderers[i] = sr;

                monoliths[i] = m;
            }

            DimensionManager.OnDimensionChanged += HandleDimensionChanged;
            if (DimensionManager.Instance != null)
            {
                HandleDimensionChanged(DimensionManager.Instance.CurrentDimension);
            }
        }

        private void OnDestroy()
        {
            DimensionManager.OnDimensionChanged -= HandleDimensionChanged;
        }

        private void Update()
        {
            for (int i = 0; i < monolithCount; i++)
            {
                if (monoliths[i] == null) continue;

                Vector3 pos = monoliths[i].transform.position;
                pos.x -= scrollSpeed * Time.deltaTime;

                if (pos.x < -16f)
                {
                    pos.x = 16f;
                    float h = Random.Range(3.5f, 6.5f);
                    monoliths[i].transform.localScale = new Vector3(Random.Range(1.2f, 2.5f), h, 1f);
                }

                monoliths[i].transform.position = pos;
            }
        }

        private void HandleDimensionChanged(DimensionType dim)
        {
            Color c = (dim == DimensionType.Light)
                ? new Color(0.88f, 0.84f, 0.78f, 0.45f) // Warm desert horizon
                : new Color(0.12f, 0.14f, 0.28f, 0.55f); // Deep indigo nebula spires

            for (int i = 0; i < monolithCount; i++)
            {
                if (renderers[i] != null) renderers[i].color = c;
            }
        }
    }
}
