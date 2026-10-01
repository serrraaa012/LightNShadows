using UnityEngine;

namespace LightNShadows
{
    public class CinematicBackground : MonoBehaviour
    {
        public static CinematicBackground Instance { get; private set; }

        [Header("Master Artworks")]
        [SerializeField] private SpriteRenderer dayRenderer;
        [SerializeField] private SpriteRenderer nightRenderer;

        private float currentDayAlpha = 1f;
        private float currentNightAlpha = 0f;
        private float targetDayAlpha = 1f;
        private float targetNightAlpha = 0f;

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

            // Clean up any old single-quad objects
            Transform oldA = transform.Find("Backdrop_A");
            if (oldA != null) Destroy(oldA.gameObject);
            Transform oldB = transform.Find("Backdrop_B");
            if (oldB != null) Destroy(oldB.gameObject);

            Sprite daySprite = LoadSprite("Assets/Sprites/GameBackground_Day.jpg");
            Sprite nightSprite = LoadSprite("Assets/Sprites/GameBackground.jpg");

            // Setup Day Renderer (Morning Sunny Town)
            if (daySprite != null)
            {
                Transform transDay = transform.Find("Backdrop_Day");
                GameObject quadDay = (transDay != null) ? transDay.gameObject : new GameObject("Backdrop_Day");
                quadDay.transform.SetParent(transform);
                quadDay.transform.position = new Vector3(0f, 0.2f, 5f);

                dayRenderer = quadDay.GetComponent<SpriteRenderer>();
                if (dayRenderer == null) dayRenderer = quadDay.AddComponent<SpriteRenderer>();
                dayRenderer.sprite = daySprite;
                dayRenderer.sortingOrder = -20; // Base background layer
                dayRenderer.color = new Color(1f, 1f, 1f, 1f);
            }

            // Setup Night Renderer (Midnight Moonlit Town)
            if (nightSprite != null)
            {
                Transform transNight = transform.Find("Backdrop_Night");
                GameObject quadNight = (transNight != null) ? transNight.gameObject : new GameObject("Backdrop_Night");
                quadNight.transform.SetParent(transform);
                quadNight.transform.position = new Vector3(0f, 0.2f, 5f);

                nightRenderer = quadNight.GetComponent<SpriteRenderer>();
                if (nightRenderer == null) nightRenderer = quadNight.AddComponent<SpriteRenderer>();
                nightRenderer.sprite = nightSprite;
                nightRenderer.sortingOrder = -19; // Sits directly on top of Day layer for smooth crossfade
                nightRenderer.color = new Color(1f, 1f, 1f, 0f);
            }

            FitToScreen();
        }

        private void FitToScreen()
        {
            if (targetCam == null) targetCam = Camera.main;
            if (targetCam == null) return;

            float camHeight = 2f * targetCam.orthographicSize;
            float camWidth = camHeight * targetCam.aspect;

            // Fit Day Quad
            if (dayRenderer != null && dayRenderer.sprite != null)
            {
                float sw = dayRenderer.sprite.bounds.size.x;
                float sh = dayRenderer.sprite.bounds.size.y;
                if (sw > 0f && sh > 0f)
                {
                    float scale = Mathf.Max(camWidth / sw, camHeight / sh) * 1.25f;
                    dayRenderer.transform.localScale = new Vector3(scale, scale, 1f);
                }
            }

            // Fit Night Quad identically
            if (nightRenderer != null && nightRenderer.sprite != null)
            {
                float sw = nightRenderer.sprite.bounds.size.x;
                float sh = nightRenderer.sprite.bounds.size.y;
                if (sw > 0f && sh > 0f)
                {
                    float scale = Mathf.Max(camWidth / sw, camHeight / sh) * 1.25f;
                    nightRenderer.transform.localScale = new Vector3(scale, scale, 1f);
                }
            }

            lastAspect = targetCam.aspect;
            lastOrthoSize = targetCam.orthographicSize;
        }

        private Sprite LoadSprite(string assetPath)
        {
#if UNITY_EDITOR
            Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (s != null) return s;
#endif
            string fullPath = System.IO.Path.Combine(Application.dataPath, assetPath.Replace("Assets/", ""));
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
            else
            {
                // Default to Light dimension (Day)
                targetDayAlpha = 1f;
                targetNightAlpha = 0f;
                currentDayAlpha = 1f;
                currentNightAlpha = 0f;
            }
        }

        private void Update()
        {
            if (targetCam == null) targetCam = Camera.main;

            // Handle dynamic window resizing (Free Aspect, 16:9, ultrawide)
            if (targetCam != null && (Mathf.Abs(targetCam.aspect - lastAspect) > 0.01f || Mathf.Abs(targetCam.orthographicSize - lastOrthoSize) > 0.01f))
            {
                FitToScreen();
            }

            // Smooth cinematic crossfade between Day and Night (0.2s transition)
            currentDayAlpha = Mathf.MoveTowards(currentDayAlpha, targetDayAlpha, Time.deltaTime * 5f);
            currentNightAlpha = Mathf.MoveTowards(currentNightAlpha, targetNightAlpha, Time.deltaTime * 5f);

            if (dayRenderer != null)
            {
                dayRenderer.color = new Color(1f, 1f, 1f, currentDayAlpha);
            }

            if (nightRenderer != null)
            {
                nightRenderer.color = new Color(1f, 1f, 1f, currentNightAlpha);
            }

            // Subtle vertical depth parallax when player jumps
            if (playerTransform != null && GameManager.Instance != null && GameManager.Instance.IsPlaying)
            {
                float jumpOffset = (playerTransform.position.y + 2.4f) * 0.05f;
                float targetY = 0.2f + jumpOffset;
                float newY = Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * 6f);
                if (dayRenderer != null) dayRenderer.transform.position = new Vector3(0f, newY, 5f);
                if (nightRenderer != null) nightRenderer.transform.position = new Vector3(0f, newY, 5f);
            }
        }

        private void HandleDimensionChanged(DimensionType dim)
        {
            if (dim == DimensionType.Light)
            {
                // Light Realm: Morning Sunny Town
                targetDayAlpha = 1f;
                targetNightAlpha = 0f;
            }
            else
            {
                // Shadow Realm: Midnight Moonlit Town
                targetDayAlpha = 0f;
                targetNightAlpha = 1f;
            }
        }
    }
}
