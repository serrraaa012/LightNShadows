using UnityEngine;

namespace LightNShadows
{
    public class BackgroundParticles : MonoBehaviour
    {
        [SerializeField] private int particleCount = 28;
        [SerializeField] private Sprite particleSprite;

        private GameObject[] particles;
        private SpriteRenderer[] renderers;
        private float[] speeds;

        private void Start()
        {
            if (particleSprite == null)
            {
                // Fallback to square
                particleSprite = Resources.Load<Sprite>("Sprites/SolidSquare");
            }

            particles = new GameObject[particleCount];
            renderers = new SpriteRenderer[particleCount];
            speeds = new float[particleCount];

            for (int i = 0; i < particleCount; i++)
            {
                GameObject p = new GameObject($"BgDust_{i}");
                p.transform.SetParent(transform);

                float startX = Random.Range(-10f, 11f);
                float startY = Random.Range(-3.2f, 4.5f);
                p.transform.position = new Vector3(startX, startY, 0f);

                float size = Random.Range(0.04f, 0.12f);
                p.transform.localScale = new Vector3(size, size, 1f);

                SpriteRenderer sr = p.AddComponent<SpriteRenderer>();
                sr.sprite = particleSprite;
                sr.sortingOrder = 0; // Behind everything, in front of background
                renderers[i] = sr;

                speeds[i] = Random.Range(1.5f, 4.5f);
                particles[i] = p;
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
            for (int i = 0; i < particleCount; i++)
            {
                if (particles[i] == null) continue;

                Vector3 pos = particles[i].transform.position;
                pos.x -= speeds[i] * Time.deltaTime;

                if (pos.x < -11.5f)
                {
                    pos.x = 11.5f;
                    pos.y = Random.Range(-3.2f, 4.5f);
                }

                particles[i].transform.position = pos;
            }
        }

        private void HandleDimensionChanged(DimensionType dim)
        {
            Color c = (dim == DimensionType.Light) 
                ? new Color(0.2f, 0.2f, 0.25f, 0.22f) 
                : new Color(0.9f, 0.95f, 1f, 0.25f);

            for (int i = 0; i < particleCount; i++)
            {
                if (renderers[i] != null) renderers[i].color = c;
            }
        }
    }
}
