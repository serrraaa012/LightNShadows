using UnityEngine;

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
        private GUIStyle comboStyle;

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

            comboStyle = new GUIStyle(GUI.skin.label)
            {
                font = displayFont,
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft
            };

            stylesInitialized = true;
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
                case GameManager.GameState.GameOver:
                    DrawGameOverMenu();
                    break;
            }
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
            // Fullscreen Celestial Eclipse Artwork
            DrawFullScreenBackdrop(new Color(0.02f, 0.03f, 0.06f, 0.40f));

            float midX = Screen.width * 0.5f;
            float midY = Screen.height * 0.5f;

            // 1. Sleek Typographic Title (No image on menu - clean and spacious!)
            float titleY = midY - 140f;
            Rect titleRect = new Rect(0, titleY, Screen.width, 70);

            // Shadow
            titleStyle.normal.textColor = new Color(0.02f, 0.03f, 0.08f, 0.95f);
            GUI.Label(new Rect(2, titleY + 3, Screen.width, 70), "LIGHT   N   SHADOWS", titleStyle);

            // Dual Glow Backlight
            titleStyle.normal.textColor = new Color(0.2f, 0.9f, 1.4f, 0.55f);
            GUI.Label(new Rect(-1, titleY - 1, Screen.width, 70), "LIGHT   N   SHADOWS", titleStyle);

            // Crisp Front Title
            titleStyle.normal.textColor = Color.white;
            GUI.Label(titleRect, "LIGHT   N   SHADOWS", titleStyle);

            // Sleek Dual Divider Bar: Solar Gold (Left) & Electric Cyan (Right)
            float divHalf = 150f;
            float divY = titleY + 68f;
            GUI.color = new Color(1.3f, 0.88f, 0.25f, 0.85f); // Solar Gold
            GUI.DrawTexture(new Rect(midX - divHalf, divY, divHalf, 2), Texture2D.whiteTexture);
            GUI.color = new Color(0.2f, 0.95f, 1.4f, 0.85f);  // Electric Cyan
            GUI.DrawTexture(new Rect(midX, divY, divHalf, 2), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // 2. High Score Ribbon Badge
            int best = (GameManager.Instance != null) ? GameManager.Instance.HighScore : 0;
            float badgeW = 280f;
            float badgeH = 38f;
            float badgeY = midY - 25f;
            Rect badgeRect = new Rect(midX - badgeW * 0.5f, badgeY, badgeW, badgeH);

            GUI.color = new Color(0.06f, 0.08f, 0.14f, 0.92f);
            GUI.DrawTexture(badgeRect, Texture2D.whiteTexture);
            GUI.color = new Color(1.3f, 0.88f, 0.25f, 0.85f); // Solar Gold Rim
            DrawBorder(badgeRect, 1.5f);
            GUI.color = Color.white;

            GUIStyle badgeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            badgeStyle.normal.textColor = new Color(1.3f, 0.88f, 0.25f);
            GUI.Label(badgeRect, $"★  RECORD: {best:N0} PTS  ★", badgeStyle);

            // 3. Action Buttons (Clean, responsive pill buttons)
            float btnW = 300f;
            float btnH = 56f;
            float btnX = midX - btnW * 0.5f;

            if (GUI.Button(new Rect(btnX, midY + 45f, btnW, btnH), "PLAY", buttonStyle))
            {
                GameManager.Instance.StartGame();
            }

            if (GUI.Button(new Rect(btnX, midY + 118f, btnW, btnH), "HOW TO PLAY", buttonStyle))
            {
                GameManager.Instance.OpenInstructions();
            }
        }

        // ==========================================
        // 2. INSTRUCTIONS SCREEN (HOW TO PLAY)
        // ==========================================
        private void DrawInstructionsMenu()
        {
            DrawFullScreenBackdrop(new Color(0.02f, 0.03f, 0.06f, 0.78f));

            float midX = Screen.width * 0.5f;
            float midY = Screen.height * 0.5f;

            float cardW = Mathf.Min(760f, Screen.width * 0.94f);
            float cardH = Mathf.Min(520f, Screen.height * 0.90f);
            Rect panelRect = new Rect(midX - cardW * 0.5f, midY - cardH * 0.5f, cardW, cardH);

            DrawGlassPanel(panelRect);

            // Header
            headerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(panelRect.x, panelRect.y + 22, panelRect.width, 38), "HOW TO PLAY", headerStyle);

            float startY = panelRect.y + 75f;
            float subW = panelRect.width - 50f;
            float subX = panelRect.x + 25f;

            // 1. CRIMSON SPIKES
            DrawDirectiveCard(
                new Rect(subX, startY, subW, 80),
                iconJumpTex,
                new Color(1.5f, 0.2f, 0.32f),
                "CRIMSON SPIKES  —  MUST JUMP",
                "Red spikes are SOLID hazards in both Light and Shadow worlds. Phasing cannot bypass them!\n" +
                "Action: Press [ W ] or [ UP ARROW ] to jump over.",
                "JUMP"
            );

            // 2. REALM GATES
            DrawDirectiveCard(
                new Rect(subX, startY + 95, subW, 80),
                iconPhaseTex,
                new Color(0.2f, 0.95f, 1.4f),
                "TALL GATES  —  MUST PHASE",
                "Imposing energy gates cannot be jumped. Shift into the OPPOSITE realm to ghost through safely!\n" +
                "Action: Press [ SPACEBAR ] or [ CLICK ] to swap realms.",
                "PHASE"
            );

            // 3. COMBOS & ORBS
            DrawDirectiveCard(
                new Rect(subX, startY + 190, subW, 80),
                null,
                new Color(1.3f, 0.85f, 0.2f),
                "COMBOS & PRISM GEMS  —  SCORE BOOST",
                "Consecutive phase passes increase your COMBO multiplier up to x5!\n" +
                "Collect floating diamond Prism Orbs along the track for +1 bonus point.",
                "x5 BOOST"
            );

            // Buttons
            float btnW = 200f;
            float btnH = 50f;
            float bY = panelRect.y + panelRect.height - 70f;

            if (GUI.Button(new Rect(midX - btnW - 15, bY, btnW, btnH), "PLAY", buttonStyle))
            {
                GameManager.Instance.StartGame();
            }

            if (GUI.Button(new Rect(midX + 15, bY, btnW, btnH), "BACK", buttonStyle))
            {
                GameManager.Instance.OpenMainMenu();
            }
        }

        private void DrawDirectiveCard(Rect r, Texture2D icon, Color accent, string title, string body, string tag)
        {
            GUI.color = new Color(0.08f, 0.10f, 0.16f, 0.92f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);

            // Accent left bar
            GUI.color = accent;
            GUI.DrawTexture(new Rect(r.x, r.y, 4, r.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            float textLeft = r.x + 18;
            if (icon != null)
            {
                GUI.color = accent;
                GUI.DrawTexture(new Rect(r.x + 14, r.y + 18, 44, 44), icon);
                GUI.color = Color.white;
                textLeft = r.x + 72;
            }

            cardTitleStyle.normal.textColor = accent;
            GUI.Label(new Rect(textLeft, r.y + 10, r.width - textLeft - 95, 24), title, cardTitleStyle);

            GUI.Label(new Rect(textLeft, r.y + 34, r.width - textLeft - 95, 40), body, cardBodyStyle);

            GUIStyle tagStyle = new GUIStyle(cardTitleStyle) { alignment = TextAnchor.MiddleCenter, fontSize = 13 };
            tagStyle.normal.textColor = Color.white;

            GUI.color = new Color(accent.r * 0.35f, accent.g * 0.35f, accent.b * 0.35f, 0.9f);
            GUI.DrawTexture(new Rect(r.x + r.width - 92, r.y + 24, 80, 32), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(r.x + r.width - 92, r.y + 24, 80, 32), tag, tagStyle);
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

            // 3. COMBO STREAK (Floating with drop-shadow, no black background)
            if (GameManager.Instance.ComboCount > 1)
            {
                float comboY = scoreY + 66f;
                Color comboColor = isLight ? new Color(1.3f, 0.88f, 0.25f) : new Color(0.2f, 0.95f, 1.4f);

                comboStyle.normal.textColor = new Color(0.02f, 0.03f, 0.06f, 0.95f);
                GUI.Label(new Rect(scoreX + 2, comboY + 2, 300, 30), $"★ COMBO x{GameManager.Instance.ComboCount}!", comboStyle);
                comboStyle.normal.textColor = comboColor;
                GUI.Label(new Rect(scoreX, comboY, 300, 30), $"★ COMBO x{GameManager.Instance.ComboCount}!", comboStyle);
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
                    fontSize = 17,
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
                fontSize = 32,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            scoreValStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(boxRect.x, boxRect.y + 14, boxRect.width, 40), $"{GameManager.Instance.CurrentScore:N0} PTS", scoreValStyle);

            GUIStyle statLine = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter
            };
            statLine.normal.textColor = new Color(0.7f, 0.8f, 0.95f);
            string comboStat = (GameManager.Instance.MaxCombo > 1) ? $"   |   MAX COMBO: x{GameManager.Instance.MaxCombo}" : "";
            GUI.Label(new Rect(boxRect.x, boxRect.y + 60, boxRect.width, 28), $"BEST: {GameManager.Instance.HighScore:N0} PTS{comboStat}", statLine);

            // 4. Action Buttons
            float btnW = 280f;
            float btnH = 50f;
            float btnX = midX - btnW * 0.5f;

            if (GUI.Button(new Rect(btnX, panelRect.y + 250, btnW, btnH), "PLAY AGAIN", buttonStyle))
            {
                GameManager.Instance.RestartGame();
            }

            if (GUI.Button(new Rect(btnX, panelRect.y + 318, btnW, btnH), "MAIN MENU", buttonStyle))
            {
                GameManager.Instance.OpenMainMenu();
            }
        }

        // ==========================================
        // HELPERS
        // ==========================================
        private void DrawFullScreenBackdrop(Color scrimColor)
        {
            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
            if (bgTex != null)
            {
                GUI.color = Color.white;
                GUI.DrawTexture(screenRect, bgTex, ScaleMode.ScaleAndCrop);
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
