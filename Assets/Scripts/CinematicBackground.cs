using UnityEngine;

namespace LightNShadows
{
    public class CinematicBackground : MonoBehaviour
    {
        public static CinematicBackground Instance { get; private set; }

        [Header("Master Artwork")]
        [SerializeField] private SpriteRenderer bgRenderer;

        private Transform playerTransform;
        private Camera targetCam;
        private float lastAspect = -1f;
        private float lastOrthoSize = -1f;

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

            Transform oldB = transform.Find("Backdrop_B");
            if (oldB != null) Destroy(oldB.gameObject);

            Sprite bgSprite = LoadBackgroundSprite();

            if (bgSprite != null)
            {
                Transform transA = transform.Find("Backdrop_A");
                GameObject quadA = (transA != null) ? transA.gameObject : new GameObject("Backdrop_A");
                quadA.transform.SetParent(transform);
                quadA.transform.position = new Vector3(0f, 0.2f, 5f);

                bgRenderer = quadA.GetComponent<SpriteRenderer>();
                if (bgRenderer == null) bgRenderer = quadA.AddComponent<SpriteRenderer>();
                bgRenderer.sprite = bgSprite;
                bgRenderer.sortingOrder = -20; // Behind all gameplay elements

                FitToScreen();
            }
        }

        private void FitToScreen()
        {
            if (bgRenderer == null || bgRenderer.sprite == null) return;
            if (targetCam == null) targetCam = Camera.main;
            if (targetCam == null) return;

            // Full visible dimensions of the orthographic camera
            float camHeight = 2f * targetCam.orthographicSize;
            float camWidth = camHeight * targetCam.aspect;

            float spriteWidth = bgRenderer.sprite.bounds.size.x;
            float spriteHeight = bgRenderer.sprite.bounds.size.y;

            if (spriteWidth <= 0f || spriteHeight <= 0f) return;

            // Scale to cover the entire camera viewport with safety margin (ScaleAndCrop behavior)
            // Extra margin ensures ZERO border gaps even on ultra-wide screens or Free Aspect
            float scaleX = camWidth / spriteWidth;
            float scaleY = camHeight / spriteHeight;
            float coverScale = Mathf.Max(scaleX, scaleY) * 1.25f;

            bgRenderer.transform.localScale = new Vector3(coverScale, coverScale, 1f);

            lastAspect = targetCam.aspect;
            lastOrthoSize = targetCam.orthographicSize;
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
            targetCam = Camera.main;

            GameObject player = GameObject.FindWithTag("Player");
            if (player == null) player = GameObject.Find("Player");
            if (player != null) playerTransform = player.transform;

            FitToScreen();

            if (DimensionManager.Instance != null)
            {
                HandleDimensionChanged(DimensionManager.Instance.CurrentDimension);
            }
        }

        private void Update()
        {
            if (targetCam == null) targetCam = Camera.main;

            // If Game View aspect ratio changes (e.g. user resizes Free Aspect window), dynamically re-cover
            if (targetCam != null && (Mathf.Abs(targetCam.aspect - lastAspect) > 0.01f || Mathf.Abs(targetCam.orthographicSize - lastOrthoSize) > 0.01f))
            {
                FitToScreen();
            }

            if (bgRenderer == null) return;

            // Subtle vertical parallax response when player jumps
            if (playerTransform != null && GameManager.Instance != null && GameManager.Instance.IsPlaying)
            {
                float jumpOffset = (playerTransform.position.y + 2.4f) * 0.05f;
                float targetY = 0.2f + jumpOffset;
                bgRenderer.transform.position = new Vector3(0f, Mathf.Lerp(bgRenderer.transform.position.y, targetY, Time.deltaTime * 6f), 5f);
            }
        }

        private void HandleDimensionChanged(DimensionType dim)
        {
            if (bgRenderer == null) return;

            bool isLight = (dim == DimensionType.Light);
            // Light Realm: Crisp moonlit blue brilliance
            // Shadow Realm: Deep mysterious indigo-violet nocturnal mood
            Color tint = isLight ? new Color(1.08f, 1.08f, 1.12f, 1f) : new Color(0.68f, 0.62f, 0.95f, 1f);

            bgRenderer.color = tint;
        }
    }
}
