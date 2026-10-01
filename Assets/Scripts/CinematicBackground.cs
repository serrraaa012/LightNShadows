using UnityEngine;

namespace LightNShadows
{
    public class CinematicBackground : MonoBehaviour
    {
        public static CinematicBackground Instance { get; private set; }

        [Header("Master Artwork")]
        [SerializeField] private SpriteRenderer bgRendererA;
        [SerializeField] private SpriteRenderer bgRendererB;
        [SerializeField] private float parallaxSpeed = 0.8f;

        private float bgWidth = 24f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInit()
        {
            if (FindFirstObjectByType<CinematicBackground>() == null)
            {
                GameObject bgGo = new GameObject("CinematicBackground");
                bgGo.AddComponent<CinematicBackground>();
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            SetupMasterArtwork();
        }

        private void SetupMasterArtwork()
        {
            GameObject oldParallax = GameObject.Find("ParallaxBackdrop");
            if (oldParallax != null) oldParallax.SetActive(false);

            Sprite bgSprite = LoadBackgroundSprite();

            if (bgSprite != null)
            {
                // Create two scrolling quads for infinite seamless parallax
                if (bgRendererA == null)
                {
                    GameObject quadA = new GameObject("Backdrop_A");
                    quadA.transform.SetParent(transform);
                    quadA.transform.position = new Vector3(0f, 0f, 5f);
                    quadA.transform.localScale = new Vector3(1.45f, 1.45f, 1f);

                    bgRendererA = quadA.AddComponent<SpriteRenderer>();
                    bgRendererA.sprite = bgSprite;
                    bgRendererA.sortingOrder = -20;
                }

                if (bgRendererB == null)
                {
                    GameObject quadB = new GameObject("Backdrop_B");
                    quadB.transform.SetParent(transform);
                    bgWidth = bgRendererA.bounds.size.x;
                    quadB.transform.position = new Vector3(bgWidth - 0.05f, 0f, 5f);
                    quadB.transform.localScale = new Vector3(1.45f, 1.45f, 1f);

                    bgRendererB = quadB.AddComponent<SpriteRenderer>();
                    bgRendererB.sprite = bgSprite;
                    bgRendererB.sortingOrder = -20;
                }
            }
        }

        private Sprite LoadBackgroundSprite()
        {
#if UNITY_EDITOR
            Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/GameBackground.jpg");
            if (s != null) return s;
#endif
            string fullPath = System.IO.Path.Combine(Application.dataPath, "Sprites/GameBackground.jpg");
            if (System.IO.File.Exists(fullPath))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(fullPath);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
                if (tex.LoadImage(bytes))
                {
                    tex.wrapMode = TextureWrapMode.Clamp;
                    return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                }
            }
            return null;
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
            if (GameManager.Instance != null && (!GameManager.Instance.IsPlaying || GameManager.Instance.IsGameOver))
            {
                return;
            }

            // Infinite smooth parallax scrolling
            if (bgRendererA != null && bgRendererB != null)
            {
                bgRendererA.transform.position += Vector3.left * parallaxSpeed * Time.deltaTime;
                bgRendererB.transform.position += Vector3.left * parallaxSpeed * Time.deltaTime;

                if (bgRendererA.transform.position.x < -bgWidth)
                {
                    bgRendererA.transform.position = new Vector3(bgRendererB.transform.position.x + bgWidth - 0.05f, bgRendererA.transform.position.y, bgRendererA.transform.position.z);
                }

                if (bgRendererB.transform.position.x < -bgWidth)
                {
                    bgRendererB.transform.position = new Vector3(bgRendererA.transform.position.x + bgWidth - 0.05f, bgRendererB.transform.position.y, bgRendererB.transform.position.z);
                }
            }
        }

        private void HandleDimensionChanged(DimensionType dim)
        {
            bool isLight = (dim == DimensionType.Light);
            Color tint = isLight ? new Color(1.1f, 1.05f, 0.95f, 1f) : new Color(0.65f, 0.60f, 1.05f, 1f);

            if (bgRendererA != null) bgRendererA.color = tint;
            if (bgRendererB != null) bgRendererB.color = tint;
        }
    }
}
