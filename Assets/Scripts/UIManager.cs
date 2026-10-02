using UnityEngine;
using System.Collections.Generic;

namespace LightNShadows
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private Texture2D bgTex;
        private Texture2D titleLogoTex;
        private Texture2D panelTex;
        private Texture2D buttonTex;
        private Texture2D buttonHoverTex;
        private Texture2D iconJumpTex;
        private Texture2D iconPhaseTex;

        // Intro Logo Flash
        private float splashTimer = 0f;
        private const float SplashDuration = 4.2f;
        private bool splashAudioTriggered = false;

        [Header("Stylized Fonts")]
        [SerializeField] private Font displayFont;
        [SerializeField] private Font bodyFont;

        private GUIStyle titleStyle;
        private GUIStyle headerStyle;
        private GUIStyle cardTitleStyle;
        private GUIStyle cardBodyStyle;
        private GUIStyle buttonStyle;
        private GUIStyle hudStyle;
        private GUIStyle inputFieldStyle;

        // Username Dialog State
        private bool showNameModal = false;
        private string inputPlayerName = "";
        private bool focusNameFieldNext = false;
        private bool startGameOnConfirm = false;
        private static bool hasPromptedNameOnOpen = false;

        // ==========================================
        // CELESTIAL PRISM & STARBURST CLICK EFFECTS
        // ==========================================
        private enum CosmicParticleType
        {
            StarGlint,      // 4-point radiant rotating star
            DiamondShard    // Faceted diamond rhombus crystal
        }

        private class CosmicSpark
        {
            public Vector2 position;
            public Vector2 velocity;
            public float rotation;
            public float angularVelocity;
            public float size;
            public float life;
            public float maxLife;
            public Color color;
            public CosmicParticleType particleType;
        }

        private class NovaRing
        {
            public Vector2 center;
            public float currentRadius;
            public float maxRadius;
            public float life;
            public float maxLife;
            public Color color;
        }

        private class StarburstFlare
        {
            public Vector2 center;
            public float currentSize;
            public float targetSize;
            public float life;
            public float maxLife;
            public Color color;
        }

        private readonly List<CosmicSpark> activeSparks = new List<CosmicSpark>();
        private readonly List<NovaRing> activeNovaRings = new List<NovaRing>();
        private readonly List<StarburstFlare> activeFlares = new List<StarburstFlare>();

        private Texture2D starGlintTex;
        private Texture2D diamondShardTex;
        private Texture2D novaRingTex;
        private Texture2D starburstFlareTex;

        private bool stylesInitialized = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            LoadTextures();
        }

        private void LoadTextures()
        {
            bgTex = LoadTexture("Assets/Sprites/GameBackground.jpg");
            titleLogoTex = LoadTexture("Assets/Sprites/TitleLogo.jpg");
            panelTex = LoadTexture("Assets/Sprites/UIPanel.png");
            buttonTex = LoadTexture("Assets/Sprites/UIButton.png");
            buttonHoverTex = LoadTexture("Assets/Sprites/UIButtonHover.png");
            iconJumpTex = LoadTexture("Assets/Sprites/IconJump.png");
            iconPhaseTex = LoadTexture("Assets/Sprites/IconPhase.png");
        }

        private Texture2D LoadTexture(string assetPath)
        {
#if UNITY_EDITOR
            Texture2D tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (tex != null) return tex;
#endif
            string fullPath = System.IO.Path.Combine(Application.dataPath, assetPath.Replace("Assets/", ""));
            if (System.IO.File.Exists(fullPath))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(fullPath);
                Texture2D loaded = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (loaded.LoadImage(bytes))
                {
                    loaded.wrapMode = TextureWrapMode.Clamp;
                    return loaded;
                }
            }
            return null;
        }

        private void InitStyles()
        {
            if (stylesInitialized && titleStyle != null) return;

            if (displayFont == null)
            {
                displayFont = Resources.Load<Font>("Fonts/Righteous-Regular");
#if UNITY_EDITOR
                if (displayFont == null) displayFont = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Righteous-Regular.ttf");
                if (displayFont == null) displayFont = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Fonts/Righteous-Regular.ttf");
#endif
                if (displayFont == null) displayFont = Font.CreateDynamicFontFromOSFont("Impact", 24);
                if (displayFont == null) displayFont = Font.CreateDynamicFontFromOSFont("Bahnschrift", 24);
            }

            if (bodyFont == null)
            {
                bodyFont = Resources.Load<Font>("Fonts/Audiowide-Regular");
#if UNITY_EDITOR
                if (bodyFont == null) bodyFont = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Audiowide-Regular.ttf");
                if (bodyFont == null) bodyFont = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Fonts/Audiowide-Regular.ttf");
#endif
                if (bodyFont == null) bodyFont = displayFont;
            }

            int titleSize = Mathf.Clamp((int)(Screen.width * 0.052f), 38, 64);

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                font = displayFont,
                fontSize = titleSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
            titleStyle.normal.textColor = Color.white;

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                font = displayFont,
                fontSize = Mathf.Clamp((int)(Screen.width * 0.038f), 28, 44),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false
            };
            headerStyle.normal.textColor = Color.white;

            cardTitleStyle = new GUIStyle(GUI.skin.label)
            {
                font = displayFont,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };

            cardBodyStyle = new GUIStyle(GUI.skin.label)
            {
                font = bodyFont ?? displayFont,
                fontSize = 14,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.UpperLeft,
                wordWrap = true
            };
            cardBodyStyle.normal.textColor = new Color(0.85f, 0.9f, 0.98f);

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                font = displayFont,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            buttonStyle.normal.textColor = Color.white;
            buttonStyle.hover.textColor = new Color(0.2f, 1f, 1.4f);
            buttonStyle.active.textColor = new Color(1.3f, 0.9f, 0.3f);

            if (buttonTex != null)
            {
                buttonStyle.normal.background = buttonTex;
                buttonStyle.active.background = buttonHoverTex ?? buttonTex;
                buttonStyle.hover.background = buttonHoverTex ?? buttonTex;
            }

            hudStyle = new GUIStyle(GUI.skin.label)
            {
                font = displayFont,
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft
            };

            inputFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                font = displayFont,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            inputFieldStyle.normal.textColor = Color.white;
            inputFieldStyle.focused.textColor = new Color(1.3f, 0.9f, 0.3f);

            stylesInitialized = true;
        }

        private void Update()
        {
            if (GameManager.Instance == null) return;
            if (showNameModal) return;

            // Toggle pause with Escape or P key
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                if (GameManager.Instance.IsPlaying || GameManager.Instance.IsPaused)
                {
                    GameManager.Instance.TogglePause();
                }
            }
        }

        private void OnGUI()
        {
            InitStyles();

            GameManager.GameState state = (GameManager.Instance != null) ? GameManager.Instance.State : GameManager.GameState.Title;

            switch (state)
            {
                case GameManager.GameState.IntroSplash:
                    DrawIntroSplash();
                    break;
                case GameManager.GameState.Title:
                    DrawStartMenu();
                    break;
                case GameManager.GameState.Instructions:
                    DrawInstructionsMenu();
                    break;
                case GameManager.GameState.Playing:
                    DrawHUD();
                    break;
                case GameManager.GameState.Paused:
                    DrawPauseMenu();
                    break;
                case GameManager.GameState.GameOver:
                    DrawGameOverMenu();
                    break;
            }

            // Draw username modal dialog on top if active
            if (showNameModal)
            {
                DrawNameModal();
            }

            // Draw celestial starburst glints, prism crystals, and nova shockwaves
            UpdateAndDrawCosmicEffects();
        }

        // ==========================================
        // 0. FULL-SCREEN INTRO LOGO FLASH
        // ==========================================
        private void DrawIntroSplash()
        {
            splashTimer += Time.deltaTime;

            if (!splashAudioTriggered)
            {
                splashAudioTriggered = true;
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayPhase();
                }
            }

            // Allow skipping via tap/click after initial 0.5s
            if (splashTimer > 0.5f && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
            {
                if (GameManager.Instance != null) GameManager.Instance.CompleteIntroSplash();
                return;
            }

            if (splashTimer >= SplashDuration)
            {
                if (GameManager.Instance != null) GameManager.Instance.CompleteIntroSplash();
                return;
            }

            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);

            // Clean deep black base
            GUI.color = Color.black;
            GUI.DrawTexture(screenRect, Texture2D.whiteTexture);

            // Alpha envelope (quick fade in 0.35s, stays full for majority, smooth fade out at end)
            float logoAlpha = 1f;
            if (splashTimer < 0.35f)
            {
                logoAlpha = Mathf.SmoothStep(0f, 1f, splashTimer / 0.35f);
            }
            else if (splashTimer > SplashDuration - 0.6f)
            {
                logoAlpha = Mathf.SmoothStep(1f, 0f, (splashTimer - (SplashDuration - 0.6f)) / 0.6f);
            }

            // Full-screen Title Logo image (fills the entire screen edge-to-edge, NOT above any other image)
            if (titleLogoTex != null)
            {
                GUI.color = new Color(1f, 1f, 1f, logoAlpha);
                GUI.DrawTexture(screenRect, titleLogoTex, ScaleMode.ScaleAndCrop);
            }

            // Initial Supernova Flash Ignition (Brilliant white flare bursting 0.0s - 0.35s)
            if (splashTimer < 0.35f)
            {
                float flashAlpha = Mathf.Pow(1f - (splashTimer / 0.35f), 2f) * 0.9f;
                GUI.color = new Color(1f, 1f, 1f, flashAlpha);
                GUI.DrawTexture(screenRect, Texture2D.whiteTexture);
            }

            GUI.color = Color.white;
        }

        // ==========================================
        // 1. IMMERSIVE FULL-SCREEN START MENU
        // ==========================================
        private void DrawStartMenu()
        {
            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);

            // 1. Fullscreen Dual-Realm Artwork with Stylish Light(N)Shadows Title
            Texture2D menuBg = titleLogoTex ?? bgTex;
            if (menuBg != null)
            {
                GUI.color = Color.white;
                GUI.DrawTexture(screenRect, menuBg, ScaleMode.ScaleAndCrop);
            }
            else
            {
                DrawFullScreenBackdrop(new Color(0.02f, 0.03f, 0.06f, 0.40f));
            }

            // Only ask for username when opening the game
            if (!hasPromptedNameOnOpen && !showNameModal)
            {
                hasPromptedNameOnOpen = true;
                startGameOnConfirm = false;
                OpenNameModal();
            }

            float midX = Screen.width * 0.5f;

            // 2. High Score Ribbon Badge (Sleek rounded pill badge - NO harsh wireframe boxes)
            int best = (GameManager.Instance != null) ? GameManager.Instance.HighScore : 0;
            float badgeW = 280f;
            float badgeH = 34f;
            float badgeY = Screen.height * 0.53f;
            Rect badgeRect = new Rect(midX - badgeW * 0.5f, badgeY, badgeW, badgeH);

            if (buttonTex != null)
            {
                GUI.color = new Color(0.40f, 0.28f, 0.05f, 0.92f); // Deep solar gold rounded pill
                GUI.DrawTexture(badgeRect, buttonTex);
            }
            else
            {
                GUI.color = new Color(0.12f, 0.09f, 0.04f, 0.90f);
                GUI.DrawTexture(badgeRect, Texture2D.whiteTexture);
            }
            GUI.color = Color.white;

            GUIStyle badgeStyle = new GUIStyle(cardBodyStyle)
            {
                font = displayFont,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            badgeStyle.normal.textColor = new Color(1.4f, 0.95f, 0.35f);
            GUI.Label(badgeRect, $"★  RECORD: {best:N0} PTS  ★", badgeStyle);

            // 3. Runner Profile Tag (Clickable to Edit Name - Sleek rounded pill)
            float tagY = badgeY + 42f;
            float tagH = 38f;
            Rect tagRect = new Rect(midX - badgeW * 0.5f, tagY, badgeW, tagH);

            bool isTagHover = tagRect.Contains(Event.current.mousePosition) && !showNameModal;
            if (buttonTex != null)
            {
                GUI.color = isTagHover ? new Color(0.15f, 0.40f, 0.65f, 0.98f) : new Color(0.08f, 0.20f, 0.35f, 0.92f);
                GUI.DrawTexture(tagRect, buttonTex);
            }
            else
            {
                GUI.color = isTagHover ? new Color(0.12f, 0.22f, 0.35f, 0.95f) : new Color(0.06f, 0.12f, 0.20f, 0.90f);
                GUI.DrawTexture(tagRect, Texture2D.whiteTexture);
            }
            GUI.color = Color.white;

            string runnerTag = (GameManager.Instance != null && GameManager.Instance.HasPlayerName)
                ? GameManager.Instance.PlayerName
                : "SET NAME";

            GUIStyle tagTextStyle = new GUIStyle(cardBodyStyle)
            {
                font = displayFont,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            tagTextStyle.normal.textColor = isTagHover ? new Color(0.40f, 1.15f, 1.65f) : new Color(0.90f, 0.95f, 1f);
            GUI.Label(tagRect, $"👤 RUNNER: {runnerTag}  ✎", tagTextStyle);

            if (GUI.Button(tagRect, GUIContent.none, GUIStyle.none) && !showNameModal)
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                SpawnCosmicClickEffect(Event.current.mousePosition != Vector2.zero ? Event.current.mousePosition : tagRect.center, new Color(0.2f, 0.9f, 1.4f));
                startGameOnConfirm = false;
                OpenNameModal();
            }

            // 4. Action Buttons (Clean rounded pill buttons - NO square boxes)
            float btnW = Mathf.Clamp(Screen.width * 0.26f, 260f, 320f);
            float btnH = 52f;
            float btnX = midX - btnW * 0.5f;
            float playBtnY = tagY + 48f;
            float howToBtnY = playBtnY + 62f;

            if (DrawStyledButton(new Rect(btnX, playBtnY, btnW, btnH), "PLAY", ButtonTheme.PrimaryCyan, interactive: !showNameModal))
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.StartGame();
                }
            }

            if (DrawStyledButton(new Rect(btnX, howToBtnY, btnW, btnH), "HOW TO PLAY", ButtonTheme.SecondaryIndigo, interactive: !showNameModal))
            {
                GameManager.Instance.OpenInstructions();
            }
        }

        private void OpenNameModal()
        {
            showNameModal = true;
            inputPlayerName = (GameManager.Instance != null && GameManager.Instance.HasPlayerName) 
                ? GameManager.Instance.PlayerName 
                : "";
            focusNameFieldNext = true;
        }

        private void ConfirmPlayerName()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();

            string cleanName = inputPlayerName?.Trim();
            if (string.IsNullOrWhiteSpace(cleanName))
            {
                cleanName = "RUNNER";
            }
            if (cleanName.Length > 14)
            {
                cleanName = cleanName.Substring(0, 14);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetPlayerName(cleanName);
                if (startGameOnConfirm)
                {
                    GameManager.Instance.StartGame();
                }
            }

            showNameModal = false;
        }

        // ==========================================
        // 1.5. USERNAME PROMPT MODAL
        // ==========================================
        private void DrawNameModal()
        {
            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);

            // Dark semi-transparent scrim backdrop
            GUI.color = new Color(0.02f, 0.03f, 0.06f, 0.85f);
            GUI.DrawTexture(screenRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            float midX = Screen.width * 0.5f;
            float midY = Screen.height * 0.5f;

            float cardW = Mathf.Min(480f, Screen.width * 0.92f);
            float cardH = 310f;
            Rect modalRect = new Rect(midX - cardW * 0.5f, midY - cardH * 0.5f, cardW, cardH);

            DrawGlassPanel(modalRect);

            // 1. Header: "ENTER YOUR NAME" (Neon Cyan glow with drop shadow)
            GUIStyle modalHeader = new GUIStyle(headerStyle) { fontSize = 30 };
            modalHeader.normal.textColor = new Color(0.02f, 0.04f, 0.08f, 0.9f);
            GUI.Label(new Rect(modalRect.x + 2, modalRect.y + 26, modalRect.width, 36), "ENTER YOUR NAME", modalHeader);

            modalHeader.normal.textColor = new Color(0.2f, 0.95f, 1.4f);
            GUI.Label(new Rect(modalRect.x, modalRect.y + 24, modalRect.width, 36), "ENTER YOUR NAME", modalHeader);

            // 2. Subtitle / Instruction Line
            GUIStyle subStyle = new GUIStyle(cardBodyStyle)
            {
                font = displayFont,
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter
            };
            subStyle.normal.textColor = new Color(0.75f, 0.85f, 0.98f);
            GUI.Label(new Rect(modalRect.x, modalRect.y + 64, modalRect.width, 22), "Returning runner continues record. New runner starts at 0 pts.", subStyle);

            // 3. Stylized Input Box Container
            float inputW = cardW - 70f;
            float inputH = 50f;
            Rect inputRect = new Rect(midX - inputW * 0.5f, modalRect.y + 104f, inputW, inputH);

            GUI.color = new Color(0.05f, 0.07f, 0.12f, 0.95f);
            GUI.DrawTexture(inputRect, Texture2D.whiteTexture);
            GUI.color = new Color(0.3f, 0.85f, 1.2f, 0.9f); // Cyan neon border
            DrawBorder(inputRect, 2f);
            GUI.color = Color.white;

            // User icon on the left
            GUIStyle iconStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter
            };
            iconStyle.normal.textColor = new Color(0.3f, 0.95f, 1.4f);
            GUI.Label(new Rect(inputRect.x + 8, inputRect.y, 36, inputH), "👤", iconStyle);

            // Text Field
            Rect fieldRect = new Rect(inputRect.x + 46, inputRect.y + 4, inputW - 56, inputH - 8);
            GUI.SetNextControlName("PlayerNameInputField");
            inputPlayerName = GUI.TextField(fieldRect, inputPlayerName, 14, inputFieldStyle);

            if (focusNameFieldNext)
            {
                GUI.FocusControl("PlayerNameInputField");
                focusNameFieldNext = false;
            }

            // Placeholder hint if field is empty
            if (string.IsNullOrEmpty(inputPlayerName) && GUI.GetNameOfFocusedControl() != "PlayerNameInputField")
            {
                GUIStyle phStyle = new GUIStyle(inputFieldStyle)
                {
                    fontSize = 16,
                    fontStyle = FontStyle.Italic
                };
                phStyle.normal.textColor = new Color(0.45f, 0.55f, 0.7f, 0.7f);
                GUI.Label(fieldRect, "Type your name...", phStyle);
            }

            // Keyboard Enter handler
            if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
            {
                ConfirmPlayerName();
                Event.current.Use();
            }

            // 4. Action Buttons (CANCEL on left, CONFIRM/PLAY on right)
            float btnH = 46f;
            float btnY = modalRect.y + 180f;
            float bW = 160f;
            float gap = 16f;
            float leftBtnX = midX - bW - gap * 0.5f;
            float rightBtnX = midX + gap * 0.5f;

            string confirmLabel = startGameOnConfirm ? "PLAY NOW" : "CONFIRM";

            if (DrawStyledButton(new Rect(leftBtnX, btnY, bW, btnH), "CANCEL", ButtonTheme.SecondaryIndigo))
            {
                if (GameManager.Instance != null && !GameManager.Instance.HasPlayerName)
                {
                    GameManager.Instance.SetPlayerName("RUNNER");
                }
                showNameModal = false;
            }

            if (DrawStyledButton(new Rect(rightBtnX, btnY, bW, btnH), confirmLabel, ButtonTheme.PrimaryCyan))
            {
                ConfirmPlayerName();
            }

            // Helper hint at the bottom
            GUIStyle hintStyle = new GUIStyle(cardBodyStyle)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter
            };
            hintStyle.normal.textColor = new Color(0.5f, 0.6f, 0.75f);
            GUI.Label(new Rect(modalRect.x, modalRect.y + cardH - 32, modalRect.width, 22), "Max 14 characters  •  Press [ ENTER ] to confirm", hintStyle);
        }

        // ==========================================
        // 2. INSTRUCTIONS SCREEN (HOW TO PLAY)
        // ==========================================
        private void DrawInstructionsMenu()
        {
            DrawFullScreenBackdrop(new Color(0.02f, 0.03f, 0.06f, 0.78f));

            float midX = Screen.width * 0.5f;
            float midY = Screen.height * 0.5f;

            float cardW = Mathf.Clamp(Screen.width * 0.90f, 620f, 780f);
            float cardH = Mathf.Clamp(Screen.height * 0.88f, 460f, 520f);
            Rect panelRect = new Rect(midX - cardW * 0.5f, midY - cardH * 0.5f, cardW, cardH);

            DrawGlassPanel(panelRect);

            // 1. Header & Subtitle
            headerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(panelRect.x, panelRect.y + 18, panelRect.width, 36), "HOW TO PLAY", headerStyle);

            GUIStyle subHeader = new GUIStyle(cardBodyStyle)
            {
                font = displayFont,
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter
            };
            subHeader.normal.textColor = new Color(0.75f, 0.85f, 1f, 0.75f);
            GUI.Label(new Rect(panelRect.x, panelRect.y + 52, panelRect.width, 22), "MASTER THE DUALITY OF LIGHT AND SHADOW", subHeader);

            float startY = panelRect.y + 82f;
            float subW = panelRect.width - 48f;
            float subX = panelRect.x + 24f;
            float itemH = 82f;

            // 2. Directive Card 1: CRIMSON SPIKES
            DrawDirectiveCard(
                new Rect(subX, startY, subW, itemH),
                iconJumpTex,
                new Color(1.6f, 0.28f, 0.38f),
                "CRIMSON SPIKES",
                "JUMP:  Press [ W ] or [ UP ARROW ]",
                "Solid hazard in all realms. Jump over to clear.",
                "JUMP"
            );

            // 3. Directive Card 2: ENERGY GATES
            DrawDirectiveCard(
                new Rect(subX, startY + 94f, subW, itemH),
                iconPhaseTex,
                new Color(0.2f, 0.95f, 1.4f),
                "ENERGY GATES",
                "PHASE:  Press [ SPACEBAR ] or [ CLICK ]",
                "Cannot be jumped. Match opposite color to ghost through.",
                "PHASE"
            );

            // 4. Directive Card 3: SCORING & GEMS
            DrawDirectiveCard(
                new Rect(subX, startY + 188f, subW, itemH),
                null,
                new Color(1.3f, 0.88f, 0.25f),
                "SCORING & GEMS",
                "SCORE:  +1 per obstacle & Prism Orb collected",
                "Phase through matching gates and grab orbs to beat your best!",
                "SCORE"
            );

            // 5. Action Buttons (Vibrant styled buttons)
            float btnW = 200f;
            float btnH = 48f;
            float bY = panelRect.y + panelRect.height - 64f;

            if (DrawStyledButton(new Rect(midX - btnW - 14, bY, btnW, btnH), "PLAY", ButtonTheme.PrimaryCyan))
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.StartGame();
                }
            }

            if (DrawStyledButton(new Rect(midX + 14, bY, btnW, btnH), "BACK", ButtonTheme.SecondaryIndigo))
            {
                GameManager.Instance.OpenMainMenu();
            }
        }

        private void DrawDirectiveCard(Rect r, Texture2D icon, Color accent, string title, string action, string rule, string tag)
        {
            GUI.color = new Color(0.06f, 0.08f, 0.14f, 0.94f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);

            // Accent left bar
            GUI.color = accent;
            GUI.DrawTexture(new Rect(r.x, r.y, 4, r.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            float leftOffset = (icon != null) ? 68f : 20f;
            float textLeft = r.x + leftOffset;

            if (icon != null)
            {
                GUI.color = accent;
                GUI.DrawTexture(new Rect(r.x + 14, r.y + (r.height - 42) * 0.5f, 42, 42), icon);
                GUI.color = Color.white;
            }

            float tagW = 90f;
            float tagH = 34f;
            float tagRightMargin = 14f;
            // Correct relative width calculation:
            float contentW = r.width - leftOffset - tagW - tagRightMargin - 16f;

            // 1. Title / Header
            GUIStyle cardTitle = new GUIStyle(cardTitleStyle)
            {
                font = displayFont,
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = false
            };
            cardTitle.normal.textColor = accent;
            GUI.Label(new Rect(textLeft, r.y + 9, contentW, 22), title, cardTitle);

            // 2. Action Line (Bold White)
            GUIStyle actStyle = new GUIStyle(cardBodyStyle)
            {
                font = displayFont,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = false
            };
            actStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(textLeft, r.y + 33, contentW, 20), action, actStyle);

            // 3. Rule Line (Legible Silver/Blue)
            GUIStyle ruleStyle = new GUIStyle(cardBodyStyle)
            {
                fontSize = 13,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = false
            };
            ruleStyle.normal.textColor = new Color(0.78f, 0.85f, 0.95f);
            GUI.Label(new Rect(textLeft, r.y + 55, contentW, 20), rule, ruleStyle);

            // 4. Right Action Tag
            Rect tagRect = new Rect(r.x + r.width - tagW - tagRightMargin, r.y + (r.height - tagH) * 0.5f, tagW, tagH);
            GUI.color = new Color(accent.r * 0.28f, accent.g * 0.28f, accent.b * 0.28f, 0.95f);
            GUI.DrawTexture(tagRect, Texture2D.whiteTexture);
            GUI.color = accent;
            DrawBorder(tagRect, 1.5f);
            GUI.color = Color.white;

            GUIStyle tagStyle = new GUIStyle(cardTitleStyle)
            {
                font = displayFont,
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13
            };
            tagStyle.normal.textColor = Color.white;
            GUI.Label(tagRect, tag, tagStyle);
        }

        // ==========================================
        // 3. IN-GAME HUD
        // ==========================================
        private void DrawHUD()
        {
            if (GameManager.Instance == null) return;

            bool isLight = DimensionManager.Instance != null && DimensionManager.Instance.CurrentDimension == DimensionType.Light;

            // Clean floating score display with high-contrast drop-shadows (no box/background)
            float scoreX = 28f;
            float scoreY = 22f;

            // 1. SCORE
            hudStyle.normal.textColor = new Color(0.02f, 0.03f, 0.06f, 0.95f);
            GUI.Label(new Rect(scoreX + 2, scoreY + 2, 300, 35), $"SCORE: {GameManager.Instance.CurrentScore:N0}", hudStyle);
            hudStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(scoreX, scoreY, 300, 35), $"SCORE: {GameManager.Instance.CurrentScore:N0}", hudStyle);

            // 2. BEST SCORE
            GUIStyle subHud = new GUIStyle(hudStyle) { fontSize = 16 };
            subHud.normal.textColor = new Color(0.02f, 0.03f, 0.06f, 0.95f);
            GUI.Label(new Rect(scoreX + 2, scoreY + 36, 300, 26), $"BEST:  {GameManager.Instance.HighScore:N0}", subHud);
            subHud.normal.textColor = new Color(1f, 0.88f, 0.35f, 0.95f);
            GUI.Label(new Rect(scoreX, scoreY + 34, 300, 26), $"BEST:  {GameManager.Instance.HighScore:N0}", subHud);

            // 3. RUNNER CALLSIGN
            string pName = (GameManager.Instance != null && GameManager.Instance.HasPlayerName) ? GameManager.Instance.PlayerName : "RUNNER";
            GUIStyle runnerStyle = new GUIStyle(hudStyle) { fontSize = 13 };
            runnerStyle.normal.textColor = new Color(0.02f, 0.03f, 0.06f, 0.95f);
            GUI.Label(new Rect(scoreX + 2, scoreY + 60, 300, 22), $"RUNNER: {pName}", runnerStyle);
            runnerStyle.normal.textColor = new Color(0.3f, 0.95f, 1.4f, 0.95f);
            GUI.Label(new Rect(scoreX, scoreY + 59, 300, 22), $"RUNNER: {pName}", runnerStyle);

            // 4. CORNER PAUSE BUTTON (Top-Right - High-tech cyber pause button)
            float pauseSize = 44f;
            Rect pauseRect = new Rect(Screen.width - pauseSize - 22f, 20f, pauseSize, pauseSize);

            bool isPauseHover = pauseRect.Contains(Event.current.mousePosition);

            if (buttonTex != null)
            {
                GUI.color = isPauseHover ? new Color(0.15f, 0.45f, 0.70f, 0.98f) : new Color(0.08f, 0.20f, 0.35f, 0.90f);
                GUI.DrawTexture(pauseRect, buttonTex);
            }
            else
            {
                GUI.color = isPauseHover ? new Color(0.12f, 0.35f, 0.55f, 0.98f) : new Color(0.04f, 0.14f, 0.24f, 0.90f);
                GUI.DrawTexture(pauseRect, Texture2D.whiteTexture);
            }
            GUI.color = Color.white;

            // Draw crisp pause bars (||)
            Color barCol = isPauseHover ? Color.white : new Color(0.9f, 0.95f, 1f, 0.95f);
            GUI.color = barCol;
            float barW = 5f;
            float barH = 18f;
            float barY = pauseRect.y + (pauseSize - barH) * 0.5f;
            float barGap = 6f;
            float totalBarsW = barW * 2f + barGap;
            float startBarX = pauseRect.x + (pauseSize - totalBarsW) * 0.5f;

            GUI.DrawTexture(new Rect(startBarX, barY, barW, barH), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(startBarX + barW + barGap, barY, barW, barH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Interaction
            if (GUI.Button(pauseRect, GUIContent.none, GUIStyle.none))
            {
                SpawnCosmicClickEffect(pauseRect.center, new Color(0.2f, 0.9f, 1.4f));
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                GameManager.Instance.PauseGame();
            }
        }

        // ==========================================
        // 3.5. IN-GAME PAUSE MENU MODAL
        // ==========================================
        private void DrawPauseMenu()
        {
            if (GameManager.Instance == null) return;

            // Fullscreen soft dark glass overlay over the frozen gameplay
            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
            GUI.color = new Color(0.02f, 0.03f, 0.07f, 0.80f);
            GUI.DrawTexture(screenRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            float midX = Screen.width * 0.5f;
            float midY = Screen.height * 0.5f;

            float cardW = Mathf.Min(480f, Screen.width * 0.88f);
            float cardH = Mathf.Min(358f, Screen.height * 0.85f);
            Rect panelRect = new Rect(midX - cardW * 0.5f, midY - cardH * 0.5f, cardW, cardH);

            DrawGlassPanel(panelRect);

            // 1. Header: "PAUSED" (Cyan / Light Blue neon glow)
            GUIStyle pauseHeader = new GUIStyle(headerStyle) { fontSize = 42 };
            pauseHeader.normal.textColor = new Color(0.02f, 0.04f, 0.08f, 0.9f);
            GUI.Label(new Rect(panelRect.x + 2, panelRect.y + 30, panelRect.width, 50), "PAUSED", pauseHeader);

            pauseHeader.normal.textColor = new Color(0.25f, 0.85f, 1.3f);
            GUI.Label(new Rect(panelRect.x, panelRect.y + 28, panelRect.width, 50), "PAUSED", pauseHeader);

            // 2. Score Banner Box
            float boxW = panelRect.width - 60f;
            float boxH = 54f;
            Rect boxRect = new Rect(panelRect.x + 30f, panelRect.y + 90f, boxW, boxH);

            GUI.color = new Color(0.08f, 0.10f, 0.16f, 0.95f);
            GUI.DrawTexture(boxRect, Texture2D.whiteTexture);
            GUI.color = new Color(0.25f, 0.8f, 1.1f, 0.35f);
            DrawBorder(boxRect, 1.5f);
            GUI.color = Color.white;

            GUIStyle scoreSummaryStyle = new GUIStyle(GUI.skin.label)
            {
                font = displayFont,
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            string pauseRunner = (GameManager.Instance != null && GameManager.Instance.HasPlayerName) ? GameManager.Instance.PlayerName : "RUNNER";
            GUI.Label(boxRect, $"{pauseRunner}   |   SCORE: {GameManager.Instance.CurrentScore:N0}   |   BEST: {GameManager.Instance.HighScore:N0}", scoreSummaryStyle);

            // 3. Action Buttons (Vibrant styled buttons)
            float btnW = 260f;
            float btnH = 46f;
            float btnX = midX - btnW * 0.5f;

            if (DrawStyledButton(new Rect(btnX, panelRect.y + 164, btnW, btnH), "RESUME", ButtonTheme.PrimaryCyan))
            {
                GameManager.Instance.ResumeGame();
            }

            if (DrawStyledButton(new Rect(btnX, panelRect.y + 224, btnW, btnH), "RESTART", ButtonTheme.SecondaryIndigo))
            {
                GameManager.Instance.RestartGame();
            }

            if (DrawStyledButton(new Rect(btnX, panelRect.y + 284, btnW, btnH), "MAIN MENU", ButtonTheme.SecondaryIndigo))
            {
                GameManager.Instance.OpenMainMenu();
            }
        }

        // ==========================================
        // 4. OVERHAULED GAME OVER SCREEN
        // ==========================================
        private void DrawGameOverMenu()
        {
            if (GameManager.Instance == null) return;

            // Fullscreen Celestial Background with Dramatic Crimson Vignette
            DrawFullScreenBackdrop(new Color(0.12f, 0.02f, 0.04f, 0.80f));

            float midX = Screen.width * 0.5f;
            float midY = Screen.height * 0.5f;

            float cardW = Mathf.Min(540f, Screen.width * 0.88f);
            float cardH = Mathf.Min(430f, Screen.height * 0.85f);
            Rect panelRect = new Rect(midX - cardW * 0.5f, midY - cardH * 0.5f, cardW, cardH);

            DrawGlassPanel(panelRect);

            // 1. BOLD "GAME OVER" (Neon Crimson Red with shadow)
            GUIStyle overTitle = new GUIStyle(headerStyle) { fontSize = 48 };
            overTitle.normal.textColor = new Color(0.05f, 0.01f, 0.02f, 0.9f);
            GUI.Label(new Rect(panelRect.x + 2, panelRect.y + 34, panelRect.width, 55), "GAME OVER", overTitle);

            overTitle.normal.textColor = new Color(1.5f, 0.2f, 0.32f); // Luminous Crimson
            GUI.Label(new Rect(panelRect.x, panelRect.y + 32, panelRect.width, 55), "GAME OVER", overTitle);

            // 2. High Score Celebration or Divider
            if (GameManager.Instance.IsNewHighScore)
            {
                GUIStyle recordStyle = new GUIStyle(GUI.skin.label)
                {
                    font = displayFont,
                    fontSize = 18,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                recordStyle.normal.textColor = new Color(1.3f, 0.88f, 0.25f); // Solar Gold
                GUI.Label(new Rect(panelRect.x, panelRect.y + 92, panelRect.width, 28), "★ NEW RECORD! ★", recordStyle);
            }
            else
            {
                GUI.color = new Color(0.25f, 0.8f, 1.1f, 0.35f);
                GUI.DrawTexture(new Rect(panelRect.x + 80, panelRect.y + 96, panelRect.width - 160, 1), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }

            // 3. Stats Box
            float boxW = panelRect.width - 70f;
            float boxH = 105f;
            Rect boxRect = new Rect(panelRect.x + 35f, panelRect.y + 120f, boxW, boxH);

            GUI.color = new Color(0.08f, 0.10f, 0.16f, 0.95f);
            GUI.DrawTexture(boxRect, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUIStyle scoreValStyle = new GUIStyle(GUI.skin.label)
            {
                font = displayFont,
                fontSize = 34,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            scoreValStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(boxRect.x, boxRect.y + 14, boxRect.width, 40), $"{GameManager.Instance.CurrentScore:N0} PTS", scoreValStyle);

            GUIStyle statLine = new GUIStyle(GUI.skin.label)
            {
                font = displayFont,
                fontSize = 16,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter
            };
            statLine.normal.textColor = new Color(0.7f, 0.8f, 0.95f);
            GUI.Label(new Rect(boxRect.x, boxRect.y + 60, boxRect.width, 28), $"BEST: {GameManager.Instance.HighScore:N0} PTS", statLine);

            // 4. Action Buttons (Vibrant styled buttons with neon glow and glass sheen)
            float btnW = 280f;
            float btnH = 50f;
            float btnX = midX - btnW * 0.5f;

            if (DrawStyledButton(new Rect(btnX, panelRect.y + 250, btnW, btnH), "PLAY AGAIN", ButtonTheme.PrimaryCyan))
            {
                GameManager.Instance.RestartGame();
            }

            if (DrawStyledButton(new Rect(btnX, panelRect.y + 318, btnW, btnH), "MAIN MENU", ButtonTheme.SecondaryIndigo))
            {
                GameManager.Instance.OpenMainMenu();
            }
        }

        // ==========================================
        // HELPERS & STYLED BUTTONS
        // ==========================================
        public enum ButtonTheme
        {
            PrimaryCyan,
            SecondaryIndigo,
            CrimsonAlert,
            SolarGold
        }

        private bool DrawStyledButton(Rect rect, string text, ButtonTheme theme = ButtonTheme.PrimaryCyan, bool interactive = true)
        {
            bool isHover = interactive && rect.Contains(Event.current.mousePosition);

            // 1. Text color and particle effect color
            Color textColor;
            Color effectColor;

            switch (theme)
            {
                case ButtonTheme.SecondaryIndigo:
                    textColor = isHover ? new Color(1f, 0.95f, 1f) : new Color(0.90f, 0.88f, 1f);
                    effectColor = new Color(0.6f, 0.45f, 1.2f);
                    break;

                case ButtonTheme.CrimsonAlert:
                    textColor = isHover ? new Color(1f, 0.92f, 0.94f) : Color.white;
                    effectColor = new Color(1.3f, 0.2f, 0.35f);
                    break;

                case ButtonTheme.SolarGold:
                    textColor = isHover ? Color.white : new Color(1f, 0.95f, 0.85f);
                    effectColor = new Color(1.3f, 0.85f, 0.2f);
                    break;

                case ButtonTheme.PrimaryCyan:
                default:
                    textColor = isHover ? new Color(0.35f, 1.15f, 1.6f) : Color.white;
                    effectColor = new Color(0.25f, 0.95f, 1.4f);
                    break;
            }

            // 2. Click press offset (subtle responsive tactile sink on press)
            Rect drawRect = rect;
            if (isHover && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                drawRect.y += 2f;
            }

            // 3. Draw Clean Rounded Pill Texture (NO square box or wireframe outline!)
            Texture2D bodyTex = isHover ? (buttonHoverTex ?? buttonTex) : buttonTex;
            if (bodyTex != null)
            {
                GUI.color = Color.white;
                GUI.DrawTexture(drawRect, bodyTex);
            }
            else
            {
                GUI.color = new Color(0.08f, 0.32f, 0.52f, 0.95f);
                GUI.DrawTexture(drawRect, Texture2D.whiteTexture);
            }
            GUI.color = Color.white;

            // 4. High-Contrast Typography with Double Drop-Shadow
            int fontSize = Mathf.RoundToInt(drawRect.height * 0.40f);
            GUIStyle btnLabel = new GUIStyle(GUI.skin.label)
            {
                font = displayFont,
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            // Deep Drop Shadow
            btnLabel.normal.textColor = new Color(0.01f, 0.02f, 0.04f, 0.95f);
            GUI.Label(new Rect(drawRect.x + 1.5f, drawRect.y + 2f, drawRect.width, drawRect.height), text, btnLabel);

            // Crisp Main Text
            btnLabel.normal.textColor = textColor;
            GUI.Label(drawRect, text, btnLabel);

            // 5. Interaction Trigger with Celestial Click Effect!
            if (!interactive) return false;

            bool clicked = GUI.Button(rect, GUIContent.none, GUIStyle.none);
            if (clicked)
            {
                Vector2 clickPos = Event.current.mousePosition;
                if (clickPos == Vector2.zero || !rect.Contains(clickPos))
                {
                    clickPos = rect.center;
                }

                SpawnCosmicClickEffect(clickPos, effectColor);

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayButtonClick();
                }
            }

            return clicked;
        }

        // ==========================================
        // CELESTIAL PRISM & STARBURST CLICK EFFECTS
        // ==========================================
        private void EnsureCosmicTextures()
        {
            if (starGlintTex != null && diamondShardTex != null && novaRingTex != null && starburstFlareTex != null) return;

            // 1. Procedural 4-Point Radiant Star Glint (64x64)
            int sSize = 64;
            starGlintTex = new Texture2D(sSize, sSize, TextureFormat.RGBA32, false);
            starGlintTex.wrapMode = TextureWrapMode.Clamp;
            Vector2 sCenter = new Vector2(sSize * 0.5f, sSize * 0.5f);
            float maxR = sSize * 0.48f;

            for (int y = 0; y < sSize; y++)
            {
                for (int x = 0; x < sSize; x++)
                {
                    float dx = x - sCenter.x;
                    float dy = y - sCenter.y;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    // Central glowing core
                    float core = Mathf.Pow(Mathf.Clamp01(1f - (dist / (maxR * 0.30f))), 2f) * 0.95f;

                    // Sharp horizontal beam
                    float hBeam = Mathf.Pow(Mathf.Clamp01(1f - (Mathf.Abs(dx) / maxR)), 1.4f) *
                                  Mathf.Pow(Mathf.Clamp01(1f - (Mathf.Abs(dy) / 4.2f)), 2.2f);

                    // Sharp vertical beam
                    float vBeam = Mathf.Pow(Mathf.Clamp01(1f - (Mathf.Abs(dy) / maxR)), 1.4f) *
                                  Mathf.Pow(Mathf.Clamp01(1f - (Mathf.Abs(dx) / 4.2f)), 2.2f);

                    // Soft diagonal cross
                    float diag1 = Mathf.Pow(Mathf.Clamp01(1f - (dist / (maxR * 0.55f))), 2f) *
                                  Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(dx - dy) / 6f), 2f) * 0.45f;
                    float diag2 = Mathf.Pow(Mathf.Clamp01(1f - (dist / (maxR * 0.55f))), 2f) *
                                  Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(dx + dy) / 6f), 2f) * 0.45f;

                    float alpha = Mathf.Clamp01(core + hBeam + vBeam + diag1 + diag2);
                    starGlintTex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            starGlintTex.Apply();

            // 2. Procedural Faceted Diamond Prism Crystal Shard (48x48)
            int dSize = 48;
            diamondShardTex = new Texture2D(dSize, dSize, TextureFormat.RGBA32, false);
            diamondShardTex.wrapMode = TextureWrapMode.Clamp;
            Vector2 dCenter = new Vector2(dSize * 0.5f, dSize * 0.5f);
            float hw = dSize * 0.26f;
            float hh = dSize * 0.46f;

            for (int y = 0; y < dSize; y++)
            {
                for (int x = 0; x < dSize; x++)
                {
                    float dx = x - dCenter.x;
                    float dy = y - dCenter.y;

                    // Rhombus equation: |dx|/hw + |dy|/hh <= 1
                    float d = (Mathf.Abs(dx) / hw) + (Mathf.Abs(dy) / hh);
                    if (d > 1f)
                    {
                        diamondShardTex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    // Anti-aliased outer edge
                    float edgeAlpha = Mathf.Clamp01((1f - d) / 0.12f);

                    // Faceted jewel shading: bright specular facet on top-left
                    float facetShade = (dx < 0f) ? 1.0f : 0.78f;
                    if (dy > 0f) facetShade += 0.16f;
                    float rim = Mathf.Pow(d, 2.5f) * 0.4f;
                    float brightness = Mathf.Clamp01(facetShade + rim);

                    diamondShardTex.SetPixel(x, y, new Color(brightness, brightness, brightness, edgeAlpha * 0.95f));
                }
            }
            diamondShardTex.Apply();

            // 3. Procedural Concentric Nova Ripple Ring (128x128)
            int rSize = 128;
            novaRingTex = new Texture2D(rSize, rSize, TextureFormat.RGBA32, false);
            novaRingTex.wrapMode = TextureWrapMode.Clamp;
            Vector2 rCenter = new Vector2(rSize * 0.5f, rSize * 0.5f);
            float rTarget = rSize * 0.44f;
            float rThick = 4.2f;

            for (int y = 0; y < rSize; y++)
            {
                for (int x = 0; x < rSize; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), rCenter);
                    float delta = Mathf.Abs(dist - rTarget);
                    if (delta <= rThick)
                    {
                        float a = Mathf.Clamp01(1f - (delta / rThick));
                        novaRingTex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                    }
                    else
                    {
                        novaRingTex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            novaRingTex.Apply();

            // 4. Procedural 8-Point Diffraction Starburst Flare (128x128)
            int fSize = 128;
            starburstFlareTex = new Texture2D(fSize, fSize, TextureFormat.RGBA32, false);
            starburstFlareTex.wrapMode = TextureWrapMode.Clamp;
            Vector2 fCenter = new Vector2(fSize * 0.5f, fSize * 0.5f);
            float fR = fSize * 0.48f;

            for (int y = 0; y < fSize; y++)
            {
                for (int x = 0; x < fSize; x++)
                {
                    float dx = x - fCenter.x;
                    float dy = y - fCenter.y;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    // Central luminous core
                    float core = Mathf.Pow(Mathf.Clamp01(1f - (dist / (fR * 0.35f))), 2.2f);

                    // Primary cross spikes
                    float hSpike = Mathf.Pow(Mathf.Clamp01(1f - (Mathf.Abs(dx) / fR)), 1.2f) *
                                   Mathf.Pow(Mathf.Clamp01(1f - (Mathf.Abs(dy) / 4f)), 2f);
                    float vSpike = Mathf.Pow(Mathf.Clamp01(1f - (Mathf.Abs(dy) / fR)), 1.2f) *
                                   Mathf.Pow(Mathf.Clamp01(1f - (Mathf.Abs(dx) / 4f)), 2f);

                    // Diagonal 45 deg spikes
                    float diag1 = Mathf.Pow(Mathf.Clamp01(1f - (dist / (fR * 0.70f))), 1.5f) *
                                  Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(dx - dy) / 5.5f), 2f) * 0.55f;
                    float diag2 = Mathf.Pow(Mathf.Clamp01(1f - (dist / (fR * 0.70f))), 1.5f) *
                                  Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(dx + dy) / 5.5f), 2f) * 0.55f;

                    float alpha = Mathf.Clamp01(core * 0.95f + (hSpike + vSpike) * 0.9f + diag1 + diag2);
                    starburstFlareTex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            starburstFlareTex.Apply();
        }

        public void SpawnCosmicClickEffect(Vector2 origin, Color themeColor)
        {
            EnsureCosmicTextures();

            // 1. Instant Center Starburst Flare
            activeFlares.Add(new StarburstFlare
            {
                center = origin,
                currentSize = 28f,
                targetSize = 92f,
                life = 0.22f,
                maxLife = 0.22f,
                color = Color.Lerp(themeColor, Color.white, 0.45f)
            });

            // 2. Expanding Celestial Nova Shockwaves
            activeNovaRings.Add(new NovaRing
            {
                center = origin,
                currentRadius = 10f,
                maxRadius = 65f,
                life = 0.24f,
                maxLife = 0.24f,
                color = Color.white
            });

            activeNovaRings.Add(new NovaRing
            {
                center = origin,
                currentRadius = 16f,
                maxRadius = 110f,
                life = 0.38f,
                maxLife = 0.38f,
                color = themeColor
            });

            // 3. Radial Burst of 14-18 Rotating Star Glints & Prism Diamond Crystals
            // Theme colors: Solar Gold (Light), Lunar Cyan (Shadow), and Radiant White
            Color solarGold = new Color(1.0f, 0.88f, 0.28f);
            Color lunarCyan = new Color(0.22f, 0.92f, 1.0f);
            Color starlightWhite = new Color(1.0f, 1.0f, 1.0f);

            int count = Random.Range(14, 18);
            for (int i = 0; i < count; i++)
            {
                float baseAngle = (i * (360f / count)) + Random.Range(-12f, 12f);
                float rad = baseAngle * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                float speed = Random.Range(130f, 380f);

                // Alternating celestial colors: theme, gold, cyan, white
                Color sparkColor;
                int colMod = i % 4;
                if (colMod == 0) sparkColor = starlightWhite;
                else if (colMod == 1) sparkColor = solarGold;
                else if (colMod == 2) sparkColor = lunarCyan;
                else sparkColor = themeColor;

                CosmicParticleType pType = (i % 2 == 0) ? CosmicParticleType.StarGlint : CosmicParticleType.DiamondShard;

                activeSparks.Add(new CosmicSpark
                {
                    position = origin + dir * Random.Range(4f, 14f),
                    velocity = dir * speed,
                    rotation = Random.Range(0f, 360f),
                    angularVelocity = Random.Range(-320f, 320f),
                    size = (pType == CosmicParticleType.StarGlint) ? Random.Range(20f, 36f) : Random.Range(16f, 30f),
                    life = Random.Range(0.38f, 0.58f),
                    maxLife = 0.58f,
                    color = sparkColor,
                    particleType = pType
                });
            }
        }

        private void UpdateAndDrawCosmicEffects()
        {
            EnsureCosmicTextures();
            float dt = Time.unscaledDeltaTime;
            if (dt <= 0f || dt > 0.1f) dt = 0.016f;

            Matrix4x4 origMatrix = GUI.matrix;

            // 1. Draw Nova Shockwave Rings
            for (int i = activeNovaRings.Count - 1; i >= 0; i--)
            {
                var r = activeNovaRings[i];
                r.life -= dt;
                if (r.life <= 0f)
                {
                    activeNovaRings.RemoveAt(i);
                    continue;
                }

                float progress = 1f - (r.life / r.maxLife);
                float radius = Mathf.Lerp(r.currentRadius, r.maxRadius, Mathf.Sin(progress * Mathf.PI * 0.5f));
                float alpha = (1f - progress) * 0.88f;

                Rect rRect = new Rect(r.center.x - radius, r.center.y - radius, radius * 2f, radius * 2f);
                GUI.color = new Color(r.color.r, r.color.g, r.color.b, alpha);
                GUI.DrawTexture(rRect, novaRingTex);
            }

            // 2. Draw Center Starburst Flares
            for (int i = activeFlares.Count - 1; i >= 0; i--)
            {
                var f = activeFlares[i];
                f.life -= dt;
                if (f.life <= 0f)
                {
                    activeFlares.RemoveAt(i);
                    continue;
                }

                float progress = 1f - (f.life / f.maxLife);
                float size = Mathf.Lerp(f.currentSize, f.targetSize, Mathf.Sin(progress * Mathf.PI * 0.5f));
                float alpha = Mathf.Clamp01(1f - progress) * 0.95f;

                Rect fRect = new Rect(f.center.x - size * 0.5f, f.center.y - size * 0.5f, size, size);
                GUI.color = new Color(f.color.r, f.color.g, f.color.b, alpha);
                GUI.DrawTexture(fRect, starburstFlareTex);
            }

            // 3. Draw Rotating Star Glints & Prism Diamond Crystals
            for (int i = activeSparks.Count - 1; i >= 0; i--)
            {
                var s = activeSparks[i];
                s.life -= dt;
                if (s.life <= 0f)
                {
                    activeSparks.RemoveAt(i);
                    continue;
                }

                // Physics motion: velocity + aerodynamic deceleration drag
                s.position += s.velocity * dt;
                s.velocity *= Mathf.Pow(0.85f, dt * 60f); // drag
                s.rotation += s.angularVelocity * dt;

                float progress = 1f - (s.life / s.maxLife);
                // Celestial scale curve: quick bloom then smooth tapering
                float scale = (progress < 0.18f)
                    ? Mathf.Lerp(0.4f, 1.15f, progress / 0.18f)
                    : Mathf.Lerp(1.15f, 0.25f, (progress - 0.18f) / 0.82f);
                float renderSize = s.size * scale;
                float alpha = Mathf.Clamp01((1f - progress) * 1.35f);

                Rect sRect = new Rect(s.position.x - renderSize * 0.5f, s.position.y - renderSize * 0.5f, renderSize, renderSize);

                GUIUtility.RotateAroundPivot(s.rotation, s.position);
                GUI.color = new Color(s.color.r, s.color.g, s.color.b, alpha);
                Texture2D texToDraw = (s.particleType == CosmicParticleType.StarGlint) ? starGlintTex : diamondShardTex;
                GUI.DrawTexture(sRect, texToDraw);
                GUI.matrix = origMatrix;
            }

            GUI.color = Color.white;
        }

        private void DrawFullScreenBackdrop(Color scrimColor)
        {
            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
            Texture2D backdrop = titleLogoTex ?? bgTex;
            if (backdrop != null)
            {
                GUI.color = Color.white;
                GUI.DrawTexture(screenRect, backdrop, ScaleMode.ScaleAndCrop);
            }
            else
            {
                GUI.color = new Color(0.04f, 0.05f, 0.09f, 1f);
                GUI.DrawTexture(screenRect, Texture2D.whiteTexture);
            }

            // Scrim overlay for contrast and legibility
            GUI.color = scrimColor;
            GUI.DrawTexture(screenRect, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        private void DrawGlassPanel(Rect rect)
        {
            if (panelTex != null)
            {
                GUI.DrawTexture(rect, panelTex);
            }
            else
            {
                GUI.color = new Color(0.05f, 0.06f, 0.11f, 0.95f);
                GUI.DrawTexture(rect, Texture2D.whiteTexture);
                GUI.color = Color.white;
            }
        }

        private void DrawBorder(Rect r, float thickness)
        {
            GUI.DrawTexture(new Rect(r.x, r.y, r.width, thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y + r.height - thickness, r.width, thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y, thickness, r.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x + r.width - thickness, r.y, thickness, r.height), Texture2D.whiteTexture);
        }
    }
}
