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
        private GUIStyle inputFieldStyle;

        // Username Dialog State
        private bool showNameModal = false;
        private string inputPlayerName = "";
        private bool focusNameFieldNext = false;

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

            // If player hasn't entered a name yet, prompt them on the start menu
            if (GameManager.Instance != null && !GameManager.Instance.HasPlayerName && !showNameModal)
            {
                OpenNameModal();
            }

            float midX = Screen.width * 0.5f;

            // 2. High Score Ribbon Badge
            int best = (GameManager.Instance != null) ? GameManager.Instance.HighScore : 0;
            float badgeW = 280f;
            float badgeH = 34f;
            float badgeY = Screen.height * 0.53f;
            Rect badgeRect = new Rect(midX - badgeW * 0.5f, badgeY, badgeW, badgeH);

            GUI.color = new Color(0.04f, 0.06f, 0.12f, 0.90f);
            GUI.DrawTexture(badgeRect, Texture2D.whiteTexture);
            GUI.color = new Color(1.3f, 0.88f, 0.25f, 0.85f); // Solar Gold Rim
            DrawBorder(badgeRect, 1.5f);
            GUI.color = Color.white;

            GUIStyle badgeStyle = new GUIStyle(cardBodyStyle)
            {
                font = displayFont,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            badgeStyle.normal.textColor = new Color(1.3f, 0.88f, 0.25f);
            GUI.Label(badgeRect, $"★  RECORD: {best:N0} PTS  ★", badgeStyle);

            // 3. Runner Profile Tag (Clickable to Edit Name)
            float tagY = badgeY + 40f;
            float tagH = 38f;
            Rect tagRect = new Rect(midX - badgeW * 0.5f, tagY, badgeW, tagH);

            bool isTagHover = tagRect.Contains(Event.current.mousePosition) && !showNameModal;
            GUI.color = isTagHover ? new Color(0.12f, 0.16f, 0.26f, 0.95f) : new Color(0.04f, 0.06f, 0.12f, 0.90f);
            GUI.DrawTexture(tagRect, Texture2D.whiteTexture);
            GUI.color = isTagHover ? new Color(0.3f, 0.95f, 1.4f) : new Color(0.2f, 0.65f, 0.95f, 0.7f);
            DrawBorder(tagRect, 1.5f);
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
            tagTextStyle.normal.textColor = isTagHover ? Color.white : new Color(0.85f, 0.92f, 1f);
            GUI.Label(tagRect, $"👤 RUNNER: {runnerTag}  ✎", tagTextStyle);

            if (GUI.Button(tagRect, GUIContent.none, GUIStyle.none) && !showNameModal)
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                OpenNameModal();
            }

            // 4. Action Buttons (Clean, responsive pill buttons)
            float btnW = Mathf.Clamp(Screen.width * 0.26f, 260f, 320f);
            float btnH = 52f;
            float btnX = midX - btnW * 0.5f;
            float playBtnY = tagY + 48f;
            float howToBtnY = playBtnY + 60f;

            if (GUI.Button(new Rect(btnX, playBtnY, btnW, btnH), "PLAY", buttonStyle) && !showNameModal)
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                if (GameManager.Instance != null && !GameManager.Instance.HasPlayerName)
                {
                    OpenNameModal();
                }
                else
                {
                    GameManager.Instance.StartGame();
                }
            }

            if (GUI.Button(new Rect(btnX, howToBtnY, btnW, btnH), "HOW TO PLAY", buttonStyle) && !showNameModal)
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
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
            GUI.Label(new Rect(modalRect.x, modalRect.y + 64, modalRect.width, 22), "Choose your runner callsign for the leaderboard", subStyle);

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

            // 4. Action Buttons
            bool hasExistingName = GameManager.Instance != null && GameManager.Instance.HasPlayerName;
            float btnH = 46f;
            float btnY = modalRect.y + 180f;

            if (hasExistingName)
            {
                // Two buttons: CONFIRM (right) and CANCEL (left)
                float bW = 160f;
                float gap = 16f;
                float leftBtnX = midX - bW - gap * 0.5f;
                float rightBtnX = midX + gap * 0.5f;

                if (GUI.Button(new Rect(leftBtnX, btnY, bW, btnH), "CANCEL", buttonStyle))
                {
                    if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                    showNameModal = false;
                }

                if (GUI.Button(new Rect(rightBtnX, btnY, bW, btnH), "CONFIRM", buttonStyle))
                {
                    ConfirmPlayerName();
                }
            }
            else
            {
                // Single prominent CONFIRM button
                float bW = 220f;
                if (GUI.Button(new Rect(midX - bW * 0.5f, btnY, bW, btnH), "CONFIRM", buttonStyle))
                {
                    ConfirmPlayerName();
                }
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

            // 5. Action Buttons
            float btnW = 200f;
            float btnH = 48f;
            float bY = panelRect.y + panelRect.height - 64f;

            if (GUI.Button(new Rect(midX - btnW - 14, bY, btnW, btnH), "PLAY", buttonStyle))
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                GameManager.Instance.StartGame();
            }

            if (GUI.Button(new Rect(midX + 14, bY, btnW, btnH), "BACK", buttonStyle))
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
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

            // 4. CORNER PAUSE BUTTON (Top-Right)
            float pauseSize = 44f;
            Rect pauseRect = new Rect(Screen.width - pauseSize - 22f, 20f, pauseSize, pauseSize);

            // Interaction
            if (GUI.Button(pauseRect, GUIContent.none, GUIStyle.none))
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                GameManager.Instance.PauseGame();
            }

            bool isHover = pauseRect.Contains(Event.current.mousePosition);
            Color btnBg = isHover ? new Color(0.18f, 0.22f, 0.32f, 0.95f) : new Color(0.06f, 0.08f, 0.14f, 0.85f);
            GUI.color = btnBg;
            GUI.DrawTexture(pauseRect, Texture2D.whiteTexture);

            // Glow border on hover
            Color borderCol = isHover ? new Color(0.3f, 0.9f, 1.2f, 1f) : new Color(0.45f, 0.65f, 0.85f, 0.5f);
            GUI.color = borderCol;
            DrawBorder(pauseRect, 2f);

            // Draw crisp pause bars (||)
            Color barCol = isHover ? Color.white : new Color(0.9f, 0.95f, 1f, 0.95f);
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

            // 3. Action Buttons
            float btnW = 260f;
            float btnH = 46f;
            float btnX = midX - btnW * 0.5f;

            if (GUI.Button(new Rect(btnX, panelRect.y + 164, btnW, btnH), "RESUME", buttonStyle))
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                GameManager.Instance.ResumeGame();
            }

            if (GUI.Button(new Rect(btnX, panelRect.y + 224, btnW, btnH), "RESTART", buttonStyle))
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                GameManager.Instance.RestartGame();
            }

            if (GUI.Button(new Rect(btnX, panelRect.y + 284, btnW, btnH), "MAIN MENU", buttonStyle))
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
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

            string overRunner = (GameManager.Instance != null && GameManager.Instance.HasPlayerName) ? GameManager.Instance.PlayerName : "RUNNER";

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
                GUI.Label(new Rect(panelRect.x, panelRect.y + 92, panelRect.width, 28), $"★ NEW RECORD BY {overRunner.ToUpper()}! ★", recordStyle);
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
            GUI.Label(new Rect(boxRect.x, boxRect.y + 60, boxRect.width, 28), $"RUNNER: {overRunner}   |   BEST: {GameManager.Instance.HighScore:N0} PTS", statLine);

            // 4. Action Buttons
            float btnW = 280f;
            float btnH = 50f;
            float btnX = midX - btnW * 0.5f;

            if (GUI.Button(new Rect(btnX, panelRect.y + 250, btnW, btnH), "PLAY AGAIN", buttonStyle))
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                GameManager.Instance.RestartGame();
            }

            if (GUI.Button(new Rect(btnX, panelRect.y + 318, btnW, btnH), "MAIN MENU", buttonStyle))
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayButtonClick();
                GameManager.Instance.OpenMainMenu();
            }
        }

        // ==========================================
        // HELPERS
        // ==========================================
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
